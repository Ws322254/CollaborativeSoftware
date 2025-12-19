using System.Windows;
using System.Windows.Controls;
using WpfApp2.Data;
using WpfApp2.Models;
using WpfApp2.Services;

namespace WpfApp2
{
    public partial class MySqlQuizWindow : Window
    {
        private readonly ApplicationUser _currentUser;
        private readonly Test _currentTest;
        private readonly MySqlDbContext _dbContext;
        private readonly MySqlQuizService _quizService;

        private List<QuestionEntity> _questions = new();
        private int _currentQuestionIndex = 0;
        private Dictionary<int, string> _selectedAnswers = new();
        private TestAttempt? _testAttempt;

        public MySqlQuizWindow(ApplicationUser user, Test test)
        {
            InitializeComponent();
            _currentUser = user;
            _currentTest = test;
            _dbContext = new MySqlDbContext();
            _quizService = new MySqlQuizService(_dbContext);
            TestNameBlock.Text = test.TestName;
            SubjectBlock.Text = test.Subject?.SubjectName ?? "Unknown Subject";
            LoadTest();
        }

        private async void LoadTest()
        {
            try
            {
                _questions = await _quizService.GetRandomQuestionsAsync(_currentTest.SubjectId, _currentTest.NumQuestions);

                if (_questions.Count == 0)
                {
                    MessageBox.Show(
"No questions available for this test.\n\nPlease ask a lecturer to add questions to the Question Bank.",
                        "No Questions",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    ReturnToDashboard();
                    return;
                }

                _testAttempt = await _quizService.CreateTestAttemptAsync(_currentUser.Id, _currentTest.TestId);
                DisplayQuestion(0);
            }
            catch (Exception ex)
            {
                var errorMessage = GetFullExceptionMessage(ex);

                string helpMessage = "";
                if (errorMessage.Contains("doesn't have a default value"))
                {
                    helpMessage = "\n\nSOLUTION: Run this SQL in phpMyAdmin:\n\n" +
          "ALTER TABLE `TestAttempt` CHANGE `AttemptID` `AttemptID` int(11) NOT NULL AUTO_INCREMENT;\n" +
         "ALTER TABLE `StudentAnswer` CHANGE `StudentAnswerID` `StudentAnswerID` int(11) NOT NULL AUTO_INCREMENT;";
                }

                MessageBox.Show($"Error loading test: {errorMessage}{helpMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ReturnToDashboard();
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

        private void ReturnToDashboard()
        {
            var dashboard = new StudentDashboardWindow(_currentUser);
            dashboard.Show();
            Close();
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _questions.Count) return;

            _currentQuestionIndex = index;
            var question = _questions[index];

            QuestionCounterBlock.Text = $"Question {index + 1} of {_questions.Count}";

            double progress = (double)(index + 1) / _questions.Count;
            ProgressFill.Width = progress * (ActualWidth - 60);

            QuestionTextBlock.Text = question.QuestionText;

            OptionA.Content = $"A) {question.OptionA}";
            OptionB.Content = $"B) {question.OptionB}";
            OptionC.Content = $"C) {question.OptionC}";
            OptionD.Content = $"D) {question.OptionD}";

            OptionA.IsChecked = false;
            OptionB.IsChecked = false;
            OptionC.IsChecked = false;
            OptionD.IsChecked = false;

            if (_selectedAnswers.TryGetValue(question.QuestionId, out string? previousAnswer))
            {
                switch (previousAnswer)
                {
                    case "A": OptionA.IsChecked = true; break;
                    case "B": OptionB.IsChecked = true; break;
                    case "C": OptionC.IsChecked = true; break;
                    case "D": OptionD.IsChecked = true; break;
                }
            }

            PreviousButton.IsEnabled = index > 0;

            if (index == _questions.Count - 1)
            {
                NextButton.Visibility = Visibility.Collapsed;
                SubmitButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextButton.Visibility = Visibility.Visible;
                SubmitButton.Visibility = Visibility.Collapsed;
            }

            int answeredCount = _selectedAnswers.Count;
            AnswerStatusBlock.Text = $"{answeredCount} of {_questions.Count} answered";
        }

        private void SaveCurrentAnswer()
        {
            var question = _questions[_currentQuestionIndex];
            string? selectedAnswer = null;

            if (OptionA.IsChecked == true) selectedAnswer = "A";
            else if (OptionB.IsChecked == true) selectedAnswer = "B";
            else if (OptionC.IsChecked == true) selectedAnswer = "C";
            else if (OptionD.IsChecked == true) selectedAnswer = "D";

            if (selectedAnswer != null)
            {
                _selectedAnswers[question.QuestionId] = selectedAnswer;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentAnswer();
            if (_currentQuestionIndex > 0)
            {
                DisplayQuestion(_currentQuestionIndex - 1);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentAnswer();
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                DisplayQuestion(_currentQuestionIndex + 1);
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentAnswer();

            if (_selectedAnswers.Count < _questions.Count)
            {
                var result = MessageBox.Show(
  $"You have only answered {_selectedAnswers.Count} of {_questions.Count} questions.\n\n" +
           "Unanswered questions will be marked as incorrect.\n\n" +
           "Are you sure you want to submit?",
   "Incomplete Test",
       MessageBoxButton.YesNo,
     MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                foreach (var question in _questions)
                {
                    string answer = _selectedAnswers.TryGetValue(question.QuestionId, out string? a) ? a : "";
                    await _quizService.RecordStudentAnswerAsync(_testAttempt!.AttemptId, question.QuestionId, answer);
                }

                var completedAttempt = await _quizService.CompleteTestAttemptAsync(_testAttempt!.AttemptId);

                int correctCount = _questions.Count(q =>
   _selectedAnswers.TryGetValue(q.QuestionId, out string? ans) &&
      ans?.ToUpper() == q.CorrectAnswer?.ToUpper());

                string status = completedAttempt.ScorePercentage >= 50 ? "PASSED" : "FAILED";
                string statusMessage = completedAttempt.ScorePercentage >= 50 ? "Congratulations!" : "Keep practicing!";

                var resultMessage = $"Test Completed!\n" +
               $"===============================\n\n" +
        $"Score: {completedAttempt.ScorePercentage:F1}%\n" +
          $"Correct Answers: {correctCount} / {_questions.Count}\n" +
          $"Points Earned: {completedAttempt.TotalPoints}\n\n" +
       $"Status: {status}\n\n" +
      $"{statusMessage}";

                MessageBox.Show(resultMessage, "Test Results", MessageBoxButton.OK,
         completedAttempt.ScorePercentage >= 50 ? MessageBoxImage.Information : MessageBoxImage.Warning);

                ReturnToDashboard();
            }
            catch (Exception ex)
            {
                var errorMessage = GetFullExceptionMessage(ex);

                string helpMessage = "";
                if (errorMessage.Contains("doesn't have a default value"))
                {
                    helpMessage = "\n\nSOLUTION: Run this SQL in phpMyAdmin:\n\n" +
         "ALTER TABLE `StudentAnswer` CHANGE `StudentAnswerID` `StudentAnswerID` int(11) NOT NULL AUTO_INCREMENT;";
                }

                MessageBox.Show($"Error submitting test: {errorMessage}{helpMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_testAttempt != null && _selectedAnswers.Count > 0 && _selectedAnswers.Count < _questions.Count)
            {
                var result = MessageBox.Show(
         "You haven't completed the test.\n\nAre you sure you want to exit? Your progress will be lost.",
       "Exit Test?",
        MessageBoxButton.YesNo,
     MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
    }
}
