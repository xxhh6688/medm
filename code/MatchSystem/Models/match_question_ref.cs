namespace MatchSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("matchsystem.match_question_ref")]
    public partial class match_question_ref
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int QuestionId { get; set; }

        public virtual match match { get; set; }

        public virtual question question { get; set; }
    }
}
