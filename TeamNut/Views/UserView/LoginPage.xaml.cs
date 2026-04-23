// <copyright file="LoginPage.xaml.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Views.UserView
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using TeamNut.ViewModels;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// LoginPage.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public UserViewModel ViewModel { get; }

        public LoginPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserViewModel>();
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
