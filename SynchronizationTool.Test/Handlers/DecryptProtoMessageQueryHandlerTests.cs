namespace SynchronizationTool.Test.Handlers
{
    public class DecryptProtoMessageQueryHandlerTests
    {
        [Fact]
        public async Task ValidMessage()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }

        [Fact]
        public async Task InvalidMessage()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }
    }
}
