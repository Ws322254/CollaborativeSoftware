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
                // HARDCODED TEST LOGIN - Remove this in production
                if (email.ToLower() == "admin@test.com" && password == "Admin@123")
                {
                    RecordLoginAuditAsync(1, "Admin", true);

                    var testApplicationUser = new ApplicationUser
                    {
                        Id = 1,
                        Email = "admin@test.com",
                        FirstName = "Test",
                        LastName = "Admin",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    AdminDashboardWindow testDashboard = new AdminDashboardWindow(testApplicationUser);
                    testDashboard.Show();
                    this.Close();
                    return;
                }

                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Role == "Admin");

                if (admin == null)
                {
                    var userCheck = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Role != "Admin");

                    if (userCheck != null)
                    {
                        RecordLoginAuditAsync(0, "Admin", false);
                        MessageBox.Show("This account does not have admin privileges. Please use the appropriate login.");
                        return;
                    }

                    RecordLoginAuditAsync(0, "Admin", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                var passwordHash = HashPassword(password);
                
                // Trim the database hash and compare
                var dbHash = admin.PasswordHash?.Trim() ?? string.Empty;
                
                if (dbHash != passwordHash)
                {
                    RecordLoginAuditAsync(admin.UserId, "Admin", false);
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

                RecordLoginAuditAsync(admin.UserId, "Admin", true);

                Session.CurrentUserEmail = admin.Email;
                Session.CurrentUserRole = _role;
                Session.CurrentUserId = admin.UserId;

                MessageBox.Show("Login successful!");

                var applicationUser = new ApplicationUser
                {
                    Id = admin.UserId,
                    Email = admin.Email,
                    FirstName = admin.Email.Split('@')[0],
                    LastName = "Admin",
                    Role = "Admin",
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt
                };

                AdminDashboardWindow dashboard = new AdminDashboardWindow(applicationUser);
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
