using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Views;
using TeamNut.Views.UserView;

namespace TeamNut
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            Frame rootFrame = new Frame();
            _window.Content = rootFrame;

            rootFrame.Navigate(typeof(MealsPage)); // 👈 pra testar meals

            _window.Activate();
        }
    }
}