using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WpfApp2.Data;
using WpfApp2.Models;

namespace WpfApp2.Services
{
    public class MySqlAuthService
    {
     private readonly MySqlDbContext _context;

        public MySqlAuthService()
        {
    _context = new MySqlDbContext();
  }

        public MySqlAuthService(MySqlDbContext context)
        {
_context = context;
        }

     public async Task<(bool Success, User? User, string Message)> AuthenticateUserAsync(string email, string role)
    {
     try
       {
       var user = await _context.Users
         .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Role == role);

       if (user == null)
            {
   await RecordLoginAuditAsync(0, role, false);
          return (false, null, "Invalid email or user not found.");
  }

     if (!user.IsActive)
    {
     await RecordLoginAuditAsync(user.UserId, role, false);
                return (false, null, "Your account has been disabled.");
     }

      await RecordLoginAuditAsync(user.UserId, role, true);
    return (true, user, "Login successful!");
      }
      catch (Exception ex)
     {
  return (false, null, $"Database error: {ex.Message}");
      }
     }

        public async Task<(bool Success, User? User, string Message)> AuthenticateStudentAsync(string email, string password)
     {
     try
    {
   var studentRecord = await _context.StudentManagements
    .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

          if (studentRecord != null)
  {
   var passwordHash = HashPassword(password);
           if (studentRecord.PasswordHash != passwordHash)
      {
          await RecordLoginAuditAsync(studentRecord.StudentId, "Student", false);
     return (false, null, "Invalid email or password.");
     }

             if (!studentRecord.IsApproved)
      {
       await RecordLoginAuditAsync(studentRecord.StudentId, "Student", false);
      return (false, null, "Your account is pending approval by a lecturer.");
     }

         if (!studentRecord.IsActive)
     {
      await RecordLoginAuditAsync(studentRecord.StudentId, "Student", false);
          return (false, null, "Your account has been disabled.");
       }

       try
       {
       studentRecord.LastLoginTime = DateTime.Now;
       await _context.SaveChangesAsync();
 }
  catch
  {
    }

           await RecordLoginAuditAsync(studentRecord.StudentId, "Student", true);

    var user = new User
        {
UserId = studentRecord.StudentId,
    Email = studentRecord.Email,
        Role = "Student",
      IsActive = studentRecord.IsActive,
         CreatedAt = studentRecord.CreatedAt
       };
    return (true, user, "Login successful!");
    }

          var userRecord = await _context.Users
.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Role == "Student");

       if (userRecord == null)
    {
   await RecordLoginAuditAsync(0, "Student", false);
       return (false, null, "Invalid email or password.");
         }

                if (!userRecord.IsActive)
           {
   await RecordLoginAuditAsync(userRecord.UserId, "Student", false);
    return (false, null, "Your account has been disabled.");
    }

await RecordLoginAuditAsync(userRecord.UserId, "Student", true);
   return (true, userRecord, "Login successful!");
   }
      catch (Exception ex)
      {
    var innerMessage = ex.InnerException?.Message ?? ex.Message;
   return (false, null, $"Database error: {innerMessage}");
            }
 }

        public async Task<(bool Success, StudentManagementRecord? Student, string Message)> AuthenticateStudentManagementAsync(string email, string password)
        {
            try
        {
     var student = await _context.StudentManagements
     .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

     if (student == null)
         {
  await RecordLoginAuditAsync(0, "Student", false);
    return (false, null, "Invalid email or password.");
       }

   var passwordHash = HashPassword(password);
    if (student.PasswordHash != passwordHash)
      {
       await RecordLoginAuditAsync(student.StudentId, "Student", false);
   return (false, null, "Invalid email or password.");
     }

        if (!student.IsApproved)
   {
          await RecordLoginAuditAsync(student.StudentId, "Student", false);
           return (false, null, "Your account is pending approval by a lecturer.");
     }

            if (!student.IsActive)
   {
  await RecordLoginAuditAsync(student.StudentId, "Student", false);
return (false, null, "Your account has been disabled.");
    }

     try
   {
    using (var updateContext = new MySqlDbContext())
{
        var studentToUpdate = await updateContext.StudentManagements.FindAsync(student.StudentId);
if (studentToUpdate != null)
       {
  studentToUpdate.LastLoginTime = DateTime.Now;
       await updateContext.SaveChangesAsync();
      }
        }
     }
  catch
   {
    }

  await RecordLoginAuditAsync(student.StudentId, "Student", true);
     return (true, student, "Login successful!");
        }
catch (Exception ex)
      {
  var innerMessage = ex.InnerException?.Message ?? ex.Message;
     return (false, null, $"Database error: {innerMessage}");
   }
 }

  public async Task<(bool Success, Lecturer? Lecturer, string Message)> AuthenticateLecturerAsync(string email, string password)
        {
     try
 {
    var lecturer = await _context.Lecturers
        .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && !l.IsAdmin);

          if (lecturer == null)
  {
       var adminCheck = await _context.Lecturers
             .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && l.IsAdmin);

 if (adminCheck != null)
   {
            await RecordLoginAuditAsync(0, "Lecturer", false);
     return (false, null, "This account is an admin account. Please use Admin login.");
     }

   await RecordLoginAuditAsync(0, "Lecturer", false);
return (false, null, "Invalid email or password.");
           }

        var passwordHash = HashPassword(password);
    if (lecturer.PasswordHash != passwordHash)
      {
    await RecordLoginAuditAsync(lecturer.LecturerId, "Lecturer", false);
      return (false, null, "Invalid email or password.");
           }

     await RecordLoginAuditAsync(lecturer.LecturerId, "Lecturer", true);
           return (true, lecturer, "Login successful!");
  }
