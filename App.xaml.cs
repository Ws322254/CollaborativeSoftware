using System.Windows;
using WpfApp2.Data;

namespace WpfApp2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    try
                    {
                        // Force delete old database if it exists (fresh start)
                        context.Database.EnsureDeleted();
                    }
                    catch { /* Ignore if database doesn't exist */ }

                    // Create fresh database
                    context.Database.EnsureCreated();

                    // Initialize database with test data
                    DatabaseInitializer.InitializeTestData(context);

                    // Display user count
                    var userCount = context.Users.Count();
                    var users = context.Users.ToList();

                    System.Diagnostics.Debug.WriteLine($"\n? DATABASE READY!");
                    System.Diagnostics.Debug.WriteLine($"Total users: {userCount}\n");
                    foreach (var user in users)
                    {
                        System.Diagnostics.Debug.WriteLine($"? {user.Username,-12} | Role: {user.Role,-10} | Email: {user.Email}");
                    }
                    System.Diagnostics.Debug.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database initialization error: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            try
            {
                // Show RoleSelectionWindow
                RoleSelectionWindow roleWindow = new RoleSelectionWindow();
                this.MainWindow = roleWindow;
                roleWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading role selection window: {ex.Message}\n\n{ex.InnerException?.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Shutdown();
            }
        }
    }
}
