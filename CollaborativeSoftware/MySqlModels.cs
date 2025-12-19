using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollaborativeSoftware.Models
{
    /// <summary>
  /// Maps to MySQL 'User' table - Users with Role enum
    /// </summary>
    [Table("User")]
    public class User
    {
        [Key]
        [Column("UserID")]
        public int UserId { get; set; }

      [Column("Email")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

    [Column("Role")]
        [MaxLength(20)]
        public string Role { get; set; } = "Student"; // 'Student', 'Lecturer', 'Admin'

        [Column("Password")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

 [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
    }

    /// <summary>
    /// Maps to MySQL 'StudentManagement' table - Full student records with more details
    /// </summary>
    [Table("StudentManagement")]
    public class StudentManagementRecord
    {
        [Key]
        [Column("StudentID")]
        public int StudentId { get; set; }

   [Column("StudentNumber")]
      [MaxLength(10)]
        public string StudentNumber { get; set; } = string.Empty;

        [Column("FirstName")]
    [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName")]
   [MaxLength(50)]
  public string LastName { get; set; } = string.Empty;

        [Column("Email")]
        [MaxLength(100)]
   public string Email { get; set; } = string.Empty;

     [Column("PasswordHash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

 [Column("CourseTitle")]
        [MaxLength(100)]
      public string CourseTitle { get; set; } = string.Empty;

     [Column("IsApproved")]
        public bool IsApproved { get; set; } = false;

     [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

     [Column("LastLoginTime")]
      public DateTime? LastLoginTime { get; set; }
    }

    /// <summary>
    /// Maps to MySQL 'Lecturer' table
    /// </summary>
    [Table("Lecturer")]
    public class Lecturer
    {
        [Key]
        [Column("LecturerID")]
        public int LecturerId { get; set; }

        [Column("FirstName")]
   [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

   [Column("LastName")]
     [MaxLength(50)]
  public string LastName { get; set; } = string.Empty;

        [Column("Email")]
      [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("PasswordHash")]
        [MaxLength(255)]
  public string PasswordHash { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("IsAdmin")]
        public bool IsAdmin { get; set; } = false;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

      // Navigation properties
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }

    /// <summary>
/// Maps to MySQL 'Subject' table
    /// </summary>
    [Table("Subject")]
    public class SubjectEntity
    {
      [Key]
        [Column("SubjectID")]
        public int SubjectId { get; set; }

    [Column("SubjectName")]
        [MaxLength(100)]
     public string SubjectName { get; set; } = string.Empty;

        [Column("IsActive")]
      public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
        public virtual ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    }

    /// <summary>
    /// Maps to MySQL 'Test' table - Updated to match actual schema
    /// </summary>
    [Table("Test")]
    public class Test
    {
        [Key]
        [Column("TestID")]
        public int TestId { get; set; }

        [Column("SubjectID")]
        public int SubjectId { get; set; }

        [Column("LecturerID")]
      public int? LecturerId { get; set; }

        [Column("TestName")]
        [MaxLength(100)]
     public string TestName { get; set; } = string.Empty;

     [Column("NumQuestions")]
        public int NumQuestions { get; set; } = 10;

        [Column("CreatedAt")]
   public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
   [ForeignKey("SubjectId")]
        public virtual SubjectEntity? Subject { get; set; }

        [ForeignKey("LecturerId")]
  public virtual Lecturer? Lecturer { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
    }

    /// <summary>
/// Maps to MySQL 'Question' table - with OptionA, OptionB, OptionC, OptionD, CorrectAnswer
    /// </summary>
 [Table("Question")]
    public class QuestionEntity
    {
   [Key]
        [Column("QuestionID")]
     public int QuestionId { get; set; }

    [Column("SubjectID")]
  public int SubjectId { get; set; }

        [Column("QuestionText")]
    public string QuestionText { get; set; } = string.Empty;

        [Column("OptionA")]
        [MaxLength(255)]
 public string OptionA { get; set; } = string.Empty;

    [Column("OptionB")]
    [MaxLength(255)]
    public string OptionB { get; set; } = string.Empty;

        [Column("OptionC")]
        [MaxLength(255)]
        public string OptionC { get; set; } = string.Empty;

 [Column("OptionD")]
        [MaxLength(255)]
        public string OptionD { get; set; } = string.Empty;

        [Column("CorrectAnswer")]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, or D

        // Navigation properties
        [ForeignKey("SubjectId")]
        public virtual SubjectEntity? Subject { get; set; }
    }

    /// <summary>
    /// Maps to MySQL 'Answer' table
    /// </summary>
    [Table("Answer")]
    public class Answer
    {
        [Key]
        [Column("AnswerID")]
    public int AnswerId { get; set; }

      [Column("QuestionID")]
        public int QuestionId { get; set; }

        [Column("OptionLabel")]
    [MaxLength(1)]
        public string OptionLabel { get; set; } = string.Empty; // A, B, C, D

    [Column("AnswerText")]
        [MaxLength(255)]
        public string AnswerText { get; set; } = string.Empty;

        [Column("IsCorrect")]
        public bool IsCorrect { get; set; } = false;

   [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("QuestionId")]
     public virtual QuestionEntity? Question { get; set; }
    }

    /// <summary>
    /// Maps to MySQL 'TestAttempt' table - Updated to match actual schema
    /// </summary>
    [Table("TestAttempt")]
    public class TestAttempt
{
 [Key]
        [Column("AttemptID")]
        public int AttemptId { get; set; }

        [Column("StudentID")]
 public int StudentId { get; set; }

        [Column("TestID")]
        public int TestId { get; set; }

        [Column("ScorePercentage")]
        public decimal ScorePercentage { get; set; } = 0;

        [Column("TotalPoints")]
 public int TotalPoints { get; set; } = 0;

        [Column("AttemptDate")]
        public DateTime AttemptDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }

        [ForeignKey("TestId")]
        public virtual Test? Test { get; set; }

        public virtual ICollection<StudentAnswerEntity> StudentAnswers { get; set; } = new List<StudentAnswerEntity>();
    }

    /// <summary>
    /// Maps to MySQL 'StudentAnswer' table for recording student responses
    /// </summary>
    [Table("StudentAnswer")]
    public class StudentAnswerEntity
    {
        [Key]
    [Column("StudentAnswerID")]
        public int StudentAnswerId { get; set; }

        [Column("AttemptID")]
        public int AttemptId { get; set; }

        [Column("QuestionID")]
        public int QuestionId { get; set; }

 [Column("SelectedAnswer")]
        [MaxLength(1)]
      public string SelectedAnswer { get; set; } = string.Empty;

        [Column("IsCorrect")]
  public bool IsCorrect { get; set; } = false;

        [Column("AnsweredAt")]
        public DateTime AnsweredAt { get; set; } = DateTime.Now;

        [Column("PointsEarned")]
        public int PointsEarned { get; set; } = 0;

   // Navigation properties
    [ForeignKey("AttemptId")]
        public virtual TestAttempt? TestAttempt { get; set; }

   [ForeignKey("QuestionId")]
        public virtual QuestionEntity? Question { get; set; }
    }

    /// <summary>
    /// Maps to MySQL 'LoginAudit' table
    /// </summary>
    [Table("LoginAudit")]
    public class LoginAudit
    {
        [Key]
        [Column("AuditID")]
        public int AuditId { get; set; }

  [Column("UserID")]
   public int UserId { get; set; }

        [Column("Role")]
        [MaxLength(20)]
   public string Role { get; set; } = string.Empty; // 'Student', 'Admin', 'Lecturer'

        [Column("Success")]
        public bool Success { get; set; } = false;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
 }
}
