using System.Windows;
using System.Windows.Controls;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;
using Microsoft.EntityFrameworkCore;

namespace CollaborativeSoftware
{
    public partial class LecturerDashboardWindow : Window
    {
        private readonly ApplicationUser _currentUser;
        private readonly MySqlDbContext _dbContext;
        private readonly MySqlAuthService _authService;
        private readonly MySqlQuizService _quizService;
        private List<StudentManagementRecord> _allStudents = new();
      private List<Test> _allTests = new();
        private List<QuestionEntity> _allQuestions = new();
        private List<SubjectEntity> _allSubjects = new();
    private bool _isNavExpanded = true;
        private string _currentPanel = "Dashboard";

        public LecturerDashboardWindow(ApplicationUser user)
        {
            try
 {
         InitializeComponent();
    _currentUser = user;
           _dbContext = new MySqlDbContext();
                _authService = new MySqlAuthService(_dbContext);
        _quizService = new MySqlQuizService(_dbContext);
         UserNameBlock.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";
            }
     catch (Exception ex)
 {
       MessageBox.Show(
     $"Error initializing Lecturer Dashboard: {ex.Message}\n\n{ex.InnerException?.Message}",
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
             await LoadStudents();
                await LoadTests();
           await LoadQuestions();
          await LoadSubjects();
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
   var pendingCount = 0;
                var totalStudents = 0;
                var totalTests = 0;
           var totalQuestions = 0;

            await Task.Run(() =>
     {
         pendingCount = _dbContext.StudentManagements.Count(s => !s.IsApproved && s.IsActive);
         totalStudents = _dbContext.StudentManagements.Count(s => s.IsActive);
    totalTests = _dbContext.Tests.Count();
       totalQuestions = _dbContext.Questions.Count();
  });

     PendingApprovalsBlock.Text = pendingCount.ToString();
          TotalStudentsBlock.Text = totalStudents.ToString();
                TotalQuizzesBlock.Text = totalTests.ToString();
  TotalQuestionsBlock.Text = totalQuestions.ToString();
     }
            catch (Exception ex)
            {
   MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
    }

        private async Task LoadStudents()
        {
     try
          {
         List<StudentManagementRecord> students = new();

                await Task.Run(() =>
  {
  students = _dbContext.StudentManagements
   .OrderByDescending(s => s.CreatedAt)
             .ToList();
    });

       _allStudents = students;
      StudentsDataGrid.ItemsSource = _allStudents;
            }
            catch (Exception ex)
       {
    MessageBox.Show($"Error loading students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

     private async Task LoadTests()
     {
            try
   {
                List<Test> tests = new();

      await Task.Run(() =>
       {
             tests = _dbContext.Tests
  .Include(t => t.Subject)
               .OrderByDescending(t => t.CreatedAt)
            .ToList();
        });

         _allTests = tests;
        QuizzesDataGrid.ItemsSource = _allTests;
            }
            catch (Exception ex)
        {
     MessageBox.Show($"Error loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
       }
    }

        private async Task LoadQuestions()
        {
         try
            {
    List<QuestionEntity> questions = new();

    await Task.Run(() =>
      {
  questions = _dbContext.Questions
  .Include(q => q.Subject)
 .ToList();
      });

     _allQuestions = questions;
       QuestionsDataGrid.ItemsSource = _allQuestions;
         }
      catch (Exception ex)
  {
      MessageBox.Show($"Error loading questions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      private async Task LoadSubjects()
        {
    try
            {
         List<SubjectEntity> subjects = new();

    await Task.Run(() =>
          {
               subjects = _dbContext.Subjects
       .OrderBy(s => s.SubjectName)
        .ToList();
        });

   _allSubjects = subjects;
   SubjectsDataGrid.ItemsSource = _allSubjects;
            }
 catch (Exception ex)
            {
     MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPanel(string panelName)
 {
            _currentPanel = panelName;

  DashboardPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            TestsPanel.Visibility = Visibility.Collapsed;
 QuestionsPanel.Visibility = Visibility.Collapsed;
            SubjectsPanel.Visibility = Visibility.Collapsed;
            AnalyticsPanel.Visibility = Visibility.Collapsed;

         switch (panelName)
       {
      case "Dashboard":
        DashboardPanel.Visibility = Visibility.Visible;
       PageTitleIcon.Text = "";
        PageTitle.Text = "Dashboard";
   PageSubtitle.Text = "Welcome back! Here's your overview.";
             break;
            case "Students":
           StudentsPanel.Visibility = Visibility.Visible;
               PageTitleIcon.Text = "";
    PageTitle.Text = "Student Management";
       PageSubtitle.Text = "Approve, manage, and monitor student accounts.";
          break;
          case "Subjects":
             SubjectsPanel.Visibility = Visibility.Visible;
      PageTitleIcon.Text = "";
      PageTitle.Text = "Subject Management";
 PageSubtitle.Text = "Create and manage quiz subjects.";
  break;
          case "Tests":
          TestsPanel.Visibility = Visibility.Visible;
                 PageTitleIcon.Text = "";
PageTitle.Text = "Test Management";
       PageSubtitle.Text = "Create, edit, and manage tests.";
    break;
     case "Questions":
         QuestionsPanel.Visibility = Visibility.Visible;
   PageTitleIcon.Text = "";
              PageTitle.Text = "Question Bank";
       PageSubtitle.Text = "Manage questions for all subjects.";
    break;
                case "Analytics":
      AnalyticsPanel.Visibility = Visibility.Visible;
 PageTitleIcon.Text = "";
      PageTitle.Text = "Analytics & Reports";
      PageSubtitle.Text = "View performance data and statistics.";
     LoadAnalytics();
  break;
            }
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
ShowPanel("Dashboard");
        }

        private void NavStudents_Click(object sender, RoutedEventArgs e)
    {
    ShowPanel("Students");
   }

        private void NavSubjects_Click(object sender, RoutedEventArgs e)
   {
 ShowPanel("Subjects");
        }

        private void NavQuizzes_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Tests");
        }

   private void NavQuestions_Click(object sender, RoutedEventArgs e)
{
            ShowPanel("Questions");
        }

     private void NavAnalytics_Click(object sender, RoutedEventArgs e)
  {
    ShowPanel("Analytics");
        }

   private void StudentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
        {
             string searchText = StudentSearchBox.Text.ToLower();

         if (string.IsNullOrWhiteSpace(searchText))
  {
        StudentsDataGrid.ItemsSource = _allStudents;
     return;
     }

              var filtered = _allStudents.Where(s =>
          s.FirstName.ToLower().Contains(searchText) ||
   s.LastName.ToLower().Contains(searchText) ||
        s.Email.ToLower().Contains(searchText) ||
    s.StudentNumber.ToLower().Contains(searchText)
       ).ToList();

                StudentsDataGrid.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

     private async void RefreshStudents_Click(object sender, RoutedEventArgs e)
      {
     await LoadStudents();
    await LoadStatistics();
   MessageBox.Show("Student list refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
       if (StudentsDataGrid.SelectedItem is StudentManagementRecord selectedStudent)
    {
    if (selectedStudent.IsApproved)
         {
           MessageBox.Show("This student is already approved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
return;
        }

         var result = MessageBox.Show(
              $"Approve student {selectedStudent.FirstName} {selectedStudent.LastName}?\n\nThey will be able to log in and take tests.",
            "Confirm Approval",
        MessageBoxButton.YesNo,
     MessageBoxImage.Question);

         if (result == MessageBoxResult.Yes)
      {
       try
      {
    var success = await _authService.ApproveStudentRecordAsync(selectedStudent.StudentId);

     if (success)
     {
     MessageBox.Show(
           $"Student {selectedStudent.FirstName} {selectedStudent.LastName} has been approved!",
          "Success",
          MessageBoxButton.OK,
      MessageBoxImage.Information);
 await LoadStudents();
       await LoadStatistics();
  }
           else
             {
   MessageBox.Show("Failed to approve student.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
       }
    }
         catch (Exception ex)
          {
          MessageBox.Show($"Error approving student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
              }
      }
          }
            else
 {
           MessageBox.Show("Please select a student to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
   }
        }

        private async void DisableButton_Click(object sender, RoutedEventArgs e)
        {
      if (StudentsDataGrid.SelectedItem is StudentManagementRecord selectedStudent)
            {
           if (!selectedStudent.IsActive)
    {
  MessageBox.Show("This student is already disabled.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
         return;
             }

            var result = MessageBox.Show(
        $"Disable student {selectedStudent.FirstName} {selectedStudent.LastName}?\n\nThey will no longer be able to log in.",
        "Confirm Disable",
         MessageBoxButton.YesNo,
       MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
        {
                try
         {
           var success = await _authService.DisableStudentRecordAsync(selectedStudent.StudentId);

         if (success)
      {
     MessageBox.Show(
        $"Student {selectedStudent.FirstName} {selectedStudent.LastName} has been disabled.",
            "Success",
        MessageBoxButton.OK,
MessageBoxImage.Information);
              await LoadStudents();
     await LoadStatistics();
             }
      else
             {
     MessageBox.Show("Failed to disable student.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
   catch (Exception ex)
       {
         MessageBox.Show($"Error disabling student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
    }
  }
       else
      {
    MessageBox.Show("Please select a student to disable.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
    }

    private async void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem is StudentManagementRecord selectedStudent)
     {
                if (selectedStudent.IsActive)
         {
 MessageBox.Show("This student is already active.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
   return;
      }

    var result = MessageBox.Show(
  $"Re-enable student {selectedStudent.FirstName} {selectedStudent.LastName}?\n\nThey will be able to log in again.",
  "Confirm Enable",
     MessageBoxButton.YesNo,
            MessageBoxImage.Question);

      if (result == MessageBoxResult.Yes)
                {
            try
              {
 selectedStudent.IsActive = true;
        await _dbContext.SaveChangesAsync();

         MessageBox.Show(
        $"Student {selectedStudent.FirstName} {selectedStudent.LastName} has been re-enabled.",
 "Success",
       MessageBoxButton.OK,
           MessageBoxImage.Information);
      await LoadStudents();
          await LoadStatistics();
                    }
    catch (Exception ex)
    {
             MessageBox.Show($"Error enabling student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          }
     }
       }
            else
        {
       MessageBox.Show("Please select a student to enable.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
    }

        private void ViewStudentDetails_Click(object sender, RoutedEventArgs e)
    {
            if (StudentsDataGrid.SelectedItem is StudentManagementRecord selectedStudent)
      {
           var details = $"Student Details\n" +
   $"===============================\n\n" +
      $"ID: {selectedStudent.StudentId}\n" +
   $"Student Number: {selectedStudent.StudentNumber}\n" +
        $"Name: {selectedStudent.FirstName} {selectedStudent.LastName}\n" +
  $"Email: {selectedStudent.Email}\n" +
           $"Course: {selectedStudent.CourseTitle}\n\n" +
     $"Status:\n" +
         $"  - Approved: {(selectedStudent.IsApproved ? "Yes" : "No")}\n" +
          $"  - Active: {(selectedStudent.IsActive ? "Yes" : "No")}\n\n" +
            $"Dates:\n" +
       $"  - Created: {selectedStudent.CreatedAt:dd MMM yyyy HH:mm}\n" +
   $"  - Last Login: {(selectedStudent.LastLoginTime.HasValue ? selectedStudent.LastLoginTime.Value.ToString("dd MMM yyyy HH:mm") : "Never")}";

 MessageBox.Show(details, "Student Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
        MessageBox.Show("Please select a student to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
   }

        private async void RefreshSubjects_Click(object sender, RoutedEventArgs e)
        {
     await LoadSubjects();
        MessageBox.Show("Subject list refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

 private async void CreateSubject_Click(object sender, RoutedEventArgs e)
        {
    var subjectName = PromptInput("Enter new subject name:");

            if (!string.IsNullOrWhiteSpace(subjectName))
      {
           try
    {
             var newSubject = await _quizService.CreateSubjectAsync(subjectName.Trim());

              MessageBox.Show(
     $"Subject '{newSubject.SubjectName}' created successfully!",
  "Success",
             MessageBoxButton.OK,
    MessageBoxImage.Information);

         await LoadSubjects();
        }
      catch (Exception ex)
   {
        MessageBox.Show($"Error creating subject: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
}
       }
        }

        private async void EditSubject_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsDataGrid.SelectedItem is SubjectEntity selectedSubject)
       {
    var newName = PromptInput("Edit subject name:", selectedSubject.SubjectName);

      if (!string.IsNullOrWhiteSpace(newName) && newName != selectedSubject.SubjectName)
    {
  try
        {
      var success = await _quizService.UpdateSubjectAsync(selectedSubject.SubjectId, newName.Trim());

        if (success)
         {
      MessageBox.Show(
              $"Subject updated to '{newName}'!",
       "Success",
   MessageBoxButton.OK,
   MessageBoxImage.Information);

   await LoadSubjects();
    }
                 else
       {
             MessageBox.Show("Failed to update subject.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
       }
    }
              catch (Exception ex)
           {
        MessageBox.Show($"Error updating subject: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
         }
    }
    else
{
                MessageBox.Show("Please select a subject to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

      private async void ToggleSubjectActive_Click(object sender, RoutedEventArgs e)
        {
       if (SubjectsDataGrid.SelectedItem is SubjectEntity selectedSubject)
            {
                var newStatus = selectedSubject.IsActive ? "inactive" : "active";
                var result = MessageBox.Show(
     $"Set subject '{selectedSubject.SubjectName}' to {newStatus}?",
    "Confirm Toggle",
              MessageBoxButton.YesNo,
    MessageBoxImage.Question);

              if (result == MessageBoxResult.Yes)
      {
      try
  {
      var success = await _quizService.ToggleSubjectActiveAsync(selectedSubject.SubjectId);

        if (success)
      {
         MessageBox.Show(
            $"Subject is now {newStatus}!",
 "Success",
               MessageBoxButton.OK,
            MessageBoxImage.Information);

           await LoadSubjects();
      }
              else
                {
      MessageBox.Show("Failed to toggle subject status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
     }
         catch (Exception ex)
  {
   MessageBox.Show($"Error toggling subject: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
          }
         }
      else
            {
 MessageBox.Show("Please select a subject to toggle.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
        }

        private async void DeleteSubject_Click(object sender, RoutedEventArgs e)
      {
   if (SubjectsDataGrid.SelectedItem is SubjectEntity selectedSubject)
        {
          var result = MessageBox.Show(
             $"Delete subject '{selectedSubject.SubjectName}'?\n\nThis action cannot be undone!\n\nNote: Subjects with associated tests or questions cannot be deleted.",
             "Confirm Delete",
  MessageBoxButton.YesNo,
         MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes)
       {
  try
      {
  var (success, message) = await _quizService.DeleteSubjectAsync(selectedSubject.SubjectId);

       if (success)
         {
 MessageBox.Show(
    message,
              "Success",
  MessageBoxButton.OK,
     MessageBoxImage.Information);

     await LoadSubjects();
           }
             else
  {
                    MessageBox.Show(message, "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
           }
    }
   catch (Exception ex)
        {
         MessageBox.Show($"Error deleting subject: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
     }
         }
         else
            {
       MessageBox.Show("Please select a subject to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
       }
 }

     private string? PromptInput(string prompt, string defaultValue = "")
   {
  var window = new Window
          {
     Title = "Input",
                Width = 400,
             Height = 160,
              WindowStartupLocation = WindowStartupLocation.CenterOwner,
      Owner = this,
       Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1a, 0x1a, 0x2e)),
          ResizeMode = ResizeMode.NoResize
            };

   var stackPanel = new StackPanel { Margin = new Thickness(20) };
            var label = new TextBlock
            {
           Text = prompt,
 Margin = new Thickness(0, 0, 0, 10),
        Foreground = System.Windows.Media.Brushes.White,
     FontSize = 14
          };
       var textBox = new TextBox
            {
      Height = 35,
   Padding = new Thickness(10, 8, 10, 8),
            Text = defaultValue,
    FontSize = 14,
Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2d, 0x2d, 0x44)),
       Foreground = System.Windows.Media.Brushes.White,
    BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3d, 0x3d, 0x54))
    };

var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };
        var cancelButton = new Button
            {
                Content = "Cancel",
          Width = 80,
      Height = 32,
     Margin = new Thickness(0, 0, 10, 0),
          Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4d, 0x4d, 0x6d)),
                Foreground = System.Windows.Media.Brushes.White,
   BorderThickness = new Thickness(0),
  Cursor = System.Windows.Input.Cursors.Hand
        };
     var okButton = new Button
  {
  Content = "OK",
       Width = 80,
 Height = 32,
    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x8b, 0x3d, 0xff)),
        Foreground = System.Windows.Media.Brushes.White,
       BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
     };

      stackPanel.Children.Add(label);
      stackPanel.Children.Add(textBox);
            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);
     stackPanel.Children.Add(buttonPanel);

            window.Content = stackPanel;

        cancelButton.Click += (s, e) => { window.DialogResult = false; };
     okButton.Click += (s, e) => { window.DialogResult = true; };
   textBox.Focus();

            return window.ShowDialog() == true ? textBox.Text : null;
        }

      private async void RefreshTests_Click(object sender, RoutedEventArgs e)
        {
   await LoadTests();
     await LoadStatistics();
            MessageBox.Show("Test list refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
     }

        private async void CreateQuizButton_Click(object sender, RoutedEventArgs e)
 {
            var lecturerId = _currentUser.Id;

            var createWindow = new CreateTestWindow(lecturerId);
            createWindow.Owner = this;

        if (createWindow.ShowDialog() == true)
  {
           await LoadTests();
        await LoadStatistics();
       }
        }

        private async void EditQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (QuizzesDataGrid.SelectedItem is Test selectedTest)
    {
          var editWindow = new EditTestWindow(selectedTest);
     editWindow.Owner = this;

  if (editWindow.ShowDialog() == true && editWindow.TestUpdated)
       {
  await LoadTests();
      await LoadStatistics();
      }
      }
            else
   {
     MessageBox.Show("Please select a test to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
    }

        private async void DeleteQuizButton_Click(object sender, RoutedEventArgs e)
        {
        if (QuizzesDataGrid.SelectedItem is Test selectedTest)
      {
     var result = MessageBox.Show(
  $"Delete test '{selectedTest.TestName}'?\n\nThis action cannot be undone!",
        "Confirm Delete",
       MessageBoxButton.YesNo,
    MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                {
       try
         {
             var success = await _quizService.DeleteTestAsync(selectedTest.TestId);

 if (success)
         {
     MessageBox.Show("Test deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
      await LoadTests();
              await LoadStatistics();
           }
         }
          catch (Exception ex)
        {
           MessageBox.Show($"Error deleting test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
   }
           }
      }
        else
            {
          MessageBox.Show("Please select a test to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
        }

        private void ViewTestResults_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesDataGrid.SelectedItem is Test selectedTest)
      {
  MessageBox.Show(
$"Test Results: {selectedTest.TestName}\n===============================\n\n" +
"Test results can be viewed in the Analytics section.\n\n" +
  "Go to: Analytics > View test attempt data",
        "View Results",
          MessageBoxButton.OK,
  MessageBoxImage.Information);
          }
        else
            {
             MessageBox.Show("Please select a test to view results.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    private async void RefreshQuestions_Click(object sender, RoutedEventArgs e)
        {
    await LoadQuestions();
  await LoadStatistics();
         MessageBox.Show("Question list refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
     }

        private async void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
          var createWindow = new CreateQuestionWindow();
            createWindow.Owner = this;

          if (createWindow.ShowDialog() == true)
            {
    await LoadQuestions();
       await LoadStatistics();

        if (createWindow.QuestionsCreatedCount > 1)
         {
             MessageBox.Show(
        $"Added {createWindow.QuestionsCreatedCount} questions to the Question Bank.",
      "Questions Added",
               MessageBoxButton.OK,
           MessageBoxImage.Information);
             }
          }
   }

        private async void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
  if (QuestionsDataGrid.SelectedItem is QuestionEntity selectedQuestion)
            {
   var editWindow = new EditQuestionWindow(selectedQuestion);
      editWindow.Owner = this;

          if (editWindow.ShowDialog() == true && editWindow.QuestionUpdated)
     {
           await LoadQuestions();
   }
   }
  else
            {
    MessageBox.Show("Please select a question to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
        }

        private async void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsDataGrid.SelectedItem is QuestionEntity selectedQuestion)
            {
     var result = MessageBox.Show(
                    $"Delete this question?\n\n\"{selectedQuestion.QuestionText}\"\n\nThis action cannot be undone!",
 "Confirm Delete",
       MessageBoxButton.YesNo,
    MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes)
   {
 try
           {
   _dbContext.Questions.Remove(selectedQuestion);
   await _dbContext.SaveChangesAsync();

 MessageBox.Show("Question deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    await LoadQuestions();
  await LoadStatistics();
              }
    catch (Exception ex)
  {
        MessageBox.Show($"Error deleting question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
  }
        }
            }
            else
            {
  MessageBox.Show("Please select a question to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewQuestion_Click(object sender, RoutedEventArgs e)
        {
        if (QuestionsDataGrid.SelectedItem is QuestionEntity selectedQuestion)
   {
  var details = $"Question Details\n" +
         $"===============================\n\n" +
            $"ID: {selectedQuestion.QuestionId}\n" +
           $"Subject: {selectedQuestion.Subject?.SubjectName ?? "N/A"}\n\n" +
                    $"Question:\n{selectedQuestion.QuestionText}\n\n" +
         $"Options:\n" +
   $"  A) {selectedQuestion.OptionA}\n" +
 $"  B) {selectedQuestion.OptionB}\n" +
           $"  C) {selectedQuestion.OptionC}\n" +
   $"  D) {selectedQuestion.OptionD}\n\n" +
     $"Correct Answer: {selectedQuestion.CorrectAnswer}";

                MessageBox.Show(details, "Question Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    else
         {
 MessageBox.Show("Please select a question to view.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
        }

      private async void LoadAnalytics()
        {
     try
{
  var totalAttempts = 0;
    var avgScore = 0.0;

 await Task.Run(() =>
      {
          totalAttempts = _dbContext.TestAttempts.Count();
       if (totalAttempts > 0)
            {
  avgScore = (double)_dbContext.TestAttempts.Average(t => t.ScorePercentage);
  }
    });

        AnalyticsSummary.Text = $"Total Test Attempts: {totalAttempts}\n" +
             $"Average Score: {avgScore:F1}%\n\n" +
          $"Active Students: {TotalStudentsBlock.Text}\n" +
   $"Total Tests: {TotalQuizzesBlock.Text}\n" +
  $"Total Questions: {TotalQuestionsBlock.Text}";

      await LoadTopPerformingStudents();
            }
          catch (Exception ex)
     {
      AnalyticsSummary.Text = $"Error loading analytics: {ex.Message}";
        }
        }

   private async Task LoadTopPerformingStudents()
      {
         try
            {
      using var dbContext = new MySqlDbContext();
        var quizService = new MySqlQuizService(dbContext);

   var leaderboardData = await quizService.GetLeaderboardAsync();

    var displayItems = leaderboardData.Select((item, index) => new LecturerLeaderboardItem
         {
     Rank = index + 1,
        StudentName = item.StudentName,
    TestsTaken = item.TestsTaken,
              AverageScore = item.AverageScore,
       TotalPoints = item.TotalPoints
 }).ToList();

                LeaderboardDataGrid.ItemsSource = displayItems;
            }
catch (Exception ex)
          {
  MessageBox.Show($"Error loading leaderboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
   }
        }

        private async void RefreshLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            await LoadTopPerformingStudents();
          MessageBox.Show("Leaderboard refreshed.", "Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
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

    public class LecturerLeaderboardItem
    {
public int Rank { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TestsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public int TotalPoints { get; set; }
    }
}
