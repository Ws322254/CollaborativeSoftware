using System.Windows;

namespace CollaborativeSoftware
{
    public partial class LecturerDashboardWindow : Window
    {
        public LecturerDashboardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) { }
        private void StudentSearchBox_TextChanged(object sender, RoutedEventArgs e) { }
        private void ApproveButton_Click(object sender, RoutedEventArgs e) { }
        private void DisableButton_Click(object sender, RoutedEventArgs e) { }
        private void CreateQuizButton_Click(object sender, RoutedEventArgs e) { }
        private void EditQuizButton_Click(object sender, RoutedEventArgs e) { }
        private void DeleteQuizButton_Click(object sender, RoutedEventArgs e) { }
        private void Logout_Click(object sender, RoutedEventArgs e) { }
    }
}
