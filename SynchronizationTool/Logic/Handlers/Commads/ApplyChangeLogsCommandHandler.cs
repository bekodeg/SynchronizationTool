using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models.Enums;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class ApplyChangeLogsCommandHandler(
        ILogger<ApplyChangeLogsCommandHandler> logger,
        DbSynchronizationContext dbSynchronizationContext
        ) : AbstractCommandHandler<ApplyChangeLogsCommand>(logger)
    {
        private readonly DbSynchronizationContext _dbSynchronizationContext = dbSynchronizationContext;

        public override async Task<ResponseModel> HandleAsync(ApplyChangeLogsCommand request, CancellationToken cancellationToken)
        {
            var changeTables = request.ChangeLogs
                .GroupBy(cl => cl.EntityId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var tableNames = await _dbSynchronizationContext.SyncEntities
                .Where(t => changeTables.Keys.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Code, cancellationToken);

            foreach (var changeTable in changeTables)
            {
                if (tableNames.TryGetValue(changeTable.Key, out var tableName))
                {
                    await ApplyChangeLog(changeTable.Value, tableNames[changeTable.Key]);
                }
                else
                {
                    _logger.LogWarning("Таблица с ID {TableId} не найдена в базе данных. Пропускаем изменения для этой таблицы.", changeTable.Key);
                }
            }

            return new();
        }

        private async Task ApplyChangeLog(List<ChangeLogDto> changelog, string table)
        {
            var rows = changelog.GroupBy(cl => cl.EntityId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(e => e.DateTime).ToList());

            Type? entityType = _dbSynchronizationContext.Model
                .GetEntityTypes()
                .FirstOrDefault(et => et.GetTableName() == table)?.ClrType;

            if (entityType == null)
            {
                _logger.LogError("Entity type for table {Table} not found.", table);
                return;
            }

            // Получаем метаданные EF Core для сопоставления колонок и свойств
            var entityProperties = _dbSynchronizationContext.Model
                .FindEntityType(entityType)?
                .GetProperties()
                .ToDictionary(p => p.GetColumnName(), p => p)!;

            foreach (var row in rows)
            {
                var latestChange = row.Value.First();
                
                var entity = await _dbSynchronizationContext.FindAsync(entityType, [row.Key]);

                if (latestChange.Type == ChangeType.Delete)
                {
                    // Удаление записи    
                    if (entity != null)
                    {
                        _dbSynchronizationContext.Remove(entity);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Сущность с ID {EntityId} не найдена для удаления в таблице {Table}.",
                            row.Key,
                            table
                        );
                    }
                    continue;
                }

                // Если запись не найдена (или это Insert), создаём новую
                if (entity == null)
                {
                    entity = Activator.CreateInstance(entityType)!;
                    SetPrimaryKeyValue(entity, row.Key, entityType);
                    _dbSynchronizationContext.Add(entity);
                }
                else
                {
                    // Для Update прикрепляем сущность к контексту, если она ещё не отслеживается
                    var entry = _dbSynchronizationContext.Entry(entity);
                    if (entry.State == EntityState.Detached)
                    {
                        _dbSynchronizationContext.Attach(entity);
                    }
                }

                var finalChanges = row.Value
                    .SelectMany(log => log.Changes)
                    .GroupBy(change => change.ColumnName)
                    .ToDictionary(g => g.Key, g => g.First().Value); // Берём последнее изменение для каждой колонки

                foreach (var log in row.Value)
                {
                    foreach (var change in log.Changes)
                    {
                        // Перезаписываем значение – в конце останется самое позднее
                        finalChanges[change.ColumnName] = change.Value;
                    }
                }

                // Применяем изменения к свойствам сущности
                ApplyChangesToEntity(entity, latestChange.Changes, entityProperties);
            }
        }

        /// <summary>
        /// Устанавливает значение первичного ключа для сущности.
        /// Предполагается, что ключ один и имеет тип Guid.
        /// </summary>
        private void SetPrimaryKeyValue(object entity, Guid keyValue, Type entityType)
        {
            var keyProperty = _dbSynchronizationContext.Model
                .FindEntityType(entityType)?
                .FindPrimaryKey()?
                .Properties
                .FirstOrDefault();

            if (keyProperty?.PropertyInfo != null)
            {
                keyProperty.PropertyInfo.SetValue(entity, keyValue);
            }
            else
            {
                throw new InvalidOperationException($"Не удалось найти свойство первичного ключа для типа {entityType.Name}");
            }
        }

        /// <summary>
        /// Применяет список изменений к объекту сущности.
        /// </summary>
        private void ApplyChangesToEntity(
            object entity,
            IReadOnlyList<ChangeDto> changes,
            Dictionary<string, IProperty> entityProperties)
        {
            foreach (var change in changes)
            {
                if (!entityProperties.TryGetValue(change.ColumnName, out var property))
                {
                    _logger.LogWarning("Колонка {ColumnName} не найдена в сущности. Пропускаем.", change.ColumnName);
                    continue;
                }

                var propertyInfo = property.PropertyInfo;
                if (propertyInfo == null)
                    continue;

                // Преобразуем строковое значение в тип свойства
                object? convertedValue = ConvertValue(change.Value, propertyInfo.PropertyType);
                propertyInfo.SetValue(entity, convertedValue);

                // При необходимости можно явно пометить свойство как изменённое (если используется Attach)
                // var entry = _dbSynchronizationContext.Entry(entity);
                // entry.Property(property.Name).IsModified = true;
            }
        }

        /// <summary>
        /// Конвертирует строковое представление значения в заданный тип.
        /// Обрабатывает NULL, Guid, DateTime, числа и т.д.
        /// </summary>
        private object? ConvertValue(string? rawValue, Type targetType)
        {
            // Обработка NULL
            if (string.IsNullOrEmpty(rawValue))
            {
                return null;
            }

            // Если целевой тип уже string — возвращаем как есть
            if (targetType == typeof(string))
                return rawValue;

            // Для Nullable<T> получаем базовый тип
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                // Используем Convert.ChangeType с инвариантной культурой
                return Convert.ChangeType(rawValue, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка преобразования значения '{Value}' в тип {Type}",
                    rawValue,
                    targetType.Name);
                return null;
            }
        }
    }
}
