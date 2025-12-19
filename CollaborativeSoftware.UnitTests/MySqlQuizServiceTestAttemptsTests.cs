using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;
using CollaborativeSoftware.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[TestClass]
public class MySqlQuizServiceTestAttemptsTests
{
    private DbContextOptions<MySqlDbContext> _options;

    [TestInitialize]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<MySqlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private MySqlDbContext CreateContext()
    {
        return new MySqlDbContext(_options);
    }

    [TestMethod]
    public async Task CreateTestAttemptAsync_CreatesNewAttempt()
    {
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Users.Add(user);
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var service = new MySqlQuizService(context);
            var result = await service.CreateTestAttemptAsync(user.UserId, test.TestId);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.UserId, result.StudentId);
            Assert.AreEqual(test.TestId, result.TestId);
        }
    }

    [TestMethod]
    public async Task RecordStudentAnswerAsync_CorrectAnswer_ScoresPositive()
    {
        int attemptId, questionId;
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            var question = new QuestionEntity
            {
                SubjectId = 0,
                QuestionText = "What is 2+2?",
                OptionA = "3",
                OptionB = "4",
                OptionC = "5",
                OptionD = "6",
                CorrectAnswer = "B"
            };
            context.Users.Add(user);
            context.Subjects.Add(subject);
            context.Questions.Add(question);
            await context.SaveChangesAsync();
            questionId = question.QuestionId;

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var service = new MySqlQuizService(context);
            var attempt = await service.CreateTestAttemptAsync(user.UserId, test.TestId);
            attemptId = attempt.AttemptId;
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            await service.RecordStudentAnswerAsync(attemptId, questionId, "B");

            var answer = await context.StudentAnswers.FirstOrDefaultAsync(sa => sa.AttemptId == attemptId);
            Assert.IsNotNull(answer);
            Assert.IsTrue(answer.IsCorrect);
            Assert.AreEqual(30, answer.PointsEarned);
        }
    }

    [TestMethod]
    public async Task RecordStudentAnswerAsync_IncorrectAnswer_ScoresNegative()
    {
        int attemptId, questionId;
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            var question = new QuestionEntity
            {
                SubjectId = 0,
                QuestionText = "What is 2+2?",
                OptionA = "3",
                OptionB = "4",
                OptionC = "5",
                OptionD = "6",
                CorrectAnswer = "B"
            };
            context.Users.Add(user);
            context.Subjects.Add(subject);
            context.Questions.Add(question);
            await context.SaveChangesAsync();
            questionId = question.QuestionId;

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var service = new MySqlQuizService(context);
            var attempt = await service.CreateTestAttemptAsync(user.UserId, test.TestId);
            attemptId = attempt.AttemptId;
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            await service.RecordStudentAnswerAsync(attemptId, questionId, "A");

            var answer = await context.StudentAnswers.FirstOrDefaultAsync(sa => sa.AttemptId == attemptId);
            Assert.IsNotNull(answer);
            Assert.IsFalse(answer.IsCorrect);
            Assert.AreEqual(-10, answer.PointsEarned);
        }
    }

    [TestMethod]
    public async Task GetStudentTestHistoryAsync_ReturnsAttemptsByStudent()
    {
        int studentId;
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var user2 = new User { Email = "student2@test.com", Role = "Student", PasswordHash = "hash" };
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Users.Add(user);
            context.Users.Add(user2);
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            studentId = user.UserId;

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var service = new MySqlQuizService(context);
            await service.CreateTestAttemptAsync(user.UserId, test.TestId);
            await service.CreateTestAttemptAsync(user.UserId, test.TestId);
            await service.CreateTestAttemptAsync(user2.UserId, test.TestId);
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetStudentTestHistoryAsync(studentId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(h => h.StudentId == studentId));
        }
    }

    [TestMethod]
    public async Task GetStudentStatisticsAsync_CalculatesCorrectly()
    {
        int studentId;
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Users.Add(user);
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            studentId = user.UserId;

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var attempt1 = new TestAttempt
            {
                StudentId = user.UserId,
                TestId = test.TestId,
                ScorePercentage = 80,
                TotalPoints = 100
            };
            var attempt2 = new TestAttempt
            {
                StudentId = user.UserId,
                TestId = test.TestId,
                ScorePercentage = 60,
                TotalPoints = 50
            };
            context.TestAttempts.Add(attempt1);
            context.TestAttempts.Add(attempt2);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var (totalPoints, averageScore, testsCompleted) = await service.GetStudentStatisticsAsync(studentId);

            Assert.AreEqual(150, totalPoints);
            Assert.AreEqual(70, averageScore);
            Assert.AreEqual(2, testsCompleted);
        }
    }

    [TestMethod]
    public async Task GetLeaderboardAsync_ReturnsTop10ByPoints()
    {
        using (var context = CreateContext())
        {
            for (int i = 0; i < 15; i++)
            {
                var user = new User
                {
                    Email = $"student{i}@test.com",
                    Role = "Student",
                    PasswordHash = "hash"
                };
                var student = new StudentManagementRecord
                {
                    FirstName = $"Student",
                    LastName = $"{i}",
                    Email = $"student{i}@test.com",
                    StudentNumber = $"S{i:000}",
                    PasswordHash = "hash"
                };
                context.Users.Add(user);
                context.StudentManagements.Add(student);
            }
            await context.SaveChangesAsync();

            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var users = await context.Users.ToListAsync();
            for (int i = 0; i < users.Count; i++)
            {
                var attempt = new TestAttempt
                {
                    StudentId = users[i].UserId,
                    TestId = test.TestId,
                    ScorePercentage = 50 + i * 3,
                    TotalPoints = 100 - i * 5
                };
                context.TestAttempts.Add(attempt);
            }
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetLeaderboardAsync();

            Assert.IsTrue(result.Count <= 10);
            for (int i = 0; i < result.Count - 1; i++)
            {
                Assert.IsTrue(result[i].TotalPoints >= result[i + 1].TotalPoints);
            }
        }
    }

    [TestMethod]
    public async Task GetLeaderboardAsync_UsesStudentManagementNameWhenAvailable()
    {
        using (var context = CreateContext())
        {
            var user = new User { Email = "student@test.com", Role = "Student", PasswordHash = "hash" };
            var student = new StudentManagementRecord
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "student@test.com",
                StudentNumber = "S001",
                PasswordHash = "hash"
            };
            context.Users.Add(user);
            context.StudentManagements.Add(student);
            await context.SaveChangesAsync();

            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var test = new Test { SubjectId = subject.SubjectId, TestName = "Quiz", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();

            var attempt = new TestAttempt
            {
                StudentId = user.UserId,
                TestId = test.TestId,
                ScorePercentage = 80,
                TotalPoints = 100
            };
            context.TestAttempts.Add(attempt);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetLeaderboardAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("John Doe", result[0].StudentName);
        }
    }
}
