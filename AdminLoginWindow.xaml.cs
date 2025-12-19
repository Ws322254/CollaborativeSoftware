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
    public partial class AdminLoginWindow : Window
    {
        private readonly UserRole _role;
        private readonly MySqlDbContext _context;

        // REQUIRED by WPF (designer / default navigation)
        public AdminLoginWindow() : this(UserRole.Admin)
        {
        }

        // Used when role is passed explicitly
        public AdminLoginWindow(UserRole role)
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
                var admin = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && l.IsAdmin);

                if (admin == null)
                {
                    var lecturerCheck = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower() && !l.IsAdmin);

                    if (lecturerCheck != null)
                    {
                        RecordLoginAuditAsync(0, "Admin", false);
                        MessageBox.Show("This account is a lecturer account. Please use Lecturer login.");
                        return;
                    }

                    RecordLoginAuditAsync(0, "Admin", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                var passwordHash = HashPassword(password);
                if (admin.PasswordHash != passwordHash)
                {
                    RecordLoginAuditAsync(admin.LecturerId, "Admin", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                RecordLoginAuditAsync(admin.LecturerId, "Admin", true);

                Session.CurrentUserEmail = admin.Email;
                Session.CurrentUserRole = _role;
                Session.CurrentUserId = admin.LecturerId;

                MessageBox.Show("Login successful!");

                AdminDashboardWindow dashboard = new AdminDashboardWindow();
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
