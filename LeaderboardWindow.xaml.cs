using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;

namespace CollaborativeSoftware
{
    public class LeaderboardItem
    {
        public int Rank { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TestsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public int TotalPoints { get; set; }
    }

    public partial class LeaderboardWindow : Window
    {
        private readonly ApplicationUser _currentUser;
        private List<SubjectEntity> _subjects = new();

        public LeaderboardWindow(ApplicationUser user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSubjects();
            await LoadLeaderboard();
        }

        private async Task LoadSubjects()
        {
            try
            {
                // Create a fresh DbContext for this operation
                using var dbContext = new MySqlDbContext();
                var quizService = new MySqlQuizService(dbContext);

                // Load all subjects for filter
                _subjects = await quizService.GetAllSubjectsAsync();

                // Add "All Subjects" option
                var displayList = new List<string> { "All Subjects" };
                displayList.AddRange(_subjects.Select(s => s.SubjectName));

                QuizComboBox.ItemsSource = displayList;
                QuizComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void QuizComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            await LoadLeaderboard();
        }

        private async Task LoadLeaderboard()
        {
            try
            {
                // Create a fresh DbContext for this operation to avoid threading issues
                using var dbContext = new MySqlDbContext();
                var quizService = new MySqlQuizService(dbContext);

                // Get leaderboard from MySQL - now returns LeaderboardEntry with proper names
                var leaderboardData = await quizService.GetLeaderboardAsync();

                var leaderboardItems = leaderboardData.Select((item, index) => new LeaderboardItem
                {
                    Rank = index + 1,
                    StudentName = item.StudentName, // Now properly gets name from StudentManagement
                    TestsTaken = item.TestsTaken,
                    AverageScore = item.AverageScore,
                    TotalPoints = item.TotalPoints
                }).ToList();

                LeaderboardDataGrid.ItemsSource = leaderboardItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading leaderboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLeaderboard();
            MessageBox.Show("Leaderboard refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
