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


namespace TeamNut.Views.UserView
{
    public sealed partial class UserDataPage : Page
    {
        public UserViewModel ViewModel => App.UserViewModel;
        public UserDataPage()
        {
            InitializeComponent();
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

        private void ViewModel_SaveDataSuccess(object sender, EventArgs e)
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
