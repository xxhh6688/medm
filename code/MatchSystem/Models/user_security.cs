namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.user_security")]
    public partial class user_security
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }

        public int UserId { get; set; }
    }
}
