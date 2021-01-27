using System.ComponentModel.DataAnnotations.Schema;

namespace MatchSystem.Models
{
    [Table("matchsystem.downloadcenter")]
    public partial class downloadcenter
    {
        public int Id { get; set; }

        public int FileId { get; set; }

        public virtual file file { get; set; }
    }
}