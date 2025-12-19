using System.Windows;

namespace CollaborativeSoftware
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: load logic
        }

        // Subject management
        private void CreateSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void EditSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ToggleSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        // Lecturer management
        private void CreateLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void EditLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void DisableLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        // System settings
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        // Logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
