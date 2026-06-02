namespace SynchronizationTool.Test.Handlers
{
    public class EncryptProtoMessageQueryHandler
    {
        [Fact]
        public async Task ValidMessage()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }
    }
}
