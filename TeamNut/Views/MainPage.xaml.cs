using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private bool mealsLoaded = false;
        private bool mealPlanLoaded = false;
        private bool calorieLoaded = false;
        private bool chatLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load the first tab by default
                if (!mealsLoaded && MealsFrame != null)
                {
                    MealsFrame.Navigate(typeof(TeamNut.MealsPage));
                    mealsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading initial content: {ex.Message}");
            }
        }

        private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedItem = MainTabView.SelectedItem as TabViewItem;
                if (selectedItem == null) return;

                if (selectedItem == MealsTab && !mealsLoaded)
                {
                    MealsFrame.Navigate(typeof(TeamNut.MealsPage));
                    mealsLoaded = true;
                }
                else if (selectedItem == MealPlanTab && !mealPlanLoaded)
                {
                    MealPlanFrame.Navigate(typeof(TeamNut.Views.MealPlanView.MealPlanPage));
                    mealPlanLoaded = true;
                }
                else if (selectedItem == ChatTab && !chatLoaded)
                {
                    ChatFrame.Navigate(typeof(TeamNut.Views.NutritionistChat.NutritionistChatPage));
                    chatLoaded = true;
                }
                // CalorieTab not loaded yet - page is empty
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in tab selection: {ex.Message}");
            }
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