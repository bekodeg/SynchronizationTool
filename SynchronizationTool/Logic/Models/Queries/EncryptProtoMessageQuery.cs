using Google.Protobuf;

namespace SynchronizationTool.Logic.Models.Commads
{
    public record EncryptProtoMessageQuery : Query<byte[]>
    {
        public IMessage Message { get; init; } = null!;
    }
}
