using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware.Services
{
    public class MySqlQuizService
    {
        private readonly MySqlDbContext _context;

        public MySqlQuizService()
        {
            _context = new MySqlDbContext();
 }

        public MySqlQuizService(MySqlDbContext context)
        {
 _context = context;
        }

        public async Task<List<SubjectEntity>> GetAllSubjectsAsync()
        {
return await _context.Subjects
  .Where(s => s.IsActive)
                .OrderBy(s => s.SubjectName)
   .ToListAsync();
     }

    public async Task<List<SubjectEntity>> GetAllSubjectsIncludingInactiveAsync()
  {
            return await _context.Subjects
         .OrderBy(s => s.SubjectName)
            .ToListAsync();
        }

        public async Task<SubjectEntity?> GetSubjectByIdAsync(int subjectId)
      {
    return await _context.Subjects.FindAsync(subjectId);
        }

        public async Task<SubjectEntity> CreateSubjectAsync(string subjectName)
        {
   var subject = new SubjectEntity
     {
           SubjectName = subjectName,
IsActive = true
 };

            _context.Subjects.Add(subject);
   await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<bool> UpdateSubjectAsync(int subjectId, string newName)
        {
   var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return false;

            subject.SubjectName = newName;
            await _context.SaveChangesAsync();
return true;
    }

        public async Task<bool> ToggleSubjectActiveAsync(int subjectId)
        {
  var subject = await _context.Subjects.FindAsync(subjectId);
   if (subject == null) return false;

            subject.IsActive = !subject.IsActive;
    await _context.SaveChangesAsync();
    return true;
        }

        public async Task<(bool Success, string Message)> DeleteSubjectAsync(int subjectId)
        {
       var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null)
 return (false, "Subject not found.");

   var hasTests = await _context.Tests.AnyAsync(t => t.SubjectId == subjectId);
        if (hasTests)
         return (false, "Cannot delete subject - it has associated tests.");

 var hasQuestions = await _context.Questions.AnyAsync(q => q.SubjectId == subjectId);
            if (hasQuestions)
      return (false, "Cannot delete subject - it has associated questions.");

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return (true, "Subject deleted successfully.");
   }

     public async Task<int> GetSubjectCountAsync()
        {
    return await _context.Subjects.CountAsync(s => s.IsActive);
     }

        public async Task<List<Test>> GetAllTestsAsync()
        {
        return await _context.Tests
     .Include(t => t.Subject)
          .Include(t => t.Lecturer)
.OrderBy(t => t.TestName)
 .ToListAsync();
 }

        public async Task<List<Test>> GetTestsBySubjectAsync(int subjectId)
      {
     return await _context.Tests
        .Include(t => t.Subject)
    .Include(t => t.Lecturer)
          .Where(t => t.SubjectId == subjectId)
     .OrderBy(t => t.TestName)
         .ToListAsync();
        }

        public async Task<Test?> GetTestWithQuestionsAsync(int testId)
        {
return await _context.Tests
       .Include(t => t.Subject)
              .FirstOrDefaultAsync(t => t.TestId == testId);
        }

     public async Task<Test?> GetTestByIdAsync(int testId)
        {
   return await _context.Tests
   .Include(t => t.Subject)
         .Include(t => t.Lecturer)
          .FirstOrDefaultAsync(t => t.TestId == testId);
        }

        public async Task<List<QuestionEntity>> GetQuestionsBySubjectAsync(int subjectId)
        {
            return await _context.Questions
                .Where(q => q.SubjectId == subjectId)
            .ToListAsync();
        }

        public async Task<List<QuestionEntity>> GetRandomQuestionsAsync(int subjectId, int count = 10)
{
            var questions = await _context.Questions
   .Where(q => q.SubjectId == subjectId)
  .ToListAsync();

            var random = new Random();
            return questions
     .OrderBy(q => random.Next())
    .Take(count)
                .ToList();
        }

        public async Task<List<QuestionEntity>> GetAllQuestionsAsync()
        {
  return await _context.Questions
    .Include(q => q.Subject)
          .ToListAsync();
        }

   public async Task<List<QuestionEntity>> GetQuestionsForTestAsync(int testId)
        {
            var test = await _context.Tests.FindAsync(testId);
  if (test == null) return new List<QuestionEntity>();

     return await _context.Questions
      .Where(q => q.SubjectId == test.SubjectId)
    .ToListAsync();
        }

      public async Task<TestAttempt> CreateTestAttemptAsync(int studentId, int testId)
        {
     var attempt = new TestAttempt
       {
      StudentId = studentId,
     TestId = testId,
   AttemptDate = DateTime.Now,
             ScorePercentage = 0,
                TotalPoints = 0
      };

            _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task RecordStudentAnswerAsync(int attemptId, int questionId, string selectedAnswer)
        {
      var question = await _context.Questions.FindAsync(questionId);

    var isCorrect = question?.CorrectAnswer?.ToUpper() == selectedAnswer?.ToUpper();
    var points = isCorrect ? 30 : -10;

         var existingAnswer = await _context.StudentAnswers
       .FirstOrDefaultAsync(sa => sa.AttemptId == attemptId && sa.QuestionId == questionId);

         if (existingAnswer != null)
     {
       existingAnswer.SelectedAnswer = selectedAnswer;
existingAnswer.IsCorrect = isCorrect;
            existingAnswer.PointsEarned = points;
          existingAnswer.AnsweredAt = DateTime.Now;
            }
    else
            {
    var studentAnswer = new StudentAnswerEntity
   {
 AttemptId = attemptId,
 QuestionId = questionId,
         SelectedAnswer = selectedAnswer,
           IsCorrect = isCorrect,
   AnsweredAt = DateTime.Now,
      PointsEarned = points
    };
          _context.StudentAnswers.Add(studentAnswer);
     }

    await _context.SaveChangesAsync();
        }

        public async Task<TestAttempt> CompleteTestAttemptAsync(int attemptId)
        {
        var attempt = await _context.TestAttempts
      .Include(ta => ta.StudentAnswers)
     .Include(ta => ta.Test)
  .FirstOrDefaultAsync(ta => ta.AttemptId == attemptId);

     if (attempt == null)
    throw new InvalidOperationException("Test attempt not found.");

            var correctAnswers = attempt.StudentAnswers.Count(a => a.IsCorrect);
         var totalQuestions = attempt.StudentAnswers.Count;
     var points = attempt.StudentAnswers.Sum(a => a.PointsEarned);
        var percentage = totalQuestions > 0 ? (decimal)correctAnswers / totalQuestions * 100 : 0;

      attempt.ScorePercentage = percentage;
        attempt.TotalPoints = points;
          attempt.AttemptDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return attempt;
  }

        public async Task<List<TestAttempt>> GetStudentTestHistoryAsync(int studentId)
        {
            return await _context.TestAttempts
       .Include(ta => ta.Test)
         .ThenInclude(t => t!.Subject)
           .Where(ta => ta.StudentId == studentId)
    .OrderByDescending(ta => ta.AttemptDate)
      .ToListAsync();
    }

    public async Task<(int TotalPoints, double AverageScore, int TestsCompleted)> GetStudentStatisticsAsync(int studentId)
  {
 var completedTests = await _context.TestAttempts
    .Where(ta => ta.StudentId == studentId)
         .ToListAsync();

            var totalPoints = completedTests.Sum(t => t.TotalPoints);
 var averageScore = completedTests.Any() ? (double)completedTests.Average(t => t.ScorePercentage) : 0;
            var testsCompleted = completedTests.Count;

     return (totalPoints, averageScore, testsCompleted);
     }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int? testId = null)
     {
 var attemptsQuery = _context.TestAttempts.AsQueryable();

         if (testId.HasValue)
            {
      attemptsQuery = attemptsQuery.Where(ta => ta.TestId == testId.Value);
         }

      var groupedAttempts = await attemptsQuery
     .GroupBy(ta => ta.StudentId)
              .Select(g => new
         {
 StudentId = g.Key,
        TotalPoints = g.Sum(ta => ta.TotalPoints),
               TestsTaken = g.Count(),
 AverageScore = g.Average(ta => ta.ScorePercentage)
   })
    .OrderByDescending(x => x.TotalPoints)
       .Take(10)
                .ToListAsync();

       var results = new List<LeaderboardEntry>();

            foreach (var attempt in groupedAttempts)
      {
     var studentRecord = await _context.StudentManagements
           .FirstOrDefaultAsync(s => s.StudentId == attempt.StudentId);

     string studentName;
         if (studentRecord != null)
                {
  studentName = $"{studentRecord.FirstName} {studentRecord.LastName}";
     }
       else
 {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == attempt.StudentId);
        studentName = user?.Email ?? $"Student #{attempt.StudentId}";
}

       results.Add(new LeaderboardEntry
        {
            StudentId = attempt.StudentId,
            StudentName = studentName,
           TotalPoints = attempt.TotalPoints,
          TestsTaken = attempt.TestsTaken,
         AverageScore = attempt.AverageScore
  });
         }

   return results;
  }

        public async Task<int> GetTotalTestsCountAsync()
   {
     return await _context.Tests.CountAsync();
        }

        public async Task<List<Test>> GetTestsByLecturerAsync(int lecturerId)
        {
 return await _context.Tests
    .Include(t => t.Subject)
            .Where(t => t.LecturerId == lecturerId)
  .OrderByDescending(t => t.CreatedAt)
         .ToListAsync();
    }

        public async Task<bool> DeleteTestAsync(int testId)
   {
  var test = await _context.Tests.FindAsync(testId);
       if (test == null) return false;

   _context.Tests.Remove(test);
   await _context.SaveChangesAsync();
  return true;
        }

 public async Task<List<StudentManagementRecord>> GetAllStudentRecordsAsync()
        {
   return await _context.StudentManagements
           .OrderBy(s => s.LastName)
         .ThenBy(s => s.FirstName)
        .ToListAsync();
        }

        public async Task<List<StudentManagementRecord>> GetPendingStudentRecordsAsync()
    {
     return await _context.StudentManagements
     .Where(s => !s.IsApproved && s.IsActive)
       .ToListAsync();
 }

        public async Task<bool> ApproveStudentRecordAsync(int studentId)
        {
            var student = await _context.StudentManagements.FindAsync(studentId);
            if (student == null) return false;

       student.IsApproved = true;
      await _context.SaveChangesAsync();
       return true;
        }

        public async Task<bool> DisableStudentRecordAsync(int studentId)
    {
        var student = await _context.StudentManagements.FindAsync(studentId);
   if (student == null) return false;

            student.IsActive = false;
  await _context.SaveChangesAsync();
   return true;
        }

        public async Task<int> GetStudentCountAsync()
        {
 return await _context.StudentManagements.CountAsync(s => s.IsActive);
      }

      public async Task<int> GetPendingApprovalCountAsync()
        {
        return await _context.StudentManagements.CountAsync(s => !s.IsApproved && s.IsActive);
    }
    }

    public class LeaderboardEntry
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
   public int TotalPoints { get; set; }
        public int TestsTaken { get; set; }
        public decimal AverageScore { get; set; }
    }
}