catch (Exception ex)
        {
        var innerMessage = ex.InnerException?.Message ?? ex.Message;
  return (false, null, $"Database error: {innerMessage}");
   }
    }

        public async Task<(bool Success, Lecturer? Admin, string Message)> AuthenticateAdminAsync(string email, string password)
        {
   try
  {
  var admin = await _context.Lecturers
         .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && l.IsAdmin);

        if (admin == null)
    {
     var lecturerCheck = await _context.Lecturers
          .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && !l.IsAdmin);

    if (lecturerCheck != null)
    {
      await RecordLoginAuditAsync(0, "Admin", false);
       return (false, null, "This account is a lecturer account. Please use Lecturer login.");
          }

             await RecordLoginAuditAsync(0, "Admin", false);
         return (false, null, "Invalid email or password.");
            }

     var passwordHash = HashPassword(password);
         if (admin.PasswordHash != passwordHash)
  {
  await RecordLoginAuditAsync(admin.LecturerId, "Admin", false);
      return (false, null, "Invalid email or password.");
            }

      await RecordLoginAuditAsync(admin.LecturerId, "Admin", true);
       return (true, admin, "Login successful!");
    }
 catch (Exception ex)
            {
var innerMessage = ex.InnerException?.Message ?? ex.Message;
    return (false, null, $"Database error: {innerMessage}");
  }
  }

        public async Task<(bool Success, User? User, string Message)> RegisterStudentAsync(
   string firstName, string lastName, string email, string password)
        {
            try
         {
    var existingStudent = await _context.StudentManagements
       .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

      if (existingStudent != null)
    {
       return (false, null, "An account with this email already exists.");
          }

     var studentNumber = await GenerateUniqueStudentNumberAsync();

  var currentTime = DateTime.Now;
  var newStudent = new StudentManagementRecord
      {
           StudentNumber = studentNumber,
      FirstName = firstName,
   LastName = lastName,
 Email = email,
          PasswordHash = HashPassword(password),
     CourseTitle = "General",
          IsApproved = false,
  IsActive = true,
         CreatedAt = currentTime,
   LastLoginTime = null
         };

     _context.StudentManagements.Add(newStudent);
 await _context.SaveChangesAsync();

       var user = new User
           {
        UserId = newStudent.StudentId,
           Email = newStudent.Email,
 Role = "Student",
          IsActive = true,
         CreatedAt = currentTime
        };

return (true, user, "Registration successful! Please wait for a lecturer to approve your account.");
            }
         catch (DbUpdateException dbEx)
   {
 var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
      if (innerMessage.Contains("Duplicate") || innerMessage.Contains("duplicate") ||
     innerMessage.Contains("UNIQUE") || innerMessage.Contains("unique"))
      {
  return (false, null, "Registration failed due to a conflict. Please try again.");
  }
       return (false, null, $"Database error: {innerMessage}");
            }
            catch (Exception ex)
      {
   var innerMessage = ex.InnerException?.Message ?? ex.Message;
         return (false, null, $"Registration error: {innerMessage}");
   }
        }

    private async Task<string> GenerateUniqueStudentNumberAsync(int maxAttempts = 10)
    {
       var random = new Random();

      for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
      var studentNumber = $"S{DateTime.Now:yyMMdd}{random.Next(100, 999)}";

        if (studentNumber.Length > 10)
      {
              studentNumber = studentNumber.Substring(0, 10);
    }

        var exists = await _context.StudentManagements
    .AnyAsync(s => s.StudentNumber == studentNumber);

      if (!exists)
         {
          return studentNumber;
      }
    }

     var fallbackNumber = $"S{DateTime.Now:HHmmssfff}";
            if (fallbackNumber.Length > 10)
   {
       fallbackNumber = fallbackNumber.Substring(0, 10);
            }
 return fallbackNumber;
 }

    private string GenerateStudentNumber()
        {
var random = new Random();
 return $"S{random.Next(100000, 999999)}";
 }

        private async Task RecordLoginAuditAsync(int userId, string role, bool success)
        {
try
   {
   using (var auditContext = new MySqlDbContext())
       {
    var audit = new LoginAudit
      {
            UserId = userId,
      Role = role,
         Success = success,
         Timestamp = DateTime.Now
  };

    auditContext.LoginAudits.Add(audit);
await auditContext.SaveChangesAsync();
    }
            }
      catch
      {
            }
    }

        public static string HashPassword(string password)
   {
  using (var sha256 = SHA256.Create())
    {
   var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
          return Convert.ToBase64String(hashedBytes);
      }
    }

    public static bool VerifyPassword(string password, string hash)
        {
   return HashPassword(password) == hash;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
          return await _context.Users.ToListAsync();
   }

     public async Task<List<User>> GetAllStudentsAsync()
     {
 return await _context.Users.Where(u => u.Role == "Student").ToListAsync();
        }

        public async Task<List<StudentManagementRecord>> GetAllStudentRecordsAsync()
   {
            return await _context.StudentManagements.ToListAsync();
     }

        public async Task<List<StudentManagementRecord>> GetPendingStudentRecordsAsync()
        {
       return await _context.StudentManagements
      .Where(s => !s.IsApproved && s.IsActive)
    .ToListAsync();
}

        public async Task<bool> ApproveStudentAsync(int studentId, int lecturerId)
   {
     try
      {
var student = await _context.Users.FindAsync(studentId);
    if (student == null) return false;

       student.IsActive = true;
   await _context.SaveChangesAsync();
   return true;
      }
    catch
   {
     return false;
            }
        }

      public async Task<bool> ApproveStudentRecordAsync(int studentId)
        {
            try
       {
   var student = await _context.StudentManagements.FindAsync(studentId);
     if (student == null) return false;

     student.IsApproved = true;
          await _context.SaveChangesAsync();
       return true;
            }
            catch
 {
                return false;
  }
}

 public async Task<bool> DisableStudentRecordAsync(int studentId)
    {
    try
 {
         var student = await _context.StudentManagements.FindAsync(studentId);
         if (student == null) return false;

         student.IsActive = false;
  await _context.SaveChangesAsync();
      return true;
         }
   catch
       {
      return false;
      }
        }

      public async Task<List<Lecturer>> GetAllLecturersAsync()
        {
      return await _context.Lecturers
  .Where(l => !l.IsAdmin)
    .ToListAsync();
  }
    }
}
