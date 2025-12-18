using System;
using System.Windows;

namespace WpfApp2
{
    public partial class RoleSelectionWindow : Window
    {
        public RoleSelectionWindow()
        {
            try
            {
                InitializeComponent();
                // Set event handler AFTER InitializeComponent
                RoleComboBox.SelectionChanged += RoleComboBox_SelectionChanged;
                // Trigger initial description update
                RoleComboBox_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing role selection: {ex.Message}\n\n{ex.InnerException?.Message}",
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                // Update description based on selected role
                if (RoleComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                {
                    string tag = selectedItem.Tag?.ToString() ?? "Student";

                    switch (tag)
                    {
                        case "Student":
                            RoleDescriptionBlock.Text = "Login to access quizzes, view results, and compete on leaderboards.";
                            break;
                        case "Lecturer":
                            RoleDescriptionBlock.Text = "Manage students, create quizzes, and track student performance.";
                            break;
                        case "Admin":
                            RoleDescriptionBlock.Text = "Configure system settings, manage subjects, and oversee operations.";
                            break;
                        default:
                            RoleDescriptionBlock.Text = "Login to access quizzes, view results, and compete on leaderboards.";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating role description: {ex.Message}");
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RoleComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                {
                    string? tag = selectedItem.Tag?.ToString();
                    string role = tag ?? "Student";

                    // Open the appropriate login window based on selected role
                    Window? loginWindow = null;

                    switch (role)
                    {
                        case "Student":
                            loginWindow = new LoginWindow();
                            break;
                        case "Lecturer":
                            loginWindow = new LecturerLoginWindow();
                            break;
                        case "Admin":
                            loginWindow = new AdminLoginWindow();
                            break;
                        default:
                            loginWindow = new LoginWindow();
                            break;
                    }

                    if (loginWindow is not null)
                    {
                        loginWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening login window: {ex.Message}\n\n{ex.InnerException?.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
