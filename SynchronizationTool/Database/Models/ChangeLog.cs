using SynchronizationTool.Database.Models.Enums;

namespace SynchronizationTool.Database.Models
{
    public class ChangeLog
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public ChangeType Type { get; set; }
        public ChangeStatus Status { get; set; }
        public Guid EntityId { get; set; }

        public Guid RowId { get; set; }

        public Guid ClientId { get; set; }

        public int ClientVersion { get; set; }

        public Entity Entity { get; set; } = null!;
        //public Client Client { get; set; }
        public ICollection<Change> Changes { get; set; } = [];
        //public ICollection<ChangeLogConflict> ChangeLogConflicts { get; set; }
    }
}
