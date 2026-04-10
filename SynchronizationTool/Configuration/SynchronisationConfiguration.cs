namespace SynchronizationTool.Configuration
{
    public record SynchronisationConfiguration
    {
        public Guid ClientId { get; init; } = Guid.NewGuid();

        public int CurrentClientVersion { get; init; } = 1;

        public string SynchSchema { get; init; } = "sync";
    }
}
