using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcher;
        private readonly DispatcherTimer reminderTimer;
        private readonly System.Collections.Generic.HashSet<int> shownReminders = new System.Collections.Generic.HashSet<int>();
        private readonly ReminderService reminderService = new ReminderService();
        public MainViewModel ViewModel { get; } = new MainViewModel();
        public TeamNut.ViewModels.RemindersViewModel RemindersViewModel { get; } = new TeamNut.ViewModels.RemindersViewModel();
        private static readonly TimeSpan ReminderPollInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ReminderTriggerWindow = TimeSpan.FromSeconds(30);
        private const string DateFormatIso = "yyyy-MM-dd";
        private const string TimeFormatShort = @"hh\:mm";
        private const string DefaultReminderTitle = "Reminder";
        private const string ReminderDialogPrompt = "Did you consume this meal?";
        private const string NoUpcomingMealsText = "No upcoming meals";
        private const string ReminderDetailsTitle = "Reminder Details";
        private const string ButtonConfirm = "Confirm";
        private const string ButtonDecline = "Decline";
        private const string ButtonClose = "Close";
        private const string LabelName = "Name";
        private const string LabelDate = "Date";
        private const string LabelTime = "Time";
        private const string LabelSound = "Sound";
        private const string LabelFrequency = "Frequency";
        private const string SoundOnText = "On";
        private const string SoundOffText = "Off";
        private const int DetailsPanelSpacing = 8;

        public MainPage()
        {
            InitializeComponent();

            dispatcher = Microsoft.UI.Dispatching.DispatcherQueue
                .GetForCurrentThread();

            _ = ViewModel.LoadHeaderData();
            LoadTopReminder();

            ReminderService.RemindersChanged += OnRemindersChanged;

            reminderTimer = new DispatcherTimer
            {
                Interval = ReminderPollInterval
            };
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

                var reminders =
                    await reminderService.GetUserReminders(userId);

                var today = DateTime.Today.ToString(DateFormatIso);
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
                    if (diff <= ReminderTriggerWindow)
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

        private async System.Threading.Tasks.Task ShowReminderDialog(Reminder rem)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = rem.Name ?? DefaultReminderTitle,
                    Content = ReminderDialogPrompt,
                    PrimaryButtonText = ButtonConfirm,
                    CloseButtonText = ButtonDecline,
                    XamlRoot = XamlRoot
                };

                if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                {
                    return;
                }

                try
                {
                    var mealService = new MealService();
                    var meals = await mealService.GetMealsAsync();

                    var matched = meals.Find(m =>
                        string.Equals(
                            m.Name?.Trim(),
                            rem.Name?.Trim(),
                            StringComparison.OrdinalIgnoreCase));

                    int userId = UserSession.UserId ?? 0;

                    if (matched != null)
                    {
                        var repo =
                            new TeamNut.Repositories.MealPlanRepository();

                        await repo.SaveMealToDailyLog(
                            userId,
                            matched.Id,
                            matched.Calories);

                        var inventory =
                            new TeamNut.Services.InventoryService();

                        await inventory.ConsumeMeal(userId, matched.Id);
                    }

                    await reminderService.DeleteReminder(rem.Id);
                    ReminderService
                        .NotifyRemindersChangedForUser(userId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Error confirming reminder: {ex.Message}");
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
                int current = UserSession.UserId ?? 0;
                if (current != userId)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(LoadTopReminder);
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

        private async void LoadTopReminder()
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0)
                {
                    return;
                }

                var next =
                    await reminderService.GetNextReminder(userId);

                var text = next != null
                    ? $"{next.Name} at {next.Time:hh\\:mm}"
                    : NoUpcomingMealsText;

                if (MainNextReminderText != null)
                {
                    MainNextReminderText.Text = text;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error loading top reminder: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Logout();

            if (Application.Current is App app &&
                app.AppWindow != null)
            {
                app.AppWindow.Content =
                    new TeamNut.Views.UserView.UserView();
            }
        }

        private async void MainNextReminderDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0)
                {
                    return;
                }

                var reminder =
                    await reminderService.GetNextReminder(userId);

                if (reminder == null)
                {
                    await new ContentDialog
                    {
                        Title = ReminderDetailsTitle,
                        Content = "No upcoming reminders.",
                        CloseButtonText = ButtonClose,
                        XamlRoot = XamlRoot
                    }.ShowAsync();
                    return;
                }

                var panel = new StackPanel
                {
                    Spacing = DetailsPanelSpacing
                };

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = LabelName,
                        FontWeight =
                            Microsoft.UI.Text.FontWeights.SemiBold
                    });
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = reminder.Name ?? string.Empty,
                        TextWrapping = TextWrapping.Wrap
                    });

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = LabelDate,
                        FontWeight =
                            Microsoft.UI.Text.FontWeights.SemiBold
                    });
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = reminder.ReminderDate ?? string.Empty
                    });

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = LabelTime,
                        FontWeight =
                            Microsoft.UI.Text.FontWeights.SemiBold
                    });
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = reminder.Time.ToString(TimeFormatShort)
                    });

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = LabelSound,
                        FontWeight =
                            Microsoft.UI.Text.FontWeights.SemiBold
                    });
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = reminder.HasSound
                            ? SoundOnText
                            : SoundOffText
                    });

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = LabelFrequency,
                        FontWeight =
                            Microsoft.UI.Text.FontWeights.SemiBold
                    });
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = reminder.Frequency ?? string.Empty
                    });

                var dialog = new ContentDialog
                {
                    Title = ReminderDetailsTitle,
                    Content = new ScrollViewer
                    {
                        Content = panel,
                        VerticalScrollMode =
                            ScrollMode.Auto,
                        VerticalScrollBarVisibility =
                            ScrollBarVisibility.Auto
                    },
                    CloseButtonText = ButtonClose,
                    XamlRoot = XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error showing reminder details: {ex.Message}");
            }
        }

        private void MainTabView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == MealsTab && MealsFrame.Content == null)
            {
                MealsFrame.Navigate(typeof(MealsPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == MealPlanTab && MealPlanFrame.Content == null)
            {
                MealPlanFrame.Navigate(typeof(TeamNut.Views.MealPlanView.MealPlanPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == DailyLogTab && DailyLogFrame.Content == null)
            {
                DailyLogFrame.Navigate(typeof(TeamNut.Views.CalorieLoggingView.CalorieLoggingPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == InventoryTab && InventoryFrame.Content == null)
            {
                InventoryFrame.Navigate(typeof(TeamNut.Views.InventoryView.InventoryPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == ChatTab && ChatFrame.Content == null)
            {
                ChatFrame.Navigate(typeof(TeamNut.Views.NutritionistChat.NutritionistChatPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == ShoppingListTab && ShoppingListFrame.Content == null)
            {
                ShoppingListFrame.Navigate(typeof(TeamNut.Views.ShoppingListView.ShoppingListPage));
            }
            else if ((MainTabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem) == RemindersTab && RemindersFrame.Content == null)
            {
                RemindersFrame.Navigate(typeof(TeamNut.Views.RemindersView.RemindersPage));
            }
        }
    }
}