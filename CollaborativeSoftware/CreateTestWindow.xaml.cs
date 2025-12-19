using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;

namespace CollaborativeSoftware
{
    public partial class CreateTestWindow : Window
    {
 private readonly MySqlDbContext _dbContext;
        private readonly MySqlQuizService _quizService;
        private readonly int _lecturerId;
      private List<SubjectEntity> _subjects = new();

        public Test? CreatedTest { get; private set; }

        public CreateTestWindow(int lecturerId)
        {
     InitializeComponent();
         _lecturerId = lecturerId;
   _dbContext = new MySqlDbContext();
        _quizService = new MySqlQuizService(_dbContext);
   
            LoadSubjects();
    
        // Update slider display
            NumQuestionsSlider.ValueChanged += (s, e) => 
       {
      NumQuestionsDisplay.Text = $"{(int)NumQuestionsSlider.Value} questions";
            };
            
            // Update available questions count when subject changes
         SubjectComboBox.SelectionChanged += SubjectComboBox_SelectionChanged;
        }

        private async void LoadSubjects()
   {
        try
            {
     _subjects = await _quizService.GetAllSubjectsAsync();
        SubjectComboBox.ItemsSource = _subjects;
      
        if (_subjects.Any())
        {
      SubjectComboBox.SelectedIndex = 0;
      }
     }
        catch (Exception ex)
      {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", 
       MessageBoxButton.OK, MessageBoxImage.Error);
    }
        }

        private async void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
if (SubjectComboBox.SelectedItem is SubjectEntity selectedSubject)
      {
     try
          {
              var questions = await _quizService.GetQuestionsBySubjectAsync(selectedSubject.SubjectId);
 AvailableQuestionsCount.Text = questions.Count.ToString();
       
 // Adjust max slider value based on available questions
       NumQuestionsSlider.Maximum = Math.Max(5, Math.Min(50, questions.Count));
   if (NumQuestionsSlider.Value > NumQuestionsSlider.Maximum)
         {
       NumQuestionsSlider.Value = NumQuestionsSlider.Maximum;
        }
           }
          catch
       {
 AvailableQuestionsCount.Text = "0";
       }
 }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
  {
      // Validate input
  if (string.IsNullOrWhiteSpace(TestNameTextBox.Text))
            {
    MessageBox.Show("Please enter a test name.", "Validation Error", 
     MessageBoxButton.OK, MessageBoxImage.Warning);
             TestNameTextBox.Focus();
       return;
 }

            if (SubjectComboBox.SelectedItem == null)
    {
   MessageBox.Show("Please select a subject.", "Validation Error", 
         MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
 }

       var selectedSubject = SubjectComboBox.SelectedItem as SubjectEntity;
      var numQuestions = (int)NumQuestionsSlider.Value;

        // Check if enough questions exist
            var availableCount = int.Parse(AvailableQuestionsCount.Text);
            if (availableCount < numQuestions)
   {
          MessageBox.Show(
      $"Not enough questions available!\n\n" +
     $"You requested {numQuestions} questions but only {availableCount} are available for this subject.\n\n" +
                $"Please add more questions to the Question Bank first, or reduce the number of questions.",
   "Insufficient Questions",
         MessageBoxButton.OK,
   MessageBoxImage.Warning);
                return;
       }

            try
            {
    // Create the test
                var newTest = new Test
                {
                TestName = TestNameTextBox.Text.Trim(),
       SubjectId = selectedSubject!.SubjectId,
         LecturerId = _lecturerId,
    NumQuestions = numQuestions,
       CreatedAt = DateTime.Now
                };

     _dbContext.Tests.Add(newTest);
          await _dbContext.SaveChangesAsync();

    CreatedTest = newTest;

       MessageBox.Show(
    $"? Test '{newTest.TestName}' created successfully!\n\n" +
         $"Subject: {selectedSubject.SubjectName}\n" +
     $"Questions: {numQuestions}",
  "Success",
          MessageBoxButton.OK,
       MessageBoxImage.Information);

     DialogResult = true;
           Close();
    }
     catch (Exception ex)
      {
          // Get the full exception details including inner exceptions
        var errorMessage = GetFullExceptionMessage(ex);
    
          // Check for common database issues and provide helpful guidance
    string helpMessage = "";
          if (errorMessage.Contains("doesn't have a default value"))
    {
    helpMessage = "\n\n?? SOLUTION: The Test table's TestID column is missing AUTO_INCREMENT.\n\n" +
             "Run this SQL in phpMyAdmin:\n" +
       "ALTER TABLE `Test` MODIFY `TestID` int(11) NOT NULL AUTO_INCREMENT;";
          }
  else if (errorMessage.Contains("foreign key constraint"))
     {
         helpMessage = "\n\n?? SOLUTION: The Subject or Lecturer doesn't exist in the database.";
        }
   else if (errorMessage.Contains("doesn't exist"))
            {
         helpMessage = "\n\n?? SOLUTION: The Test table doesn't exist.\n" +
                 "Run the SQL script: CollaborativeSoftware/SQL_SCRIPTS/complete_database_schema.sql";
                }
           
     MessageBox.Show($"Error creating test: {errorMessage}{helpMessage}", "Error", 
        MessageBoxButton.OK, MessageBoxImage.Error);
      }
     }

      private string GetFullExceptionMessage(Exception ex)
        {
            var message = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
        message += $"\n\nInner Exception: {inner.Message}";
     inner = inner.InnerException;
       }
   return message;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
      {
            DialogResult = false;
    Close();
        }
    }
}
