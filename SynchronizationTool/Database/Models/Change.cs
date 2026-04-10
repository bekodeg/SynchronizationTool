namespace SynchronizationTool.Database.Models
{
    public class Change
    {
        public int Id { get; set; }
        public string ColumnName { get; set; } = null!;
        public string Value { get; set; } = null!;
        public Guid ChangeLogId { get; set; }
        public ChangeLog ChangeLog { get; set; } = null!;
    }
}
