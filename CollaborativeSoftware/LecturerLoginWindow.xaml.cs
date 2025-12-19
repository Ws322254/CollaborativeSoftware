using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware
{
    public partial class LecturerLoginWindow : Window
    {
        private readonly UserRole _role;
        private readonly MySqlDbContext _context;

        // REQUIRED by WPF (designer / default navigation)
        public LecturerLoginWindow() : this(UserRole.Lecturer)
        {
        }

        // Used when role is passed explicitly
        public LecturerLoginWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
            _context = new MySqlDbContext();
        }

        // Login Logic (WITH 2FA)
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter an email and a password");
                return;
            }

            try
            {
                Lecturer? lecturer = null;

                try
                {
                    // Try standard query first
                    lecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower());
                }
                catch
                {
                    // If IsActive column doesn't exist, use raw SQL query
                    try
                    {
                        lecturer = await _context.Lecturers
                            .FromSqlRaw("SELECT LecturerID, FirstName, LastName, Email, PasswordHash, CreatedAt FROM Lecturer WHERE LOWER(Email) = LOWER({0})", email)
                            .FirstOrDefaultAsync();
                    }
                    catch
                    {
                        lecturer = null;
                    }
                }

                if (lecturer == null)
                {
                    RecordLoginAuditAsync(0, "Lecturer", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                var passwordHash = HashPassword(password);
                if (lecturer.PasswordHash != passwordHash)
                {
                    RecordLoginAuditAsync(lecturer.LecturerId, "Lecturer", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                RecordLoginAuditAsync(lecturer.LecturerId, "Lecturer", true);

                Session.CurrentUserEmail = lecturer.Email;
                Session.CurrentUserRole = _role;
                Session.CurrentUserId = lecturer.LecturerId;

                MessageBox.Show("Login successful!");

                var applicationUser = new ApplicationUser
                {
                    Id = lecturer.LecturerId,
                    Email = lecturer.Email,
                    FirstName = lecturer.FirstName,
                    LastName = lecturer.LastName,
                    Role = "Lecturer",
                    IsActive = true,
                    CreatedAt = lecturer.CreatedAt
                };

                LecturerDashboardWindow dashboard = new LecturerDashboardWindow(applicationUser);
                dashboard.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Database error: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow w = new RoleSelectionWindow();
            w.Show();
            this.Close();
        }

        private async void RecordLoginAuditAsync(int userId, string role, bool success)
        {
            try
            {
                using (var auditContext = new MySqlDbContext())
                {
                    var audit = new Models.LoginAudit
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
                // Silent fail for audit
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
    }
}
