namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.expert")]
    public partial class expert
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Gender { get; set; }

        [StringLength(256)]
        public string SchoolName { get; set; }

        [StringLength(256)]
        public string ResearchArea { get; set; }

        [StringLength(256)]
        public string ReviewArea { get; set; }

        [StringLength(256)]
        public string Company { get; set; }

        [StringLength(256)]
        public string Academy { get; set; }

        [StringLength(45)]
        public string Birthday { get; set; }

        [StringLength(256)]
        public string BankId { get; set; }

        [StringLength(45)]
        public string Phone { get; set; }

        [StringLength(45)]
        public string Title { get; set; }

        [StringLength(45)]
        public string Major { get; set; }

        [StringLength(45)]
        public string Degree { get; set; }

        public virtual user user { get; set; }
    }
}
