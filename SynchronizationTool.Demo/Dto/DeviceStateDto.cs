namespace SynchronizationTool.Demo.Dto
{
    public class DeviceStateDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public Guid StateTypeId { get; set; }
        public string Value { get; set; } = null!;
    }
    public class DeviceStateCreateUpdateDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public Guid StateTypeId { get; set; }
        public string Value { get; set; } = null!;
    }
}
