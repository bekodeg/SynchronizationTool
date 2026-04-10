namespace SynchronizationTool.Database.Models
{
    public class Entity
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public ICollection<ChangeLog> ChangeLogs { get; set; } = [];
    }
}
