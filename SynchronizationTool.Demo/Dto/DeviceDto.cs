namespace SynchronizationTool.Demo.Dto
{
    public class DeviceDto
    {
        public Guid Id { get; set; }
        public string MqttCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Guid DeviceTypeId { get; set; }
        public Guid HomeId { get; set; }
    }

    public class DeviceCreateUpdateDto
    {
        public Guid Id { get; set; } // клиент предоставляет Guid
        public string MqttCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Guid DeviceTypeId { get; set; }
        public Guid HomeId { get; set; }
    }
}
