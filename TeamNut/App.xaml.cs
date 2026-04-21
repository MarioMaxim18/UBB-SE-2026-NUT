using Microsoft.UI.Xaml;
using TeamNut.ViewModels;
using TeamNut.Views.UserView;

namespace TeamNut
{
    /// <summary>Application entry point and lifecycle host.</summary>
    public partial class App : Application
    {
        internal Window? AppWindow;

        public static UserViewModel UserViewModel { get; } = new UserViewModel();

        public App()
        {
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled: {e.Exception}");
            };

            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            AppWindow = new MainWindow();
            AppWindow.Content = new UserView();
            AppWindow.Activate();
        }
    }
}
