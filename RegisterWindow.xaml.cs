using System.Windows;
using System.Windows.Documents;
using WpfApp2.Data;
using WpfApp2.Services;

namespace WpfApp2
{
    public partial class RegisterWindow : Window
    {
        private MySqlAuthService? _authService;

        public RegisterWindow()
        {
            try
            {
                InitializeComponent();
                // Initialize MySQL authentication service
                _authService = new MySqlAuthService();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing registration: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_authService == null)
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
                var (success, user, message) = await _authService.RegisterStudentAsync(
                    firstName, lastName, email, password);

                MessageBox.Show(message, success ? "Registration Successful" : "Registration Failed",
                    MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (success)
                {
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
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
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
