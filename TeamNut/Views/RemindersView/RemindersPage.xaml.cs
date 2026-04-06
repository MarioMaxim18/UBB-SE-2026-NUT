using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.ViewModels;
using System;

namespace TeamNut.Views.RemindersView
{
    public sealed partial class RemindersPage : Page
    {
       
        //public RemindersViewModel ViewModel { get; }
        public TeamNut.ViewModels.RemindersViewModel ViewModel { get; } = new();
        public RemindersPage()
        {
            this.InitializeComponent();

            
            ViewModel = new RemindersViewModel();

            
            this.Loaded += async (s, e) =>
            {
                await ViewModel.LoadReminders();
            };
        }

        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel != null)
            {
                await ViewModel.LoadReminders();
            }
        }
    }
}