namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.match_file_ref")]
    public partial class match_file_ref
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int FileId { get; set; }

        public virtual file file { get; set; }

        public virtual match match { get; set; }
    }
}
