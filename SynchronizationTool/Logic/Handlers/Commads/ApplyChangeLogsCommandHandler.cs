using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models.Enums;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;
using System.Globalization;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class ApplyChangeLogsCommandHandler : AbstractCommandHandler<ApplyChangeLogsCommand>
    {
        private readonly IDbSynchronizationContext _dbSynchronizationContext;
        private readonly ISynchronizationToolContext _synchronizationToolContext;
        private readonly SynchronisationConfiguration _synchronisationConfiguration;

        public ApplyChangeLogsCommandHandler(
            ILogger<ApplyChangeLogsCommandHandler> logger,
            IDbSynchronizationContext dbSynchronizationContext,
            ISynchronizationToolContext synchronizationToolContext,
            IOptions<SynchronisationConfiguration> options
        ) : base(logger)
        {
            _dbSynchronizationContext = dbSynchronizationContext;
            _synchronizationToolContext = synchronizationToolContext;
            _synchronisationConfiguration = options.Value;
        }

        public override async Task<ResponseModel> HandleAsync(ApplyChangeLogsCommand request, CancellationToken cancellationToken)
        {
            var changeTables = _synchronizationToolContext.ChangeLogs
                .Where(cl
                    => cl.Status == ChangeStatus.Pending
                    && cl.ClientId != _synchronisationConfiguration.ClientId)
                .Select(cl => new ChangeLogDto()
                {
                    DateTime = cl.DateTime,
                    TableId = cl.EntityId,
                    Type = cl.Type,
                    Id = cl.Id,
                    ClientVersion = cl.ClientVersion,
                    ClientId = cl.ClientId,
                    EntityId = cl.RowId,
                    Changes = cl.Changes.Select(x => new ChangeDto()
                    {
                        ColumnName = x.ColumnName,
                        Value = x.Value,
                    }).ToList(),
                })
                .GroupBy(cl => cl.TableId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var tableNames = await _synchronizationToolContext.SyncEntities
                .Where(t => changeTables.Keys.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Code, cancellationToken);

            var statusUpdates = new Dictionary<Guid, ChangeStatus>();

            foreach (var changeTable in changeTables)
            {
                if (!tableNames.TryGetValue(changeTable.Key, out var tableName))
                {
                    _logger.LogWarning("Таблица с ID {TableId} не найдена. Все изменения для неё помечаются как Failed.", changeTable.Key);
                    foreach (var log in changeTable.Value)
                        statusUpdates[log.Id] = ChangeStatus.Failed;
                    continue;
                }

                var tableResults = await ApplyChangeLog(changeTable.Value, tableName, cancellationToken);
                foreach (var (logId, status) in tableResults)
                    statusUpdates[logId] = status;
            }

            var changeLogsToUpdate = await _synchronizationToolContext.ChangeLogs
                .Where(cl => statusUpdates.Keys.Contains(cl.Id))
                .ToListAsync(cancellationToken);

            foreach (var log in changeLogsToUpdate)
            {
                if (statusUpdates.TryGetValue(log.Id, out var newStatus))
                    log.Status = newStatus;
            }

            await _synchronizationToolContext.SaveChangesAsync(cancellationToken);
            return new ResponseModel();
        }

        private async Task<Dictionary<Guid, ChangeStatus>> ApplyChangeLog(
            List<ChangeLogDto> changelog,
            string table,
            CancellationToken cancellationToken)
        {
            var results = new Dictionary<Guid, ChangeStatus>();

            var rows = changelog
                .GroupBy(cl => cl.EntityId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(e => e.DateTime).ToList()
                );

            var transaction = await _dbSynchronizationContext.BeginTransactionAsync(cancellationToken);

            try
            {
                Type? entityType = _dbSynchronizationContext.FindEntityType(table);
                if (entityType == null)
                {
                    _logger.LogError("Тип сущности для таблицы {Table} не найден.", table);
                    foreach (var log in changelog)
                        results[log.Id] = ChangeStatus.Failed;
                    return results;
                }

                var entityProperties = _dbSynchronizationContext
                    .FindEntityType(entityType)?
                    .GetProperties()
                    .ToDictionary(p => p.GetColumnName(), p => p)
                    ?? throw new KeyNotFoundException(table);

                foreach (var row in rows)
                {
                    var entityId = row.Key;
                    var orderedLogs = row.Value;

                    var lastLog = orderedLogs.Last();
                    if (lastLog.Type == ChangeType.Delete)
                    {
                        try
                        {
                            var entityToDelete = await _dbSynchronizationContext.FindAsync(entityType, new object[] { entityId }, cancellationToken);
                            if (entityToDelete != null)
                                _dbSynchronizationContext.Remove(entityToDelete);
                            else
                                _logger.LogWarning("Сущность {EntityId} для удаления не найдена в таблице {Table}.", entityId, table);

                            foreach (var log in orderedLogs)
                                results[log.Id] = ChangeStatus.Applied;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка удаления сущности {EntityId} из таблицы {Table}.", entityId, table);
                            foreach (var log in orderedLogs)
                                results[log.Id] = ChangeStatus.Failed;
                        }
                        continue;
                    }

                    // Загружаем или создаём сущность
                    object? entity;
                    try
                    {
                        entity = await _dbSynchronizationContext.FindAsync(entityType, new object[] { entityId }, cancellationToken);
                        if (entity == null)
                        {
                            entity = Activator.CreateInstance(entityType)!;
                            SetPrimaryKeyValue(entity, entityId, entityType);
                            await _dbSynchronizationContext.AddAsync(entity, cancellationToken);
                        }
                        else
                        {
                            var entry = _dbSynchronizationContext.Entry(entity);
                            if (entry.State == EntityState.Detached)
                                _dbSynchronizationContext.Attach(entity);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка инициализации сущности {EntityId} в таблице {Table}.", entityId, table);
                        foreach (var log in orderedLogs)
                            results[log.Id] = ChangeStatus.Failed;
                        continue;
                    }

                    // Словарь для отслеживания применённых колонок внутри пакета
                    var appliedColumns = new Dictionary<string, (ChangeLogDto Log, object? Value)>();
                    bool rowFailed = false;

                    foreach (var changeLog in orderedLogs)
                    {
                        if (changeLog.Type == ChangeType.Delete)
                        {
                            _logger.LogWarning("Delete-лог {LogId} не последний для сущности {EntityId}. Пропускается.", changeLog.Id, entityId);
                            results[changeLog.Id] = ChangeStatus.Applied;
                            continue;
                        }

                        try
                        {
                            bool logHasConflict = false;
                            foreach (var change in changeLog.Changes)
                            {
                                if (!entityProperties.TryGetValue(change.ColumnName, out var property))
                                {
                                    _logger.LogWarning("Колонка {ColumnName} отсутствует в сущности. Пропускается.", change.ColumnName);
                                    continue;
                                }

                                var propertyInfo = property.PropertyInfo;
                                if (propertyInfo == null) continue;

                                object? newValue = ConvertValue(change.Value, propertyInfo.PropertyType);

                                // Текущее значение свойства сущности в БД (до применения данного изменения)
                                object? currentDbValue = propertyInfo.GetValue(entity);

                                // Значение из этого же пакета, если колонка уже менялась ранее
                                object? previousBatchValue = appliedColumns.TryGetValue(change.ColumnName, out var prev) ? prev.Value : null;
                                var previousLog = prev.Log; // может быть null

                                // Вызов стратегии разрешения конфликтов с учётом состояния БД
                                var resolution = ResolveConflict(
                                    change.ColumnName,
                                    previousBatchValue,
                                    newValue,
                                    currentDbValue,
                                    changeLog,
                                    previousLog);

                                if (!resolution.IsResolved)
                                {
                                    _logger.LogError(
                                        "Невозможно разрешить конфликт колонки '{Column}' сущности {EntityId}. Лог {LogId} помечается как Failed.",
                                        change.ColumnName, entityId, changeLog.Id);
                                    results[changeLog.Id] = ChangeStatus.Failed;
                                    rowFailed = true;
                                    break;
                                }

                                // Применяем итоговое значение
                                propertyInfo.SetValue(entity, resolution.ResolvedValue);
                                appliedColumns[change.ColumnName] = (changeLog, resolution.ResolvedValue);

                                if (previousBatchValue != null || currentDbValue != null && !Equals(currentDbValue, resolution.ResolvedValue))
                                    logHasConflict = true;
                            }

                            if (rowFailed) break;

                            results[changeLog.Id] = ChangeStatus.Applied;
                            if (logHasConflict)
                                _logger.LogInformation("Конфликт(ы) в логе {LogId} сущности {EntityId} успешно разрешены.", changeLog.Id, entityId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка применения лога {LogId} для сущности {EntityId}.", changeLog.Id, entityId);
                            results[changeLog.Id] = ChangeStatus.Failed;
                            rowFailed = true;
                            break;
                        }
                    }

                    if (rowFailed)
                    {
                        foreach (var log in orderedLogs)
                        {
                            if (!results.ContainsKey(log.Id))
                                results[log.Id] = ChangeStatus.Failed;
                        }
                    }
                }

                await _dbSynchronizationContext.SaveChangesWithoutTrackingAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка обработки таблицы {Table}. Откат транзакции, все логи Failed.", table);
                await transaction.RollbackAsync(cancellationToken);
                foreach (var log in changelog)
                    results[log.Id] = ChangeStatus.Failed;
            }

            return results;
        }

        /// <summary>
        /// Разрешение конфликта между изменениями колонки.
        /// Учитывает:
        /// - предыдущее значение из текущего пакета (previousBatchValue),
        /// - предлагаемое новое значение (newValue),
        /// - текущее значение в базе данных (currentDbValue).
        /// </summary>
        /// <param name="columnName">Имя колонки</param>
        /// <param name="previousBatchValue">Значение, уже применённое к колонке в рамках текущей партии (null, если ещё не менялась)</param>
        /// <param name="newValue">Новое значение из обрабатываемого лога</param>
        /// <param name="currentDbValue">Текущее значение колонки в базе данных на момент обработки</param>
        /// <param name="currentLog">Текущий лог</param>
        /// <param name="previousLog">Лог, ранее изменивший эту колонку в пакете (null, если нет)</param>
        /// <returns>Результат разрешения с итоговым значением</returns>
        protected virtual ConflictResolutionResult ResolveConflict(
            string columnName,
            object? previousBatchValue,
            object? newValue,
            object? currentDbValue,
            ChangeLogDto currentLog,
            ChangeLogDto? previousLog)
        {
            // Базовая стратегия: Last Writer Wins – возвращаем newValue в любом случае.
            // Для более точной логики нужно учитывать OldValue.
            // Пример: если currentDbValue != previousBatchValue и previousBatchValue != newValue – это конфликт.
            return ConflictResolutionResult.Success(newValue);
        }

        private void SetPrimaryKeyValue(object entity, Guid keyValue, Type entityType)
        {
            var keyProperty = _dbSynchronizationContext
                .FindEntityType(entityType)?
                .FindPrimaryKey()?
                .Properties
                .FirstOrDefault();

            if (keyProperty?.PropertyInfo != null)
                keyProperty.PropertyInfo.SetValue(entity, keyValue);
            else
                throw new InvalidOperationException($"Не удалось найти свойство первичного ключа для типа {entityType.Name}");
        }

        private object? ConvertValue(string? rawValue, Type? targetType)
        {
            if (targetType == null || string.IsNullOrEmpty(rawValue))
                return null;

            if (targetType == typeof(string))
                return rawValue;

            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                return Convert.ChangeType(rawValue, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка преобразования значения '{Value}' в тип {Type}.", rawValue, targetType.Name);
                return null;
            }
        }

        protected class ConflictResolutionResult
        {
            public bool IsResolved { get; }
            public object? ResolvedValue { get; }

            private ConflictResolutionResult(bool isResolved, object? resolvedValue)
            {
                IsResolved = isResolved;
                ResolvedValue = resolvedValue;
            }

            public static ConflictResolutionResult Success(object? value) => new(true, value);
            public static ConflictResolutionResult Failure() => new(false, null);
        }
    }
}