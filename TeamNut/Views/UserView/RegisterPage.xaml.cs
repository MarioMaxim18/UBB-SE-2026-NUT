using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TeamNut.ViewModels; 
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;



namespace TeamNut.Views.UserView
{
    public sealed partial class RegisterPage : Page
    {
        public UserViewModel ViewModel { get; }
        public RegisterPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserViewModel>();
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

        private void ViewModel_LoginSuccess(object sender, EventArgs e)
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
        private void ViewModel_RegistrationValid(object sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(UserDataPage), ViewModel.CurrentUser);
        }
    }
}
