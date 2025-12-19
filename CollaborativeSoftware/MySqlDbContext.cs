using System;
using Microsoft.EntityFrameworkCore;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware.Data
{
    /// <summary>
    /// MySQL Database Context for Quiz Platform
    /// Connects to WS322254_QuizelYang database
    /// </summary>
    public class MySqlDbContext : DbContext
    {
    // Connection string for MySQL database
        private const string ConnectionString =
   "Server=Plesk.remote.ac;Port=3306;Database=WS322254_QuizelYang;User=WS322254_Collab;Password=Admin@Root1!;SslMode=Preferred;";

   // DbSets for all tables
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<StudentManagementRecord> StudentManagements { get; set; } = null!;
        public DbSet<Lecturer> Lecturers { get; set; } = null!;
        public DbSet<SubjectEntity> Subjects { get; set; } = null!;
        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<QuestionEntity> Questions { get; set; } = null!;
        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<TestAttempt> TestAttempts { get; set; } = null!;
        public DbSet<StudentAnswerEntity> StudentAnswers { get; set; } = null!;
        public DbSet<LoginAudit> LoginAudits { get; set; } = null!;

        public MySqlDbContext()
        {
        }

        public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                // Configure MySQL connection using Pomelo
                options.UseMySql(ConnectionString, 
                   ServerVersion.AutoDetect(ConnectionString),
                 mySqlOptions =>
                      {
                    mySqlOptions.EnableRetryOnFailure(
                           maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                          errorNumbersToAdd: null);
                         });
            }
        }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

         // Configure User (updated to match actual MySQL schema)
        modelBuilder.Entity<User>(entity =>
 {
     entity.ToTable("User");
       entity.HasKey(e => e.UserId);
         entity.Property(e => e.UserId).HasColumnName("UserID");
         entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100);
        entity.Property(e => e.Role).HasColumnName("Role").HasMaxLength(20);
         entity.Property(e => e.PasswordHash).HasColumnName("Password").HasMaxLength(255);
           entity.Property(e => e.IsActive).HasColumnName("IsActive");
  entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
  });

     // Configure StudentManagement (full student records)
 modelBuilder.Entity<StudentManagementRecord>(entity =>
          {
  entity.ToTable("StudentManagement");
         entity.HasKey(e => e.StudentId);
      entity.Property(e => e.StudentId).HasColumnName("StudentID");
      entity.Property(e => e.StudentNumber).HasColumnName("StudentNumber").HasMaxLength(10);
    entity.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(50);
     entity.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(50);
          entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100);
       entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255);
        entity.Property(e => e.CourseTitle).HasColumnName("CourseTitle").HasMaxLength(100);
 entity.Property(e => e.IsApproved).HasColumnName("IsApproved");
    entity.Property(e => e.IsActive).HasColumnName("IsActive");
       entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        entity.Property(e => e.LastLoginTime).HasColumnName("LastLoginTime");
 });

     // Configure Lecturer
    modelBuilder.Entity<Lecturer>(entity =>
     {
     entity.ToTable("Lecturer");
        entity.HasKey(e => e.LecturerId);
           entity.Property(e => e.LecturerId).HasColumnName("LecturerID");
    entity.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(50);
        entity.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(50);
  entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100);
entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255);
       entity.Property(e => e.IsAdmin).HasColumnName("IsAdmin");
       entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
       // Note: IsActive column doesn't exist in database, so we ignore it
       entity.Ignore(e => e.IsActive);
   });

         // Configure Subject
         modelBuilder.Entity<SubjectEntity>(entity =>
  {
        entity.ToTable("Subject");
   entity.HasKey(e => e.SubjectId);
 entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
        entity.Property(e => e.SubjectName).HasColumnName("SubjectName").HasMaxLength(100);
       entity.Property(e => e.IsActive).HasColumnName("IsActive");
  });

     // Configure Test (updated to match actual MySQL schema)
       modelBuilder.Entity<Test>(entity =>
   {
  entity.ToTable("Test");
         entity.HasKey(e => e.TestId);
    entity.Property(e => e.TestId).HasColumnName("TestID");
     entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
     entity.Property(e => e.LecturerId).HasColumnName("LecturerID");
       entity.Property(e => e.TestName).HasColumnName("TestName").HasMaxLength(100);
       entity.Property(e => e.NumQuestions).HasColumnName("NumQuestions");
      entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

         entity.HasOne(e => e.Subject)
   .WithMany(s => s.Tests)
  .HasForeignKey(e => e.SubjectId);

      entity.HasOne(e => e.Lecturer)
    .WithMany(l => l.Tests)
   .HasForeignKey(e => e.LecturerId);
            });

        // Configure Question
            modelBuilder.Entity<QuestionEntity>(entity =>
       {
     entity.ToTable("Question");
      entity.HasKey(e => e.QuestionId);
    entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
        entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
       entity.Property(e => e.QuestionText).HasColumnName("QuestionText");
   entity.Property(e => e.OptionA).HasColumnName("OptionA").HasMaxLength(255);
            entity.Property(e => e.OptionB).HasColumnName("OptionB").HasMaxLength(255);
  entity.Property(e => e.OptionC).HasColumnName("OptionC").HasMaxLength(255);
         entity.Property(e => e.OptionD).HasColumnName("OptionD").HasMaxLength(255);
     entity.Property(e => e.CorrectAnswer).HasColumnName("CorrectAnswer").HasMaxLength(1);

   entity.HasOne(e => e.Subject)
     .WithMany(s => s.Questions)
    .HasForeignKey(e => e.SubjectId);
  });

     // Configure Answer
   modelBuilder.Entity<Answer>(entity =>
        {
      entity.ToTable("Answer");
       entity.HasKey(e => e.AnswerId);
         entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
         entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
    entity.Property(e => e.OptionLabel).HasColumnName("OptionLabel").HasMaxLength(1);
      entity.Property(e => e.AnswerText).HasColumnName("AnswerText").HasMaxLength(255);
      entity.Property(e => e.IsCorrect).HasColumnName("IsCorrect");
      entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

   entity.HasOne(e => e.Question)
        .WithMany()
      .HasForeignKey(e => e.QuestionId);
      });

      // Configure TestAttempt (updated to match actual MySQL schema)
            modelBuilder.Entity<TestAttempt>(entity =>
  {
           entity.ToTable("TestAttempt");
     entity.HasKey(e => e.AttemptId);
   entity.Property(e => e.AttemptId).HasColumnName("AttemptID");
         entity.Property(e => e.StudentId).HasColumnName("StudentID");
         entity.Property(e => e.TestId).HasColumnName("TestID");
       entity.Property(e => e.ScorePercentage).HasColumnName("ScorePercentage").HasColumnType("decimal(10,0)");
   entity.Property(e => e.TotalPoints).HasColumnName("TotalPoints");
       entity.Property(e => e.AttemptDate).HasColumnName("AttemptDate");

   entity.HasOne(e => e.Student)
     .WithMany(u => u.TestAttempts)
        .HasForeignKey(e => e.StudentId);

  entity.HasOne(e => e.Test)
       .WithMany(t => t.TestAttempts)
   .HasForeignKey(e => e.TestId);
 });

        // Configure StudentAnswer
    modelBuilder.Entity<StudentAnswerEntity>(entity =>
 {
   entity.ToTable("StudentAnswer");
          entity.HasKey(e => e.StudentAnswerId);
         entity.Property(e => e.StudentAnswerId).HasColumnName("StudentAnswerID");
    entity.Property(e => e.AttemptId).HasColumnName("AttemptID");
          entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
   entity.Property(e => e.SelectedAnswer).HasColumnName("SelectedAnswer").HasMaxLength(1);
        entity.Property(e => e.IsCorrect).HasColumnName("IsCorrect");
 entity.Property(e => e.AnsweredAt).HasColumnName("AnsweredAt");
           entity.Property(e => e.PointsEarned).HasColumnName("PointsEarned");

           entity.HasOne(e => e.TestAttempt)
      .WithMany(ta => ta.StudentAnswers)
     .HasForeignKey(e => e.AttemptId);

     entity.HasOne(e => e.Question)
     .WithMany()
       .HasForeignKey(e => e.QuestionId);
       });

     // Configure LoginAudit
  modelBuilder.Entity<LoginAudit>(entity =>
   {
      entity.ToTable("LoginAudit");
       entity.HasKey(e => e.AuditId);
         entity.Property(e => e.AuditId).HasColumnName("AuditID");
           entity.Property(e => e.UserId).HasColumnName("UserID");
   entity.Property(e => e.Role).HasColumnName("Role").HasMaxLength(20);
     entity.Property(e => e.Success).HasColumnName("Success");
      entity.Property(e => e.Timestamp).HasColumnName("Timestamp");
   });
        }
  }
}
