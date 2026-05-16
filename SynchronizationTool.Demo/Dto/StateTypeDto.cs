namespace SynchronizationTool.Demo.Dto
{
    public class StateTypeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string ChangeCommand { get; set; } = null!;
        public Guid DeviceTypeId { get; set; }
    }
    public class StateTypeCreateUpdateDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string ChangeCommand { get; set; } = null!;
        public Guid DeviceTypeId { get; set; }
    }
}
