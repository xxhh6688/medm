namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.paper")]
    public partial class paper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public paper()
        {
            paper_file_ref = new HashSet<paper_file_ref>();
            paper_review_expert = new HashSet<paper_review_expert>();
        }

        public int Id { get; set; }

        public int? MatchId { get; set; }

        public int? QuestionId { get; set; }

        public int? CreateBy { get; set; }

        [Required]
        [StringLength(256)]
        public string StudentName { get; set; }

        public string Gender { get; set; }

        [StringLength(32)]
        public string IdCardNumber { get; set; }

        public int? SchoolId { get; set; }

        public int? MajorId { get; set; }

        [StringLength(256)]
        public string Academy { get; set; }

        [StringLength(32)]
        public string StudentCellPhone { get; set; }

        [StringLength(256)]
        public string StudentMail { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        [StringLength(256)]
        public string EnglishTitle { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Abstract { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Teachers { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string DesignRequirement { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public int CurrentStatus { get; set; }

        public int Type { get; set; }
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public double Score3 { get; set; }
        
        public virtual major major { get; set; }

        public virtual match match { get; set; }

        public virtual user user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<paper_file_ref> paper_file_ref { get; set; }

        public virtual question question { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<paper_review_expert> paper_review_expert { get; set; }

        public virtual school school { get; set; }
    }
}
