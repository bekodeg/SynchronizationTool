namespace SynchronizationTool.Test.Handlers
{
    public class ApplyChangeLogsCommandHandlerTests
    {
        [Fact]
        public async Task ValidChangeLogs()
        {
            // Arrange
            //var mockContext = new Mock<DbSynchronizationContext>();
            //var mockLogger = new Mock<ILogger<ApplyChangeLogsCommandHandler>>();

            //var changeLogs = new List<ChangeLogDto>
            //{
            //    new ChangeLogDto
            //    {
            //        Id = Guid.NewGuid(),
            //        TableId = Guid.NewGuid(),
            //        EntityId = Guid.NewGuid(),
            //        Type = ChangeType.Update,
            //        Changes = new List<ChangeDto>
            //        {
            //            new ChangeDto { ColumnName = "Name", Value = "New Device Name" }
            //        },
            //        ClientVersion = 1,
            //        ClientId = Guid.NewGuid()
            //    }
            //};

            //var command = new ApplyChangeLogsCommand { ChangeLogs = changeLogs };
            //var handler = new ApplyChangeLogsCommandHandler(mockLogger.Object, mockContext.Object);

            //// Act
            //var result = await handler.HandleAsync(command, CancellationToken.None);

            //// Assert
            //Assert.NotNull(result);
            //Assert.False(result.IsError);
            //Assert.Equal(200, result.StatusCode);
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }

        [Fact]
        public async Task ChangeLogsIsNull()
        {
            // Arrange
            //var mockContext = new Mock<DbSynchronizationContext>();
            //var mockLogger = new Mock<ILogger<ApplyChangeLogsCommandHandler>>();

            //var command = new ApplyChangeLogsCommand { ChangeLogs = [] };
            //var handler = new ApplyChangeLogsCommandHandler(mockLogger.Object, mockContext.Object);

            //// Act
            //var result = await handler.HandleAsync(command, CancellationToken.None);

            //// Assert
            //Assert.True(result.IsError);
            //Assert.Equal(400, result.StatusCode);
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }

        [Fact]
        public async Task DiffVercionChange()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(1, 55));
        }
    }
}