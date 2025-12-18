using System.Windows;

namespace CollaborativeSoftware
{
    public partial class ResultsWindow : Window
    {
        public ResultsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: load results
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open leaderboard window
        }
    }
}
