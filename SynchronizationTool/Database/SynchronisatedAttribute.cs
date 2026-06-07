namespace SynchronizationTool.Database
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SynchronisatedAttribute : Attribute
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();    
    }
}
