using SynchronizationTool.Database.Context;

namespace SynchronizationTool.Logic.Models.Commads
{
    public record TrackingChangesCommand : Command
    {
        public DbSynchronizationContext Context { get; init; } = null!;
    }
}
