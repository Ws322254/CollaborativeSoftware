using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CollaborativeSoftware
{
    public partial class AdminLoginWindow : Window
    {
        private readonly UserRole _role;

        // REQUIRED by WPF (designer / default navigation)
        public AdminLoginWindow() : this(UserRole.Admin)
        {
        }

        // Used when role is passed explicitly
        public AdminLoginWindow(UserRole role)
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

            var admin = GetAdminFromDB(username);

            if (admin == null)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            bool passwordValid = VerifyPassword(password, admin.PasswordHash, admin.PasswordSalt);

            if (!passwordValid)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            // -------- 2FA START --------
            MessageBox.Show("Password verified. Sending verification code...");

            string code = TwoFactorManager.GenerateCode();
            await EmailService.Send2FACodeAsync(admin.Email, code);

            TwoFactorWindow twoFA = new TwoFactorWindow(_role);
            twoFA.Show();
            this.Close();
            // -------- 2FA END --------
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow w = new RoleSelectionWindow();
            w.Show();
            this.Close();
        }

        private AdminModel GetAdminFromDB(string username)
        {
            // TODO: Replace with real database lookup
            return new AdminModel()
            {
                Username = "admin1",
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

    public class AdminModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
    }
}
