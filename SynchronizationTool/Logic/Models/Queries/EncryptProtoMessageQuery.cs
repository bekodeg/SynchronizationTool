using Google.Protobuf;

namespace SynchronizationTool.Logic.Models.Queries
{
    public record EncryptProtoMessageQuery : Query<byte[]>
    {
        public IMessage Message { get; init; } = null!;
    }
}
