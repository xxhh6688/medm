namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.question")]
    public partial class question
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public question()
        {
            match_question_ref = new HashSet<match_question_ref>();
            paper = new HashSet<paper>();
        }

        public int Id { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Description { get; set; }

        [Required]
        [StringLength(256)]
        public string Support { get; set; }

        [Required]
        [StringLength(256)]
        public string Industry { get; set; }

        [Required]
        [StringLength(256)]
        public string FromCompany { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string DesignRequirement { get; set; }

        public string SerialNumber { get; set; }

        public int? CreateBy { get; set; }

        public int? UpdateBy { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<match_question_ref> match_question_ref { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<paper> paper { get; set; }

        public virtual user user { get; set; }

        public virtual user user1 { get; set; }
    }
}
