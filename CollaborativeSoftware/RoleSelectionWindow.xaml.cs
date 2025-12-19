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
                new StudentLoginWindow(UserRole.Student).Show();
                //StudentLoginWindow studentLogin = new StudentLoginWindow(UserRole.Student);
                //studentLogin.Show();
            }
            else if (selectedRole.Contains("Lecturer"))
            {
                new LecturerLoginWindow(UserRole.Lecturer).Show();
            }
            else if (selectedRole.Contains("Admin"))
            {
                //AdminLoginWindow adminLogin = new AdminLoginWindow(UserRole.Admin);
                //adminLogin.Show();
                new AdminLoginWindow(UserRole.Admin).Show();
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
