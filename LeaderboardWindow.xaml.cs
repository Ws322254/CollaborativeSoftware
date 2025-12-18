using System.Windows;
using System.Windows.Controls;

namespace CollaborativeSoftware
{
    public partial class LeaderboardWindow : Window
    {
        public LeaderboardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: load leaderboard data
        }

        private void QuizComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: filter leaderboard by quiz
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
