using Protos;

namespace SynchronizationTool.Logic.Models.Queries
{
    public record ChangeLogQuery : Query<ChangeBucket>
    {
        public Guid ClientId { get; set; }
    }
}
