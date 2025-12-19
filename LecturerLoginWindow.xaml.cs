using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CollaborativeSoftware
{
    public partial class LecturerLoginWindow : Window
    {
        private readonly UserRole _role;

        // REQUIRED by WPF (designer / default navigation)
        public LecturerLoginWindow() : this(UserRole.Lecturer)
        {
        }

        // Used when role is passed explicitly
        public LecturerLoginWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
        }

        // Login Logic (WITH 2FA)
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a username and a password");
                return;
            }

            var user = GetUserFromDB(username);

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

            LecturerDashboardWindow dashboard = new LecturerDashboardWindow();
            dashboard.Show();
            this.Close();
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow w = new RoleSelectionWindow();
            w.Show();
            this.Close();
        }

        private LecturerModel GetUserFromDB(string username)
        {
            return new LecturerModel()
            {
                Username = "lecturer1",
                PasswordSalt = MockSalt,
                PasswordHash = MockHashedPassword,
                Email = "butteruniverse951@gmail.com"
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

    public class LecturerModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
    }
}
