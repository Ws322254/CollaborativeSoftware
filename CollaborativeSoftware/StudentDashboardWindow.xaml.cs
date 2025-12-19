using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;
using Microsoft.EntityFrameworkCore;

namespace CollaborativeSoftware
{
    public partial class StudentDashboardWindow : Window
    {
        private readonly ApplicationUser _currentUser;
        private readonly MySqlQuizService _quizService;
      private readonly MySqlDbContext _dbContext;
     private List<Test> _availableTests = new();
        private List<StudentResultItem> _results = new();
      private List<StudentLeaderboardItem> _leaderboard = new();
        private bool _isNavExpanded = true;

        public StudentDashboardWindow(ApplicationUser user)
  {
        try
       {
InitializeComponent();
     _currentUser = user;
   _dbContext = new MySqlDbContext();
       _quizService = new MySqlQuizService(_dbContext);
    UserNameBlock.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";
     }
            catch (Exception ex)
   {
 MessageBox.Show(
    $"Error initializing Student Dashboard: {ex.Message}\n\n{ex.InnerException?.Message}",
      "Initialization Error",
          MessageBoxButton.OK,
  MessageBoxImage.Error);
        }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
     {
    await LoadAllData();
       ShowPanel("Dashboard");
     }

        private async Task LoadAllData()
        {
            try
    {
   await LoadStatistics();
         await LoadAvailableTests();
            await LoadResults();
    await LoadLeaderboard();
    UpdateProgressSummary();
   }
       catch (Exception ex)
  {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
     }

        private async Task LoadStatistics()
        {
        try
          {
          using var dbContext = new MySqlDbContext();
      var quizService = new MySqlQuizService(dbContext);

        var (totalPoints, averageScore, testsCompleted) = await quizService.GetStudentStatisticsAsync(_currentUser.Id);

    TotalPointsBlock.Text = totalPoints.ToString();
          AverageScoreBlock.Text = $"{averageScore:F1}%";
       TestsCompletedBlock.Text = testsCompleted.ToString();

       var tests = await quizService.GetAllTestsAsync();
         AvailableTestsBlock.Text = tests.Count.ToString();
            }
         catch (Exception ex)
     {
             MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
   }
        }

        private async Task LoadAvailableTests()
        {
            try
            {
     using var dbContext = new MySqlDbContext();
     var quizService = new MySqlQuizService(dbContext);

      _availableTests = await quizService.GetAllTestsAsync();

                QuizzesListBox.ItemsSource = _availableTests
       .Select(t => $"{t.TestName}  |  {t.Subject?.SubjectName ?? "Unknown"}  |  {t.NumQuestions} Questions")
      .ToList();
 }
            catch (Exception ex)
       {
           MessageBox.Show($"Error loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
        }

        private async Task LoadResults()
    {
            try
    {
       using var dbContext = new MySqlDbContext();
          var quizService = new MySqlQuizService(dbContext);

     var attempts = await quizService.GetStudentTestHistoryAsync(_currentUser.Id);

         _results = attempts.Select(a => new StudentResultItem
 {
     AttemptId = a.AttemptId,
      TestName = a.Test?.TestName ?? "Unknown Test",
         SubjectName = a.Test?.Subject?.SubjectName ?? "Unknown",
       ScorePercentage = a.ScorePercentage,
               TotalPoints = a.TotalPoints,
     AttemptDate = a.AttemptDate,
          IsPassing = a.ScorePercentage >= 50
          }).ToList();

              ResultsDataGrid.ItemsSource = _results;
            }
            catch (Exception ex)
   {
      MessageBox.Show($"Error loading results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
   }
        }

        private async Task LoadLeaderboard()
        {
   try
          {
    using var dbContext = new MySqlDbContext();
                var quizService = new MySqlQuizService(dbContext);

                var leaderboardData = await quizService.GetLeaderboardAsync();

     _leaderboard = leaderboardData.Select((item, index) => new StudentLeaderboardItem
      {
    Rank = index + 1,
  UserId = item.StudentId,
         StudentName = item.StudentName,
      TestsTaken = item.TestsTaken,
        AverageScore = item.AverageScore,
            TotalPoints = item.TotalPoints
 }).ToList();

    LeaderboardDataGrid.ItemsSource = _leaderboard;

    var userRank = _leaderboard.FirstOrDefault(l => l.UserId == _currentUser.Id);

  if (userRank != null)
    {
            YourRankBlock.Text = $"#{userRank.Rank}";
   YourTotalPointsBlock.Text = userRank.TotalPoints.ToString();
                }
            else
             {
        YourRankBlock.Text = "#--";
          YourTotalPointsBlock.Text = TotalPointsBlock.Text;
              }
       }
 catch (Exception ex)
            {
    MessageBox.Show($"Error loading leaderboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

        private void UpdateProgressSummary()
        {
       try
            {
      var testsCompleted = int.Parse(TestsCompletedBlock.Text);
    var avgScore = AverageScoreBlock.Text;
       var totalPoints = TotalPointsBlock.Text;

   if (testsCompleted == 0)
                {
   ProgressSummary.Text = "You haven't taken any tests yet.\n\n" +
    "Go to 'Available Tests' to start your first test!";
    }
       else
        {
            ProgressSummary.Text = $"You have completed {testsCompleted} test(s).\n" +
   $"Your average score is {avgScore}.\n" +
     $"You've earned {totalPoints} total points.\n\n" +
          "Keep up the great work!";
   }
            }
      catch
    {
        ProgressSummary.Text = "Loading your progress...";
  }
        }

 private void ShowPanel(string panelName)
   {
            DashboardPanel.Visibility = Visibility.Collapsed;
      TestsPanel.Visibility = Visibility.Collapsed;
     ResultsPanel.Visibility = Visibility.Collapsed;
 LeaderboardPanel.Visibility = Visibility.Collapsed;

         switch (panelName)
    {
           case "Dashboard":
 DashboardPanel.Visibility = Visibility.Visible;
 PageTitleIcon.Text = "";
            PageTitle.Text = "Dashboard";
      PageSubtitle.Text = "Welcome back! Here is your progress overview.";
              break;
          case "Tests":
       TestsPanel.Visibility = Visibility.Visible;
 PageTitleIcon.Text = "";
         PageTitle.Text = "Available Tests";
     PageSubtitle.Text = "Select a test to begin.";
         break;
     case "Results":
                 ResultsPanel.Visibility = Visibility.Visible;
        PageTitleIcon.Text = "";
  PageTitle.Text = "My Results";
          PageSubtitle.Text = "View your test history and scores.";
    break;
       case "Leaderboard":
        LeaderboardPanel.Visibility = Visibility.Visible;
           PageTitleIcon.Text = "";
         PageTitle.Text = "Leaderboard";
           PageSubtitle.Text = "See how you rank against other students.";
    break;
   }
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
{
            ShowPanel("Dashboard");
    }

        private void NavTests_Click(object sender, RoutedEventArgs e)
      {
       ShowPanel("Tests");
 }

        private void NavResults_Click(object sender, RoutedEventArgs e)
        {
          ShowPanel("Results");
     }

        private void NavLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Leaderboard");
        }

        private async void RefreshTests_Click(object sender, RoutedEventArgs e)
        {
      await LoadAvailableTests();
  await LoadStatistics();
            MessageBox.Show("Tests list refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
   }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
   if (QuizzesListBox.SelectedIndex < 0)
            {
       MessageBox.Show("Please select a test to start.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
    }

            var selectedTest = _availableTests[QuizzesListBox.SelectedIndex];

    var result = MessageBox.Show(
  $"Start test: {selectedTest.TestName}?\n\nSubject: {selectedTest.Subject?.SubjectName ?? "Unknown"}\nQuestions: {selectedTest.NumQuestions}",
         "Confirm Start",
      MessageBoxButton.YesNo,
          MessageBoxImage.Question);

   if (result == MessageBoxResult.Yes)
     {
     var quizWindow = new MySqlQuizWindow(_currentUser, selectedTest);
                quizWindow.Show();
    this.Close();
            }
        }

        private void ViewTestDetails_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesListBox.SelectedIndex < 0)
            {
    MessageBox.Show("Please select a test to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
   }

      var selectedTest = _availableTests[QuizzesListBox.SelectedIndex];

   var details = $"Test Details\n" +
   $"===============================\n\n" +
     $"Title: {selectedTest.TestName}\n" +
            $"Subject: {selectedTest.Subject?.SubjectName ?? "Unknown"}\n" +
                $"Questions: {selectedTest.NumQuestions}\n" +
      $"Passing Score: 50%\n\n" +
     $"Good luck!";

            MessageBox.Show(details, "Test Details", MessageBoxButton.OK, MessageBoxImage.Information);
 }

      private void QuizzesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
  StartTestButton_Click(null!, null!);
   }

        private async void RefreshResults_Click(object sender, RoutedEventArgs e)
        {
    await LoadResults();
            await LoadStatistics();
    MessageBox.Show("Results refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewResultDetails_Click(object sender, RoutedEventArgs e)
   {
            if (ResultsDataGrid.SelectedItem is StudentResultItem selectedResult)
          {
      var status = selectedResult.IsPassing ? "PASSED" : "FAILED";
     var statusMessage = selectedResult.IsPassing ? "Great job!" : "Keep practicing!";

      var details = $"Result Details\n" +
    $"===============================\n\n" +
     $"Test: {selectedResult.TestName}\n" +
    $"Subject: {selectedResult.SubjectName}\n\n" +
          $"Score: {selectedResult.ScorePercentage:F1}%\n" +
          $"Points Earned: {selectedResult.TotalPoints}\n" +
           $"Status: {status}\n\n" +
           $"Date: {selectedResult.AttemptDate:dd MMMM yyyy HH:mm}\n\n" +
       $"{statusMessage}";

   MessageBox.Show(details, "Result Details", MessageBoxButton.OK, MessageBoxImage.Information);
  }
            else
            {
 MessageBox.Show("Please select a result to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
        }

        private void OpenResultsWindow_Click(object sender, RoutedEventArgs e)
   {
   ResultsWindow resultsWindow = new ResultsWindow(_currentUser);
            resultsWindow.Show();
        }

 private async void RefreshLeaderboard_Click(object sender, RoutedEventArgs e)
        {
 await LoadLeaderboard();
       MessageBox.Show("Leaderboard refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenLeaderboardWindow_Click(object sender, RoutedEventArgs e)
        {
      LeaderboardWindow leaderboardWindow = new LeaderboardWindow(_currentUser);
            leaderboardWindow.Show();
        }

    private void ToggleNav_Click(object sender, RoutedEventArgs e)
{
            _isNavExpanded = !_isNavExpanded;
            ToggleNavButton.Content = _isNavExpanded ? "=" : ">";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
  "Are you sure you want to logout?",
     "Confirm Logout",
      MessageBoxButton.YesNo,
           MessageBoxImage.Question);

  if (result == MessageBoxResult.Yes)
   {
         try
 {
         RoleSelectionWindow roleSelection = new RoleSelectionWindow();
          roleSelection.Show();
          this.Close();
                }
             catch (Exception ex)
         {
        MessageBox.Show($"Error logging out: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
 }
        }
    }

    public class StudentResultItem
    {
    public int AttemptId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal ScorePercentage { get; set; }
        public int TotalPoints { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsPassing { get; set; }
    }

    public class StudentLeaderboardItem
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TestsTaken { get; set; }
      public decimal AverageScore { get; set; }
    public int TotalPoints { get; set; }
    }
}
