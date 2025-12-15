using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CollaborativeSoftware
{
    public partial class LecturerLoginWindow : Window
    {
        public LecturerLoginWindow()
        {
            InitializeComponent();
        }

        // Login Logic
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a username and a password");
                return;
            }

            // Get User
            var user = GetUserFromDB(username);

            if (user == null)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            // Verify Password
            bool passwordValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            MessageBox.Show("Login successful!");

            LecturerDashboardWindow dashboard = new LecturerDashboardWindow();
            dashboard.Show();
            this.Close();
        }

        // Back Navigation
        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow w = new RoleSelectionWindow();
            w.Show();
            this.Close();
        }

        // DB Logic
        private LecturerModel GetUserFromDB(string username)
        {
            // TODO: Replace with real database lookup
            return new LecturerModel()
            {
                Username = "lecturer1",
                PasswordSalt = MockSalt,
                PasswordHash = MockHashedPassword,
                Email = "lecturer@example.com"
            };
        }

        // Hashing Algorithm
        public static string HashPassword(string password, string salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 100000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string newHash = HashPassword(password, storedSalt);
            return storedHash == newHash;
        }

        // Mock Data
        private const string MockSalt = "Yutens";
        private static readonly string MockHashedPassword = HashPassword("password123", MockSalt);
    }

    public class LecturerModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
    }
}
