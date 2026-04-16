using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.Models.Dto;

namespace SynchronizationTool.Logic.Models.Commads
{
    public record ApplyChangeLogsCommand : Command
    {
        public required IReadOnlyList<ChangeLogDto> ChangeLogs { get; init; }
    }
}
