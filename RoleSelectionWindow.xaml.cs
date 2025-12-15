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
            // TODO: navigate based on selected role
        }
    }
}
