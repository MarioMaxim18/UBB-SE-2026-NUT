using Microsoft.UI.Xaml;
using TeamNut.ViewModels;
using TeamNut.Views.UserView;

namespace TeamNut
{
    /// <summary>Application entry point and lifecycle host.</summary>
    public partial class App : Application
    {
        /// <summary>The application's main window instance.</summary>
        internal Window? AppWindow;

        /// <summary>Gets the shared user view model.</summary>
        public static UserViewModel UserViewModel { get; } = new UserViewModel();

        /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
        public App()
        {
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled: {e.Exception}");
            };

            InitializeComponent();
        }

        /// <summary>Creates and activates the main window when the application launches.</summary>
        /// <param name="args">Launch activation event arguments.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            AppWindow = new MainWindow();
            AppWindow.Content = new UserView();
            AppWindow.Activate();
        }
    }
}
