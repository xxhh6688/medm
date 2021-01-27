namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.paper_file_ref")]
    public partial class paper_file_ref
    {
        public int Id { get; set; }

        public int PaperId { get; set; }

        public int FileId { get; set; }

        public int Type { get; set; }

        public DateTime CreateTime { get; set; }

        public virtual file file { get; set; }

        public virtual paper paper { get; set; }
    }
}
