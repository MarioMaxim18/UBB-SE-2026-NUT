using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Views.MealPlanView;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Navigate to Daily Meal Plan page by default
            ContentContainer.Navigate(typeof(DailyMealPlanPage));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {

            UserSession.Logout();

            if (Application.Current is App app && app._window != null)
            {
                app._window.Content = new TeamNut.Views.UserView.UserView();
            }
        }
    }
}