using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using TeamNut.ViewModels; 


namespace TeamNut.Views.UserView
{
    public sealed partial class LoginPage : Page
    {
        public UserViewModel ViewModel => App.UserViewModel;
        public LoginPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.StatusMessage = string.Empty;
            ViewModel.LoginSuccess += ViewModel_LoginSuccess;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.LoginSuccess -= ViewModel_LoginSuccess;
        }

        private void ViewModel_LoginSuccess(object? sender, EventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(TeamNut.Views.MainPage));
            }
        }

        public void ToRegister_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RegisterPage));
            }
        }
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pwBox && ViewModel.CurrentUser != null)
            {
                ViewModel.CurrentUser.Password = pwBox.Password;
            }
        }
    }
}
