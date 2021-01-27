namespace MatchSystem.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Ctx : DbContext
    {
        public Ctx()
            : base("name=Ctx")
        {
        }

        public virtual DbSet<article> article { get; set; }
        public virtual DbSet<article_file> article_file { get; set; }
        public virtual DbSet<expert> expert { get; set; }
        public virtual DbSet<file> file { get; set; }
        public virtual DbSet<major> major { get; set; }
        public virtual DbSet<majorcontact> majorcontact { get; set; }
        public virtual DbSet<match> match { get; set; }
        public virtual DbSet<match_file_ref> match_file_ref { get; set; }
        public virtual DbSet<match_question_ref> match_question_ref { get; set; }
        public virtual DbSet<message> message { get; set; }
        public virtual DbSet<message_user_ref> message_user_ref { get; set; }
        public virtual DbSet<paper> paper { get; set; }
        public virtual DbSet<paper_file_ref> paper_file_ref { get; set; }
        public virtual DbSet<paper_review_expert> paper_review_expert { get; set; }
        public virtual DbSet<question> question { get; set; }
        public virtual DbSet<school> school { get; set; }
        public virtual DbSet<token> token { get; set; }
        public virtual DbSet<user> user { get; set; }
        public virtual DbSet<user_security> user_security { get; set; }
        public virtual DbSet<downloadcenter> downloadcenter { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<article>()
                .Property(e => e.Title)
                .IsUnicode(false);

            modelBuilder.Entity<article>()
                .Property(e => e.Content)
                .IsUnicode(false);

            modelBuilder.Entity<article>()
                .HasMany(e => e.article_file)
                .WithRequired(e => e.article)
                .HasForeignKey(e => e.ArticleId);

            modelBuilder.Entity<expert>()
                .Property(e => e.Company)
                .IsUnicode(false);

            modelBuilder.Entity<expert>()
                .Property(e => e.SchoolName)
                .IsUnicode(false);

            modelBuilder.Entity<expert>()
                .Property(e => e.ResearchArea)
                .IsUnicode(false);

            modelBuilder.Entity<expert>()
                .Property(e => e.ReviewArea)
                .IsUnicode(false);

            modelBuilder.Entity<file>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<file>()
                .Property(e => e.Path)
                .IsUnicode(false);

            modelBuilder.Entity<file>()
                .Property(e => e.Token)
                .IsUnicode(false);

            modelBuilder.Entity<file>()
                .HasMany(e => e.user1)
                .WithOptional(e => e.file1)
                .HasForeignKey(e => e.ImageId);

            modelBuilder.Entity<file>()
                .HasMany(e => e.article_file)
                .WithRequired(e => e.file)
                .HasForeignKey(e => e.FileId);

            modelBuilder.Entity<file>()
                .HasMany(e => e.downloadcenter)
                .WithRequired(e => e.file)
                .HasForeignKey(e => e.FileId);

            modelBuilder.Entity<major>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<majorcontact>()
                .Property(e => e.Job)
                .IsUnicode(false);

            modelBuilder.Entity<match>()
                .Property(e => e.Title)
                .IsUnicode(false);

            modelBuilder.Entity<match>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<match>()
                .Property(e => e.Award)
                .IsUnicode(false);

            modelBuilder.Entity<message>()
                .Property(e => e.Title)
                .IsUnicode(false);

            modelBuilder.Entity<message>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<message>()
                .Property(e => e.ToUserId)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.StudentName)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.IdCardNumber)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.Academy)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.StudentCellPhone)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.StudentMail)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.Title)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.EnglishTitle)
                .IsUnicode(false);

            modelBuilder.Entity<paper>()
                .Property(e => e.Abstract)
                .IsUnicode(false);

            modelBuilder.Entity<paper_review_expert>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<question>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<question>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<question>()
                .Property(e => e.Industry)
                .IsUnicode(false);

            modelBuilder.Entity<question>()
                .Property(e => e.FromCompany)
                .IsUnicode(false);

            modelBuilder.Entity<question>()
                .Property(e => e.DesignRequirement)
                .IsUnicode(false);

            modelBuilder.Entity<school>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<token>()
                .Property(e => e.Token1)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.CellPhone)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.Mail)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.article)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.file)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.match)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.match1)
                .WithOptional(e => e.user1)
                .HasForeignKey(e => e.UpdateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.message)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.paper)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.paper_review_expert)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.ExpertId);

            modelBuilder.Entity<user>()
                .HasMany(e => e.question)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreateBy);

            modelBuilder.Entity<user>()
                .HasMany(e => e.question1)
                .WithOptional(e => e.user1)
                .HasForeignKey(e => e.UpdateBy);

            modelBuilder.Entity<user_security>()
                .Property(e => e.Password)
                .IsUnicode(false);
        }
    }
}
