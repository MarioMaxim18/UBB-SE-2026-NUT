namespace TeamNut.Views.UserView
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using TeamNut.ViewModels;

    /// <summary>
    /// RegisterPage.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public UserViewModel ViewModel { get; }

        public RegisterPage()
        {
            InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<UserViewModel>();
            this.DataContext = this.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.LoginSuccess += this.ViewModel_LoginSuccess;
            this.ViewModel.RegistrationValid += this.ViewModel_RegistrationValid;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.ViewModel.LoginSuccess -= this.ViewModel_LoginSuccess;
            this.ViewModel.RegistrationValid -= this.ViewModel_RegistrationValid;
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
            if (sender is PasswordBox pwBox && this.ViewModel.CurrentUser != null)
            {
                this.ViewModel.CurrentUser.Password = pwBox.Password;
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
            this.Frame?.Navigate(typeof(UserDataPage), this.ViewModel.CurrentUser);
        }
    }
}
