using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CollaborativeSoftware
{
    public partial class StudentLoginWindow : Window
    {
        private readonly UserRole _role;
        public StudentLoginWindow() : this(UserRole.Student)
        {
        }

        public StudentLoginWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
        }

        // Login Logic (NO 2FA)
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a username and a password");
                return;
            }

            var user = GetStudentFromDB(username);

            if (user == null)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            bool passwordValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            MessageBox.Show("Login successful!");

            StudentDashboardWindow dashboard = new StudentDashboardWindow();
            dashboard.Show();
            this.Close();
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow w = new RoleSelectionWindow();
            w.Show();
            this.Close();
        }

        private StudentModel GetStudentFromDB(string username)
        {
            return new StudentModel()
            {
                Username = "student1",
                PasswordSalt = MockSalt,
                PasswordHash = MockHashedPassword,
                Email = "student@example.com"
            };
        }

        public static string HashPassword(string password, string salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Encoding.UTF8.GetBytes(salt),
                100000,
                HashAlgorithmName.SHA256
            );

            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string newHash = HashPassword(password, storedSalt);
            return storedHash == newHash;
        }

        private const string MockSalt = "Yutens";
        private static readonly string MockHashedPassword =
            HashPassword("password123", MockSalt);
    }

    public class StudentModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
    }
}
