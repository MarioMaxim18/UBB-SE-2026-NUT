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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TeamNut.Views.UserView
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public UserViewModel ViewModel => App.MainViewModel;
        public RegisterPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            ViewModel.RegistrationValid += ViewModel_RegistrationValid;
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
