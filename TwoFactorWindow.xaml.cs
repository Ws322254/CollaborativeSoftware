using System;
using System.Threading.Tasks;
using System.Windows;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware
{
    public partial class TwoFactorWindow : Window
    {
        private readonly UserRole _role;

        public TwoFactorWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = CodeBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(enteredCode))
            {
                MessageBox.Show("Please enter the verification code.");
                return;
            }

            if (!TwoFactorManager.ValidateCode(enteredCode))
            {
                MessageBox.Show("Invalid or expired code.");
                return;
            }

            MessageBox.Show("2FA successful!");

            var applicationUser = new ApplicationUser
            {
                Id = Session.CurrentUserId,
                Email = Session.CurrentUserEmail,
                FirstName = string.Empty,
                LastName = string.Empty,
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            StudentDashboardWindow dashboard = new StudentDashboardWindow(applicationUser);
            dashboard.Show();
            this.Close();
        }

        private async void ResendLink_Click(object sender, RoutedEventArgs e)
        {
            string newCode = TwoFactorManager.GenerateCode();
            await EmailService.Send2FACodeAsync(Session.CurrentUserEmail, newCode);

            MessageBox.Show("A new verification code has been sent.");
        }

        private void CodeBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
