using Google.Protobuf;

namespace SynchronizationTool.Logic.Models.Queries
{
    public record DecryptProtoMessageQuery : Query<IMessage>
    {
        public byte[] Message { get; init; } = null!;
    }
}
