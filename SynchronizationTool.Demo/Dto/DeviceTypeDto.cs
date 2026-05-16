namespace SynchronizationTool.Demo.Dto
{
    public class DeviceTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class DeviceTypeCreateUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
