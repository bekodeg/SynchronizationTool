namespace SynchronizationTool.Logic.Models.Dto
{
    public record ChangeDto
    {
        public required string ColumnName { get; init; }
        public required string? Value { get; init; } // "NULL" или реальное строковое представление
    }
}
