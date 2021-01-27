namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.paper_review_expert")]
    public partial class paper_review_expert
    {
        public int Id { get; set; }

        public int PaperId { get; set; }

        public int? ExpertId { get; set; }

        public int StatusType { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime Updatetime { get; set; }

        public int Score { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Comment { get; set; }

        public virtual paper paper { get; set; }

        public virtual user user { get; set; }
    }
}
