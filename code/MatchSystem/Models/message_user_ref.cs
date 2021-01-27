namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.message_user_ref")]
    public partial class message_user_ref
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int MessageId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRead { get; set; }

        public virtual message message { get; set; }

        public virtual user user { get; set; }
    }
}
