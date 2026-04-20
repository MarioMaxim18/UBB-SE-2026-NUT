using Microsoft.UI.Xaml;
using TeamNut.ViewModels;
using TeamNut.Views.UserView;

namespace TeamNut
{
    public partial class App : Application
    {
        internal Window? window;

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
            window = new MainWindow();
            window.Content = new UserView();
            window.Activate();
        }
    }
}
