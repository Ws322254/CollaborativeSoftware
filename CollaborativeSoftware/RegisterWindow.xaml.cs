using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware
{
    public partial class RegisterWindow : Window
    {
        private readonly MySqlDbContext _context;

        public RegisterWindow()
        {
            try
            {
                InitializeComponent();
                // Initialize MySQL database context
                _context = new MySqlDbContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing registration: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("Registration service not initialized. Please restart the application.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate email format (basic validation)
            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate password strength (basic validation)
            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Password Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Register student in MySQL database
                var (success, user, message) = await RegisterStudentAsync(firstName, lastName, email, password);

                MessageBox.Show(message, success ? "Registration Successful" : "Registration Failed",
                    MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (success)
                {
                    RoleSelectionWindow roleWindow = new RoleSelectionWindow();
                    roleWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow roleWindow = new RoleSelectionWindow();
            roleWindow.Show();
            this.Close();
        }

        private async Task<(bool Success, User? User, string Message)> RegisterStudentAsync(
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
                    LastLoginTime = currentTime
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
