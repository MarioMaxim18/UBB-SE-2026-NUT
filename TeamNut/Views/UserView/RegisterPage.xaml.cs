using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.ViewModels;

namespace TeamNut.Views.UserView
{
    public sealed partial class RegisterPage : Page
    {
        public UserViewModel ViewModel => App.UserViewModel;
        public RegisterPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            ViewModel.RegistrationValid += ViewModel_RegistrationValid;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pwBox && ViewModel.CurrentUser != null)
            {
                ViewModel.CurrentUser.Password = pwBox.Password;
            }
        }
        private void ToLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(LoginPage));
            }
        }
        private void ViewModel_RegistrationValid(object? sender, EventArgs e)
        {
            this.Frame?.Navigate(typeof(UserDataPage), ViewModel.CurrentUser);
        }
    }
}
