namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.token")]
    public partial class token
    {
        public int Id { get; set; }

        [Column("Token")]
        [Required]
        [StringLength(32)]
        public string Token1 { get; set; }

        public int UserId { get; set; }

        public virtual user user { get; set; }
    }
}
