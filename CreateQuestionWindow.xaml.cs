using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;

namespace CollaborativeSoftware
{
    public partial class CreateQuestionWindow : Window
    {
        private readonly MySqlDbContext _dbContext;
        private readonly MySqlQuizService _quizService;
        private List<SubjectEntity> _subjects = new();
        private int _questionsCreated = 0;

        public QuestionEntity? CreatedQuestion { get; private set; }
        public int QuestionsCreatedCount => _questionsCreated;

        public CreateQuestionWindow()
        {
            InitializeComponent();
            _dbContext = new MySqlDbContext();
            _quizService = new MySqlQuizService(_dbContext);
    
     LoadSubjects();
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

        private string GetSelectedCorrectAnswer()
        {
            if (AnswerA.IsChecked == true) return "A";
            if (AnswerB.IsChecked == true) return "B";
            if (AnswerC.IsChecked == true) return "C";
            if (AnswerD.IsChecked == true) return "D";
   return string.Empty;
   }

        private bool ValidateInput()
        {
 if (SubjectComboBox.SelectedItem == null)
    {
     MessageBox.Show("Please select a subject.", "Validation Error", 
      MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
     }

            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text))
    {
           MessageBox.Show("Please enter the question text.", "Validation Error", 
         MessageBoxButton.OK, MessageBoxImage.Warning);
       QuestionTextBox.Focus();
         return false;
   }

      if (string.IsNullOrWhiteSpace(OptionATextBox.Text))
     {
        MessageBox.Show("Please enter Option A.", "Validation Error", 
         MessageBoxButton.OK, MessageBoxImage.Warning);
    OptionATextBox.Focus();
             return false;
  }

            if (string.IsNullOrWhiteSpace(OptionBTextBox.Text))
         {
                MessageBox.Show("Please enter Option B.", "Validation Error", 
     MessageBoxButton.OK, MessageBoxImage.Warning);
          OptionBTextBox.Focus();
      return false;
            }

   if (string.IsNullOrWhiteSpace(OptionCTextBox.Text))
       {
             MessageBox.Show("Please enter Option C.", "Validation Error", 
     MessageBoxButton.OK, MessageBoxImage.Warning);
     OptionCTextBox.Focus();
         return false;
    }

          if (string.IsNullOrWhiteSpace(OptionDTextBox.Text))
      {
    MessageBox.Show("Please enter Option D.", "Validation Error", 
    MessageBoxButton.OK, MessageBoxImage.Warning);
 OptionDTextBox.Focus();
     return false;
            }

 if (string.IsNullOrEmpty(GetSelectedCorrectAnswer()))
          {
        MessageBox.Show("Please select the correct answer.", "Validation Error", 
     MessageBoxButton.OK, MessageBoxImage.Warning);
   return false;
  }

    return true;
 }

        private void ClearForm()
        {
     QuestionTextBox.Text = string.Empty;
            OptionATextBox.Text = string.Empty;
    OptionBTextBox.Text = string.Empty;
   OptionCTextBox.Text = string.Empty;
      OptionDTextBox.Text = string.Empty;
  AnswerA.IsChecked = false;
            AnswerB.IsChecked = false;
            AnswerC.IsChecked = false;
            AnswerD.IsChecked = false;
            QuestionTextBox.Focus();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
    if (!ValidateInput())
        return;

          var selectedSubject = SubjectComboBox.SelectedItem as SubjectEntity;

            try
         {
  var newQuestion = new QuestionEntity
      {
              SubjectId = selectedSubject!.SubjectId,
        QuestionText = QuestionTextBox.Text.Trim(),
  OptionA = OptionATextBox.Text.Trim(),
           OptionB = OptionBTextBox.Text.Trim(),
        OptionC = OptionCTextBox.Text.Trim(),
    OptionD = OptionDTextBox.Text.Trim(),
       CorrectAnswer = GetSelectedCorrectAnswer()
 };

          _dbContext.Questions.Add(newQuestion);
          await _dbContext.SaveChangesAsync();

         CreatedQuestion = newQuestion;
     _questionsCreated++;

          if (AddAnotherCheckBox.IsChecked == true)
        {
 MessageBox.Show(
       $"? Question saved successfully!\n\n" +
      $"Subject: {selectedSubject.SubjectName}\n" +
     $"Correct Answer: {newQuestion.CorrectAnswer}\n\n" +
               $"Total questions added: {_questionsCreated}",
        "Question Saved",
        MessageBoxButton.OK,
            MessageBoxImage.Information);
       
      ClearForm();
      }
     else
        {
       MessageBox.Show(
         $"? Question saved successfully!\n\n" +
   $"Subject: {selectedSubject.SubjectName}\n" +
     $"Correct Answer: {newQuestion.CorrectAnswer}",
  "Success",
  MessageBoxButton.OK,
          MessageBoxImage.Information);

      DialogResult = true;
        Close();
    }
      }
            catch (Exception ex)
         {
   // Get the full exception details including inner exceptions
      var errorMessage = GetFullExceptionMessage(ex);
     
          // Check for common database issues and provide helpful guidance
   string helpMessage = "";
          if (errorMessage.Contains("Unknown column") || errorMessage.Contains("doesn't exist"))
     {
             helpMessage = "\n\n?? SOLUTION: The Question table is missing required columns.\n" +
           "Please run the SQL script in phpMyAdmin:\n" +
        "CollaborativeSoftware/SQL_SCRIPTS/create_question_table.sql";
     }
     else if (errorMessage.Contains("foreign key constraint"))
       {
        helpMessage = "\n\n?? SOLUTION: The Subject doesn't exist in the database.\n" +
    "Make sure the Subject table has the selected subject.";
              }
      else if (errorMessage.Contains("Table") && errorMessage.Contains("doesn't exist"))
           {
          helpMessage = "\n\n?? SOLUTION: The Question table doesn't exist.\n" +
        "Please run the SQL script in phpMyAdmin:\n" +
     "CollaborativeSoftware/SQL_SCRIPTS/complete_database_schema.sql";
      }
      
   MessageBox.Show($"Error saving question: {errorMessage}{helpMessage}", "Error", 
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
            if (_questionsCreated > 0)
    {
      DialogResult = true; // Questions were added
   }
            else
         {
       DialogResult = false;
            }
     Close();
     }
    }
}
