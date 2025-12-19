using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;
using CollaborativeSoftware.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[TestClass]
public class MySqlQuizServiceTests
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
    public async Task CreateSubjectAsync_CreatesAndReturnsSubject()
    {
        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.CreateSubjectAsync("Chemistry");

            Assert.IsNotNull(result);
            Assert.AreEqual("Chemistry", result.SubjectName);
            Assert.IsTrue(result.IsActive);
        }
    }

    [TestMethod]
    public async Task GetAllSubjectsAsync_ReturnsOnlyActiveSubjects()
    {
        using (var context = CreateContext())
        {
            context.Subjects.Add(new SubjectEntity { SubjectName = "Math", IsActive = true });
            context.Subjects.Add(new SubjectEntity { SubjectName = "History", IsActive = false });
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetAllSubjectsAsync();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(s => s.IsActive));
        }
    }

    [TestMethod]
    public async Task DeleteSubjectAsync_WithNoAssociatedData_DeletesSuccessfully()
    {
        int subjectId;
        using (var context = CreateContext())
        {
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            subjectId = subject.SubjectId;
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var (success, message) = await service.DeleteSubjectAsync(subjectId);

            Assert.IsTrue(success);
            Assert.AreEqual("Subject deleted successfully.", message);
        }
    }

    [TestMethod]
    public async Task DeleteSubjectAsync_WithAssociatedTests_ReturnsFalse()
    {
        int subjectId;
        using (var context = CreateContext())
        {
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            subjectId = subject.SubjectId;

            var test = new Test { SubjectId = subjectId, TestName = "Test1", NumQuestions = 10 };
            context.Tests.Add(test);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var (success, message) = await service.DeleteSubjectAsync(subjectId);

            Assert.IsFalse(success);
            Assert.AreEqual("Cannot delete subject - it has associated tests.", message);
        }
    }

    [TestMethod]
    public async Task GetTestsBySubjectAsync_ReturnsTestsForSubject()
    {
        int subjectId;
        using (var context = CreateContext())
        {
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            subjectId = subject.SubjectId;

            context.Tests.Add(new Test { SubjectId = subject.SubjectId, TestName = "Test1", NumQuestions = 10 });
            context.Tests.Add(new Test { SubjectId = subject.SubjectId, TestName = "Test2", NumQuestions = 10 });
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetTestsBySubjectAsync(subjectId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.SubjectId == subjectId));
        }
    }

    [TestMethod]
    public async Task GetPendingStudentRecordsAsync_ReturnsOnlyUnapprovedAndActive()
    {
        using (var context = CreateContext())
        {
            context.StudentManagements.Add(new StudentManagementRecord
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                StudentNumber = "S001",
                PasswordHash = "hash",
                IsApproved = true,
                IsActive = true
            });
            context.StudentManagements.Add(new StudentManagementRecord
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                StudentNumber = "S002",
                PasswordHash = "hash",
                IsApproved = false,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetPendingStudentRecordsAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Jane", result[0].FirstName);
        }
    }

    [TestMethod]
    public async Task ApproveStudentRecordAsync_ApprovesStudent()
    {
        int studentId;
        using (var context = CreateContext())
        {
            var student = new StudentManagementRecord
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                StudentNumber = "S001",
                PasswordHash = "hash",
                IsApproved = false,
                IsActive = true
            };
            context.StudentManagements.Add(student);
            await context.SaveChangesAsync();
            studentId = student.StudentId;
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.ApproveStudentRecordAsync(studentId);

            Assert.IsTrue(result);
            var updated = await context.StudentManagements.FindAsync(studentId);
            Assert.IsTrue(updated.IsApproved);
        }
    }

    [TestMethod]
    public async Task GetQuestionsBySubjectAsync_ReturnsQuestionsForSubject()
    {
        int subjectId;
        using (var context = CreateContext())
        {
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            subjectId = subject.SubjectId;

            context.Questions.Add(new QuestionEntity
            {
                SubjectId = subject.SubjectId,
                QuestionText = "Q1",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = "A"
            });
            context.Questions.Add(new QuestionEntity
            {
                SubjectId = subject.SubjectId,
                QuestionText = "Q2",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = "B"
            });
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetQuestionsBySubjectAsync(subjectId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(q => q.SubjectId == subjectId));
        }
    }

    [TestMethod]
    public async Task GetRandomQuestionsAsync_ReturnsRequestedCount()
    {
        int subjectId;
        using (var context = CreateContext())
        {
            var subject = new SubjectEntity { SubjectName = "Math", IsActive = true };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
            subjectId = subject.SubjectId;

            for (int i = 0; i < 15; i++)
            {
                context.Questions.Add(new QuestionEntity
                {
                    SubjectId = subjectId,
                    QuestionText = $"Q{i}",
                    OptionA = "A",
                    OptionB = "B",
                    OptionC = "C",
                    OptionD = "D",
                    CorrectAnswer = "A"
                });
            }
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var service = new MySqlQuizService(context);
            var result = await service.GetRandomQuestionsAsync(subjectId, 5);

            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.All(q => q.SubjectId == subjectId));
        }
    }
}
