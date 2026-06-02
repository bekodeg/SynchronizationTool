namespace IntegrationTest
{
    public class IntegrationTests
    {
        [Fact]
        public async Task TestInsertAsync()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(10, 105));
        }

        [Fact]
        public async Task TestUpdateAsync()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(10, 105));
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(10, 105));
        }

        [Fact]
        public async Task TestConfictePAsync()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(20, 505));
        }

        [Fact]
        public async Task TestConficteNAsync()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(30, 505));
        }
    }
}
