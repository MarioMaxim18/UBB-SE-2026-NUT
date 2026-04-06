using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Services; 
using TeamNut.Views;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private bool mealsLoaded = false;
        private bool chatLoaded = false;
        private bool shoppingListLoaded = false;
        private bool remindersLoaded = false; 

        private readonly ReminderService _reminderService = new();

        public MainPage()
        {
            this.InitializeComponent();
  
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
                else if (selectedItem == MealPlanTab)
                {
                    MealPlanFrame.Navigate(typeof(TeamNut.Views.MealPlanView.MealPlanPage));
                }
                else if (selectedItem == DailyLogTab)
                {
                    DailyLogFrame.Navigate(typeof(TeamNut.Views.CalorieLoggingView.CalorieLoggingPage));
                }
                else if (selectedItem == InventoryTab)
                {
                    InventoryFrame.Navigate(typeof(TeamNut.Views.InventoryView.InventoryPage));
                }
                else if (selectedItem == ChatTab && !chatLoaded)
                {
                    ChatFrame.Navigate(typeof(TeamNut.Views.NutritionistChat.NutritionistChatPage));
                    chatLoaded = true;
                }
                else if (selectedItem == ShoppingListTab && !shoppingListLoaded)
                {
                    ShoppingListFrame.Navigate(typeof(TeamNut.Views.ShoppingListView.ShoppingListPage));
                    shoppingListLoaded = true;
                }
                
                else if (selectedItem == RemindersTab && !remindersLoaded)
                {
                    RemindersFrame.Navigate(typeof(TeamNut.Views.RemindersView.RemindersPage));
                    remindersLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in tab selection: {ex.Message}");
            }
        }
        public TeamNut.ViewModels.RemindersViewModel ViewModel { get; } = new();
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




/*using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Views;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private bool mealsLoaded = false;
        private bool mealPlanLoaded = false;
        private bool chatLoaded = false;
        private bool shoppingListLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();

            // If current user is a nutritionist, restrict UI to chat only
            try
            {
                if (TeamNut.Models.UserSession.Role == "Nutritionist")
                {
                    MealsTab.Visibility = Visibility.Collapsed;
                    MealPlanTab.Visibility = Visibility.Collapsed;
                    DailyLogTab.Visibility = Visibility.Collapsed;
                    InventoryTab.Visibility = Visibility.Collapsed;
                    ShoppingListTab.Visibility = Visibility.Collapsed;

                    // Select and load chat tab
                    if (!chatLoaded && ChatFrame != null)
                    {
                        ChatFrame.Navigate(typeof(TeamNut.Views.NutritionistChat.NutritionistChatPage));
                        chatLoaded = true;
                    }
                    MainTabView.SelectedItem = ChatTab;
                }
            }
            catch { }
        }

        private void MealsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                else if (selectedItem == DailyLogTab)
                {
                    DailyLogFrame.Navigate(typeof(TeamNut.Views.CalorieLoggingView.CalorieLoggingPage));
                }
                else if (selectedItem == InventoryTab)
                {
                    InventoryFrame.Navigate(typeof(TeamNut.Views.InventoryView.InventoryPage));
                }
                else if (selectedItem == ChatTab && !chatLoaded)
                {
                    ChatFrame.Navigate(typeof(TeamNut.Views.NutritionistChat.NutritionistChatPage));
                    chatLoaded = true;
                }
                else if (selectedItem == ShoppingListTab && !shoppingListLoaded)
                {
                    ShoppingListFrame.Navigate(typeof(TeamNut.Views.ShoppingListView.ShoppingListPage));
                    shoppingListLoaded = true;
                }
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
}*/
