using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.ViewModels;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private bool mealsLoaded = false;
        private bool chatLoaded = false;
        private bool shoppingListLoaded = false;
        private bool remindersLoaded = false;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcher;
        private readonly Microsoft.UI.Xaml.DispatcherTimer reminderTimer;
        private readonly HashSet<int> shownReminders = new HashSet<int>();
        private readonly ReminderService reminderService = new ReminderService();

        public MainViewModel ViewModel { get; } = new MainViewModel();

        public RemindersViewModel RemindersViewModel { get; } = new RemindersViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _ = ViewModel.LoadHeaderData();
            LoadTopReminder();
            ReminderService.RemindersChanged += OnRemindersChanged;

            reminderTimer = new Microsoft.UI.Xaml.DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(30);
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        private async void ReminderTimer_Tick(object? sender, object? e)
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0)
                {
                    return;
                }

                var reminders = await reminderService.GetUserReminders(userId);
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var now = DateTime.Now.TimeOfDay;

                foreach (var rem in reminders)
                {
                    if (rem == null)
                    {
                        continue;
                    }
                    if (rem.ReminderDate != today)
                    {
                        continue;
                    }
                    if (shownReminders.Contains(rem.Id))
                    {
                        continue;
                    }

                    var diff = (rem.Time - now).Duration();
                    if (diff <= TimeSpan.FromSeconds(30))
                    {
                        shownReminders.Add(rem.Id);
                        await ShowReminderDialog(rem);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReminderTimer_Tick: {ex}");
            }
        }

        private async Task ShowReminderDialog(Reminder rem)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = rem.Name ?? "Reminder",
                    Content = "Did you consume this meal?",
                    PrimaryButtonText = "Confirm",
                    CloseButtonText = "Decline",
                    XamlRoot = this.XamlRoot,
                };

                var res = await dialog.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    try
                    {
                        var mealService = new MealService();
                        var meals = await mealService.GetMealsAsync();
                        var matched = meals.Find(m => string.Equals(m.Name?.Trim(), rem.Name?.Trim(), StringComparison.OrdinalIgnoreCase));
                        int userId = UserSession.UserId ?? 0;

                        if (matched != null)
                        {
                            var repo = new TeamNut.Repositories.MealPlanRepository();
                            await repo.SaveMealToDailyLog(userId, matched.Id, matched.Calories);

                            var inventory = new InventoryService();
                            await inventory.ConsumeMeal(userId, matched.Id);
                        }

                        await reminderService.DeleteReminder(rem.Id);
                        ReminderService.NotifyRemindersChangedForUser(userId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error confirming reminder: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowReminderDialog: {ex}");
            }
        }

        private void OnRemindersChanged(object? sender, int userId)
        {
            try
            {
                var current = UserSession.UserId ?? 0;
                if (current != userId)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() => LoadTopReminder());
                }
                else
                {
                    LoadTopReminder();
                }
            }
            catch
            {
            }
        }

        private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedItem = MainTabView.SelectedItem as TabViewItem;
                if (selectedItem == null)
                {
                    return;
                }

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
                    try
                    {
                        RemindersFrame.Navigate(typeof(TeamNut.Views.RemindersView.RemindersPage));
                        remindersLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"NAVIGATION ERROR: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in tab selection: {ex.Message}");
            }
        }

        private async void LoadTopReminder()
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0)
                {
                    return;
                }

                var next = await reminderService.GetNextReminder(userId);
                var text = next != null ? $"{next.Name} at {next.Time:hh\\:mm}" : "No upcoming meals";

                if (MainNextReminderText != null)
                {
                    MainNextReminderText.Text = text;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top reminder: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Logout();

            if (Application.Current is App app && app.window != null)
            {
                app.window.Content = new TeamNut.Views.UserView.UserView();
            }
        }

        private async void MainNextReminderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0)
                {
                    return;
                }

                var reminder = await reminderService.GetNextReminder(userId);
                if (reminder == null)
                {
                    var noDialog = new ContentDialog
                    {
                        Title = "Reminder Details",
                        Content = "No upcoming reminders.",
                        CloseButtonText = "Close",
                        XamlRoot = this.XamlRoot,
                    };
                    await noDialog.ShowAsync();
                    return;
                }

                var panel = new StackPanel { Spacing = 8 };
                panel.Children.Add(new TextBlock { Text = "Name", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                panel.Children.Add(new TextBlock { Text = reminder.Name ?? string.Empty, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });

                panel.Children.Add(new TextBlock { Text = "Date", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                panel.Children.Add(new TextBlock { Text = reminder.ReminderDate ?? string.Empty });

                panel.Children.Add(new TextBlock { Text = "Time", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                panel.Children.Add(new TextBlock { Text = reminder.Time.ToString(@"hh\:mm") });

                panel.Children.Add(new TextBlock { Text = "Sound", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                panel.Children.Add(new TextBlock { Text = reminder.HasSound ? "On" : "Off" });

                panel.Children.Add(new TextBlock { Text = "Frequency", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                panel.Children.Add(new TextBlock { Text = reminder.Frequency ?? string.Empty });

                var dialog = new ContentDialog
                {
                    Title = "Reminder Details",
                    Content = new ScrollViewer
                    {
                        Content = panel,
                        VerticalScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode.Auto,
                        VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto,
                    },
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing reminder details: {ex.Message}");
            }
        }
    }
}
