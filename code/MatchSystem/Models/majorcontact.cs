namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.majorcontact")]
    public partial class majorcontact
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? MajorId { get; set; }

        public int? SchoolId { get; set; }

        [Required]
        [StringLength(64)]
        public string Job { get; set; }

        public virtual major major { get; set; }

        public virtual school school { get; set; }

        public virtual user user { get; set; }
    }
}
