namespace SynchronizationTool.Test.Handlers
{
    public class TrackingChangesHandlerTests
    {
        [Fact]
        public async Task ValidTracked()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }
    }
}
