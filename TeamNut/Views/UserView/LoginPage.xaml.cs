using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.UserView
{
    /// <summary>Page for user login.</summary>
    public sealed partial class LoginPage : Page
    {
        /// <summary>Gets the shared user view model.</summary>
        public UserViewModel ViewModel { get; }

        /// <summary>Initializes a new instance of the <see cref="LoginPage"/> class.</summary>
        public LoginPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserViewModel>();
            this.DataContext = ViewModel;
        }

        /// <summary>Subscribes to login events when navigated to.</summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.StatusMessage = string.Empty;
            ViewModel.LoginSuccess += ViewModel_LoginSuccess;
        }

        /// <summary>Unsubscribes from login events when navigated away.</summary>
        /// <param name="e">Navigation event arguments.</param>
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

        /// <summary>Navigates to the register page.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
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
