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
    public partial class EditTestWindow : Window
    {
     private readonly MySqlDbContext _dbContext;
      private readonly MySqlQuizService _quizService;
 private readonly Test _test;
  private List<SubjectEntity> _subjects = new();

     public bool TestUpdated { get; private set; }

  public EditTestWindow(Test test)
   {
            InitializeComponent();
    _test = test;
   _dbContext = new MySqlDbContext();
_quizService = new MySqlQuizService(_dbContext);
          
      LoadSubjects();
      PopulateForm();

     // Update slider display
NumQuestionsSlider.ValueChanged += (s, e) => 
  {
           NumQuestionsDisplay.Text = $"{(int)NumQuestionsSlider.Value} questions";
     };
     }

        private async void LoadSubjects()
 {
            try
          {
       _subjects = await _quizService.GetAllSubjectsAsync();
    SubjectComboBox.ItemsSource = _subjects;
                
              // Select the test's current subject
     var currentSubject = _subjects.FirstOrDefault(s => s.SubjectId == _test.SubjectId);
   if (currentSubject != null)
    {
     SubjectComboBox.SelectedItem = currentSubject;
    }
    }
 catch (Exception ex)
   {
            MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", 
     MessageBoxButton.OK, MessageBoxImage.Error);
     }
        }

    private void PopulateForm()
        {
   TestIdText.Text = _test.TestId.ToString();
    TestNameTextBox.Text = _test.TestName;
      NumQuestionsSlider.Value = _test.NumQuestions;
       NumQuestionsDisplay.Text = $"{_test.NumQuestions} questions";
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

     private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
        // Find and update the test in the database
     var testToUpdate = await _dbContext.Tests.FindAsync(_test.TestId);
      
          if (testToUpdate != null)
       {
     testToUpdate.TestName = TestNameTextBox.Text.Trim();
          testToUpdate.SubjectId = selectedSubject!.SubjectId;
   testToUpdate.NumQuestions = numQuestions;

            await _dbContext.SaveChangesAsync();

         TestUpdated = true;

 MessageBox.Show(
          $"? Test '{testToUpdate.TestName}' updated successfully!",
       "Success",
         MessageBoxButton.OK,
         MessageBoxImage.Information);

          DialogResult = true;
      Close();
        }
  else
        {
    MessageBox.Show("Test not found in database.", "Error", 
  MessageBoxButton.OK, MessageBoxImage.Error);
 }
            }
    catch (Exception ex)
   {
   MessageBox.Show($"Error updating test: {ex.Message}", "Error", 
          MessageBoxButton.OK, MessageBoxImage.Error);
  }
        }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        DialogResult = false;
       Close();
        }
    }
}
