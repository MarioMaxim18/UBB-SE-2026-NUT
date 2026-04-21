using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.UserView
{
    /// <summary>Page for entering physical health data during registration.</summary>
    public sealed partial class UserDataPage : Page
    {
        public UserViewModel ViewModel { get; }

        public UserDataPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserViewModel>();
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
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
