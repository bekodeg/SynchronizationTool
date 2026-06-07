using System.ComponentModel.DataAnnotations;

namespace SynchronizationTool.Database.Models
{
    public class SynchClient
    {
        public Guid Id { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        public Guid? LastChangeLogId { get; set; }
    }
}
