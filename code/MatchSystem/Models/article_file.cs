namespace MatchSystem.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("matchsystem.article_file")]
    public partial class article_file
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public int FileId { get; set; }

        public virtual article article { get; set; }

        public virtual file file { get; set; }
    }
}
