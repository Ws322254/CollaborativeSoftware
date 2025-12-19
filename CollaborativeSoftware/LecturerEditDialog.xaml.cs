using System.Windows;
using CollaborativeSoftware.Models;

namespace CollaborativeSoftware
{
    public partial class LecturerEditDialog : Window
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        public LecturerEditDialog(Lecturer lecturer)
        {
            InitializeComponent();
            FirstNameBox.Text = lecturer.FirstName;
            LastNameBox.Text = lecturer.LastName;
            EmailBox.Text = lecturer.Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Please enter first name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Please enter last name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Please enter email.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FirstName = FirstNameBox.Text.Trim();
            LastName = LastNameBox.Text.Trim();
            Email = EmailBox.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
