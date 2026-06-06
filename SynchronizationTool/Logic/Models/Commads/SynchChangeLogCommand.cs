namespace SynchronizationTool.Logic.Models.Commads
{
    public record SynchChangeLogCommand : Command
    {
        public Guid ClientId { get; set; }
    }
}
