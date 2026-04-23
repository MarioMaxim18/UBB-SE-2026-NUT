// <copyright file="UserDataPage.xaml.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Views.UserView
{
    using System;
    using System.Linq;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using TeamNut.ViewModels;
    using TeamNut.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// UserDataPage.
    /// </summary>
    public sealed partial class UserDataPage : Page
    {
        public UserViewModel ViewModel { get; }

        private readonly IValidationService validationService;

        public UserDataPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserViewModel>();
            validationService = App.Services.GetRequiredService<IValidationService>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.StatusMessage = string.Empty;
            ViewModel.SaveDataSuccess += ViewModel_SaveDataSuccess;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.SaveDataSuccess -= ViewModel_SaveDataSuccess;
        }

        private void ViewModel_SaveDataSuccess(object? sender, EventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(TeamNut.Views.MainPage));
            }
        }

        private void NumberInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = !validationService.IsNumericOnly(args.NewText);
        }
    }
}
