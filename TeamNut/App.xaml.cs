using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;
using TeamNut.Views;
using TeamNut.Views.UserView;

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
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            _window.Content = new MealsPage();

            _window.Activate();
        }
    }
}