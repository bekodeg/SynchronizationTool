namespace SynchronizationTool.Logic.Models.Commads
{
    public record SendChangeLogCommand : Command
    {
        public Guid ClientId { get; set; }
    }
}
