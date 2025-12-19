using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CollaborativeSoftware.Data;
using CollaborativeSoftware.Models;
using CollaborativeSoftware.Services;

namespace CollaborativeSoftware
{
    public partial class AdminDashboardWindow : Window
    {
        private readonly ApplicationUser _currentUser;
        private readonly MySqlDbContext _dbContext;
        private readonly MySqlQuizService _quizService;
        private List<SubjectEntity> _allSubjects = new();
        private List<Lecturer> _allLecturers = new();

        public AdminDashboardWindow(ApplicationUser user)
        {
            try
            {
                InitializeComponent();
                _currentUser = user;
                _dbContext = new MySqlDbContext();
                _quizService = new MySqlQuizService(_dbContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Admin Dashboard: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllData()
        {
            await LoadSubjects();
            await LoadLecturers();
        }

        private async Task LoadSubjects()
        {
            try
            {
                _allSubjects = await _quizService.GetAllSubjectsIncludingInactiveAsync();
                SubjectsDataGrid.ItemsSource = _allSubjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLecturers()
        {
            try
            {
                _allLecturers = await _quizService.GetAllLecturersAsync();
                LecturersDataGrid.ItemsSource = _allLecturers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading lecturers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Subject management
        private async void CreateSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            string? subjectName = PromptInput("Enter new subject name:");
            if (!string.IsNullOrWhiteSpace(subjectName))
            {
                try
                {
                    await _quizService.CreateSubjectAsync(subjectName.Trim());
                    MessageBox.Show($"Subject '{subjectName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadSubjects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating subject: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsDataGrid.SelectedItem is SubjectEntity selectedSubject)
            {
                string? newName = PromptInput("Edit subject name:", selectedSubject.SubjectName);
                if (!string.IsNullOrWhiteSpace(newName) && newName != selectedSubject.SubjectName)
                {
                    try
                    {
                        bool success = await _quizService.UpdateSubjectAsync(selectedSubject.SubjectId, newName.Trim());
                        if (success)
                        {
                            MessageBox.Show("Subject updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadSubjects();
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

        private async void ToggleSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsDataGrid.SelectedItem is SubjectEntity selectedSubject)
            {
                string newStatus = selectedSubject.IsActive ? "inactive" : "active";
                MessageBoxResult result = MessageBox.Show(
                    $"Set subject '{selectedSubject.SubjectName}' to {newStatus}?",
                    "Confirm Toggle",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _quizService.ToggleSubjectActiveAsync(selectedSubject.SubjectId);
                        if (success)
                        {
                            MessageBox.Show($"Subject is now {newStatus}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadSubjects();
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

        // Lecturer management
        private async void CreateLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LecturerCreateDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _quizService.CreateLecturerAsync(
                        dialog.FirstName,
                        dialog.LastName,
                        dialog.Email,
                        HashPassword(dialog.Password));

                    MessageBox.Show("Lecturer created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadLecturers();
                }
                catch (Exception ex)
                {
                    var message = $"Error creating lecturer: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        message += $"\n\nInner Exception: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            if (LecturersDataGrid.SelectedItem is Lecturer selectedLecturer)
            {
                var dialog = new LecturerEditDialog(selectedLecturer);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        bool success = await _quizService.UpdateLecturerAsync(
                            selectedLecturer.LecturerId,
                            dialog.FirstName,
                            dialog.LastName,
                            dialog.Email);

                        if (success)
                        {
                            MessageBox.Show("Lecturer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadLecturers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating lecturer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lecturer to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (LecturersDataGrid.SelectedItem is Lecturer selectedLecturer)
            {
                string? newPassword = PromptInput("Enter new password:");
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBoxResult confirm = MessageBox.Show(
                        $"Reset password for {selectedLecturer.FirstName} {selectedLecturer.LastName}?",
                        "Confirm Password Reset",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        try
                        {
                            bool success = await _quizService.ResetLecturerPasswordAsync(
                                selectedLecturer.LecturerId,
                                HashPassword(newPassword));

                            if (success)
                            {
                                MessageBox.Show("Password reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadLecturers();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error resetting password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lecturer to reset password.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DisableLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            if (LecturersDataGrid.SelectedItem is Lecturer selectedLecturer)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Disable lecturer {selectedLecturer.FirstName} {selectedLecturer.LastName}?\n\nThey will no longer be able to log in.",
                    "Confirm Disable",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _quizService.DisableLecturerAsync(selectedLecturer.LecturerId);
                        if (success)
                        {
                            MessageBox.Show("Lecturer disabled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadLecturers();
                        }
                        else
                        {
                            MessageBox.Show("Note: Disable functionality not available - IsActive column not found in database.", 
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling lecturer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lecturer to disable.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            if (LecturersDataGrid.SelectedItem is Lecturer selectedLecturer)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Delete lecturer {selectedLecturer.FirstName} {selectedLecturer.LastName}?\n\nThis action cannot be undone!",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _quizService.DeleteLecturerAsync(selectedLecturer.LecturerId);
                        if (success)
                        {
                            MessageBox.Show("Lecturer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadLecturers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting lecturer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lecturer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // System settings
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RoleSelectionWindow roleSelection = new RoleSelectionWindow();
                roleSelection.Show();
                this.Close();
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

            cancelButton.Click += (s, ex) => { window.DialogResult = false; };
            okButton.Click += (s, ex) => { window.DialogResult = true; };
            textBox.Focus();

            return window.ShowDialog() == true ? textBox.Text : null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
