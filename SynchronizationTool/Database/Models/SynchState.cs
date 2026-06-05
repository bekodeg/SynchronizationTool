namespace SynchronizationTool.Database.Models
{
    public class SynchState
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        public Guid ChangeLogId { get; set; }

        public SynchClient SynchClient { get; set; } = null!;

        public ChangeLog ChangeLog { get; set; } = null!;
    }
}
