using CollaborativeSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CollaborativeSoftware
{
    public partial class TwoFactorWindow : Window
    {
        public TwoFactorWindow()
        {
            InitializeComponent();
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

            LecturerDashboardWindow dashboard = new LecturerDashboardWindow();
            dashboard.Show();
            this.Close();
        }

        private void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
