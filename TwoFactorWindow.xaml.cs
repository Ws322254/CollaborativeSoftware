using System.Windows;

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

            if (_role == UserRole.Lecturer)
            {
                new LecturerDashboardWindow().Show();
            }
            else if (_role == UserRole.Admin)
            {
                new AdminDashboardWindow().Show();
            }

            this.Close();
        }

        private void CodeBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
