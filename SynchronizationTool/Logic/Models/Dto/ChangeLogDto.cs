using SynchronizationTool.Database.Models.Enums;

namespace SynchronizationTool.Logic.Models.Dto
{
    public record ChangeLogDto
    {
        public Guid? Id { get; init; }
        public DateTime DateTime { get; init; }
        public required Guid TableId { get; init; } 
        public required Guid EntityId { get; init; } 
        public required ChangeType Type { get; init; } // Insert/Update/Delete
        public required IReadOnlyList<ChangeDto> Changes { get; init; }
        public int? ClientVersion { get; init; }
        public Guid? ClientId { get; init; }
    }
}
