using System.Windows;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;

namespace CollaborativeSoftware
{
    public partial class ResultsWindow : Window
    {
        private readonly ApplicationUser _currentUser;

        public ResultsWindow(ApplicationUser user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadResults();
        }

        private async Task LoadResults()
        {
            try
            {
                // Create fresh context to avoid threading issues
                using var dbContext = new MySqlDbContext();
                var quizService = new MySqlQuizService(dbContext);

                // Load statistics
                var (totalPoints, averageScore, testsTaken) = await quizService.GetStudentStatisticsAsync(_currentUser.Id);

                TotalPointsBlock.Text = totalPoints.ToString();
                AverageScoreBlock.Text = $"{averageScore:F1}%";
                TestsTakenBlock.Text = testsTaken.ToString();

                // Load test history
                var testHistory = await quizService.GetStudentTestHistoryAsync(_currentUser.Id);

                // Convert to display items
                var displayItems = testHistory.Select(a => new ResultDisplayItem
                {
                    TestName = a.Test?.TestName ?? "Unknown Test",
                    SubjectName = a.Test?.Subject?.SubjectName ?? "Unknown",
                    ScorePercentage = a.ScorePercentage,
                    TotalPoints = a.TotalPoints,
                    AttemptDate = a.AttemptDate
                }).ToList();

                ResultsDataGrid.ItemsSource = displayItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StudentDashboardWindow dashboard = new StudentDashboardWindow(_currentUser);
            dashboard.Show();
            this.Close();
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardWindow leaderboard = new LeaderboardWindow(_currentUser);
            leaderboard.Show();
        }
    }

    // Helper class for Results display
    public class ResultDisplayItem
    {
        public string TestName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal ScorePercentage { get; set; }
        public int TotalPoints { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
