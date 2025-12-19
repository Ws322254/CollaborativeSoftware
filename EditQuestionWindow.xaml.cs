using System.Windows;
using WpfApp2.Data;
using WpfApp2.Models;
using WpfApp2.Services;

namespace WpfApp2
{
    public partial class EditQuestionWindow : Window
    {
 private readonly MySqlDbContext _dbContext;
  private readonly MySqlQuizService _quizService;
        private readonly QuestionEntity _question;
        private List<SubjectEntity> _subjects = new();

      public bool QuestionUpdated { get; private set; }

        public EditQuestionWindow(QuestionEntity question)
        {
  InitializeComponent();
 _question = question;
  _dbContext = new MySqlDbContext();
 _quizService = new MySqlQuizService(_dbContext);
            
 LoadSubjects();
   PopulateForm();
        }

        private async void LoadSubjects()
        {
      try
  {
              _subjects = await _quizService.GetAllSubjectsAsync();
      SubjectComboBox.ItemsSource = _subjects;
 
    // Select the question's current subject
       var currentSubject = _subjects.FirstOrDefault(s => s.SubjectId == _question.SubjectId);
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
   QuestionIdText.Text = _question.QuestionId.ToString();
      QuestionTextBox.Text = _question.QuestionText;
            OptionATextBox.Text = _question.OptionA;
     OptionBTextBox.Text = _question.OptionB;
OptionCTextBox.Text = _question.OptionC;
  OptionDTextBox.Text = _question.OptionD;

       // Set correct answer
      switch (_question.CorrectAnswer?.ToUpper())
{
      case "A": AnswerA.IsChecked = true; break;
        case "B": AnswerB.IsChecked = true; break;
          case "C": AnswerC.IsChecked = true; break;
    case "D": AnswerD.IsChecked = true; break;
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

 if (string.IsNullOrWhiteSpace(OptionATextBox.Text) ||
      string.IsNullOrWhiteSpace(OptionBTextBox.Text) ||
     string.IsNullOrWhiteSpace(OptionCTextBox.Text) ||
     string.IsNullOrWhiteSpace(OptionDTextBox.Text))
            {
  MessageBox.Show("Please fill in all answer options.", "Validation Error", 
       MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
  {
   if (!ValidateInput())
    return;

     var selectedSubject = SubjectComboBox.SelectedItem as SubjectEntity;

    try
         {
     // Find and update the question in the database
var questionToUpdate = await _dbContext.Questions.FindAsync(_question.QuestionId);
  
     if (questionToUpdate != null)
   {
  questionToUpdate.SubjectId = selectedSubject!.SubjectId;
               questionToUpdate.QuestionText = QuestionTextBox.Text.Trim();
        questionToUpdate.OptionA = OptionATextBox.Text.Trim();
    questionToUpdate.OptionB = OptionBTextBox.Text.Trim();
         questionToUpdate.OptionC = OptionCTextBox.Text.Trim();
questionToUpdate.OptionD = OptionDTextBox.Text.Trim();
 questionToUpdate.CorrectAnswer = GetSelectedCorrectAnswer();

    await _dbContext.SaveChangesAsync();

    QuestionUpdated = true;

  MessageBox.Show(
  "Question updated successfully!",
  "Success",
  MessageBoxButton.OK,
          MessageBoxImage.Information);

  DialogResult = true;
    Close();
     }
    else
 {
         MessageBox.Show("Question not found in database.", "Error", 
        MessageBoxButton.OK, MessageBoxImage.Error);
    }
 }
      catch (Exception ex)
      {
         MessageBox.Show($"Error updating question: {ex.Message}", "Error", 
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
