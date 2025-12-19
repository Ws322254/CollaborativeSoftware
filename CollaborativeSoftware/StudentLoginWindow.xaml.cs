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
    public partial class StudentLoginWindow : Window
    {
        private readonly UserRole _role;
        private readonly MySqlDbContext _context;

        public StudentLoginWindow() : this(UserRole.Student)
        {
        }

        public StudentLoginWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
            _context = new MySqlDbContext();
        }

        // Login Logic (NO 2FA)
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
                var student = await _context.StudentManagements
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

                if (student == null)
                {
                    RecordLoginAuditAsync(0, "Student", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                var passwordHash = HashPassword(password);
                if (student.PasswordHash != passwordHash)
                {
                    RecordLoginAuditAsync(student.StudentId, "Student", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                if (!student.IsApproved)
                {
                    RecordLoginAuditAsync(student.StudentId, "Student", false);
                    MessageBox.Show("Your account is pending approval by a lecturer.");
                    return;
                }

                if (!student.IsActive)
                {
                    RecordLoginAuditAsync(student.StudentId, "Student", false);
                    MessageBox.Show("Your account has been disabled.");
                    return;
                }

                try
                {
                    student.LastLoginTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Log failure but allow login to proceed
                }

                RecordLoginAuditAsync(student.StudentId, "Student", true);

                Session.CurrentUserEmail = student.Email;
                Session.CurrentUserRole = _role;
                Session.CurrentUserId = student.StudentId;

                MessageBox.Show("Password verified. Sending verification code...");

                string code = TwoFactorManager.GenerateCode();
                await EmailService.Send2FACodeAsync(student.Email, code);

                TwoFactorWindow twoFA = new TwoFactorWindow(_role);
                twoFA.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Database error: {innerMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAccountLink_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow w = new RegisterWindow();
            w.Show();
            this.Close();
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
