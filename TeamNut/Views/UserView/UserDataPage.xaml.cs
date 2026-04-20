using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.ViewModels;

namespace TeamNut.Views.UserView
{
    /// <summary>Page for entering physical health data during registration.</summary>
    public sealed partial class UserDataPage : Page
    {
        /// <summary>Gets the shared user view model.</summary>
        public UserViewModel ViewModel => App.UserViewModel;

        /// <summary>Initializes a new instance of the <see cref="UserDataPage"/> class.</summary>
        public UserDataPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        /// <summary>Subscribes to save-data events when navigated to.</summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.StatusMessage = string.Empty;
            ViewModel.SaveDataSuccess += ViewModel_SaveDataSuccess;
        }

        /// <summary>Unsubscribes from save-data events when navigated away.</summary>
        /// <param name="e">Navigation event arguments.</param>
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
