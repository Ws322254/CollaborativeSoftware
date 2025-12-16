using System.Windows;

namespace CollaborativeSoftware
{
    public partial class RoleSelectionWindow : Window
    {
        public RoleSelectionWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a role.");
                return;
            }

            string selectedRole = RoleComboBox.SelectedItem.ToString();

            if (selectedRole.Contains("Student"))
            {
                StudentLoginWindow studentLogin = new StudentLoginWindow();
                studentLogin.Show();
            }
            else if (selectedRole.Contains("Lecturer"))
            {
                LecturerLoginWindow lecturerLogin = new LecturerLoginWindow();
                lecturerLogin.Show();
            }
            else if (selectedRole.Contains("Admin"))
            {
                AdminLoginWindow adminLogin = new AdminLoginWindow();
                adminLogin.Show();
            }
            else
            {
                MessageBox.Show("Invalid role selected.");
                return;
            }

            this.Close();
        }
    }
}
