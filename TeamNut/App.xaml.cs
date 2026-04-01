using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;
using TeamNut.Views;
using TeamNut.Views.UserView;
using System.Diagnostics;

namespace TeamNut
{
    public partial class App : Application
    {
        internal Window? _window;

        public static UserViewModel UserViewModel { get; } = new UserViewModel();

        public App()
        {
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled: {e.Exception}");
            };

            InitializeComponent();

            // Ensure LocalDB instance is running
            EnsureLocalDbStarted();
        }

        private void EnsureLocalDbStarted()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "start MSSQLLocalDB",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit();
            }
            catch
            {
                // If starting fails, the app will show connection error when trying to connect
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            _window.Content = new UserView();

            _window.Activate();
        }
    }
}
