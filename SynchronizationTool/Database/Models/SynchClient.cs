using System.ComponentModel.DataAnnotations;

namespace SynchronizationTool.Database.Models
{
    public class SynchClient
    {
        public Guid Id { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        public List<ChangeLog> ChangeLogs { get; init; } = [];

        public List<SynchState> SynchStates { get; init; } = [];
    }
}
