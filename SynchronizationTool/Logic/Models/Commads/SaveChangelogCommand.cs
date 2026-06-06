using Protos;

namespace SynchronizationTool.Logic.Models.Commads
{
    public record SaveChangelogCommand : Command
    {
        public ChangeBucket RpcBucket { get; init; } = null!;
    }
}
