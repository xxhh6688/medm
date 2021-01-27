namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.match")]
    public partial class match
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public match()
        {
            match_file_ref = new HashSet<match_file_ref>();
            match_question_ref = new HashSet<match_question_ref>();
            paper = new HashSet<paper>();
        }

        public int Id { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Description { get; set; }

        public DateTime RegisterStartTime { get; set; }

        public DateTime RegisterEndTime { get; set; }

        public DateTime ReviewStartTime { get; set; }

        public DateTime ReviewEndTime { get; set; }

        public DateTime Review1StartTime { get; set; }

        public DateTime Review1EndTime { get; set; }

        public DateTime Review2StartTime { get; set; }

        public DateTime Review2EndTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? CreateBy { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public int? UpdateBy { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Award { get; set; }

        public virtual user user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<match_file_ref> match_file_ref { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<match_question_ref> match_question_ref { get; set; }

        public virtual user user1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<paper> paper { get; set; }
    }
}
