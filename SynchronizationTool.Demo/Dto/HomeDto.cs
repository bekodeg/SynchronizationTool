namespace SynchronizationTool.Demo.Dto
{
    public class HomeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class HomeCreateUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
