using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.Views
{
    public sealed partial class MainPage : Page
    {
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcher;
        private readonly DispatcherTimer _reminderTimer;
        private readonly System.Collections.Generic.HashSet<int> _shownReminders = new();
        private readonly ReminderService _reminderService = new();
        public MainViewModel ViewModel { get; } = new();
        public TeamNut.ViewModels.RemindersViewModel RemindersViewModel { get; } = new();
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

            _dispatcher = Microsoft.UI.Dispatching.DispatcherQueue
                .GetForCurrentThread();

            _ = ViewModel.LoadHeaderData();
            LoadTopReminder();

            ReminderService.RemindersChanged += OnRemindersChanged;

            _reminderTimer = new DispatcherTimer
            {
                Interval = ReminderPollInterval
            };
            _reminderTimer.Tick += ReminderTimer_Tick;
            _reminderTimer.Start();
        }

        private async void ReminderTimer_Tick(object? sender, object? e)
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0) return;

                var reminders =
                    await _reminderService.GetUserReminders(userId);

                var today = DateTime.Today.ToString(DateFormatIso);
                var now = DateTime.Now.TimeOfDay;

                foreach (var rem in reminders)
                {
                    if (rem == null) continue;
                    if (rem.ReminderDate != today) continue;
                    if (_shownReminders.Contains(rem.Id)) continue;

                    var diff = (rem.Time - now).Duration();
                    if (diff <= ReminderTriggerWindow)
                    {
                        _shownReminders.Add(rem.Id);
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
                    return;

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

                    await _reminderService.DeleteReminder(rem.Id);
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
                if (current != userId) return;

                if (_dispatcher != null)
                {
                    _dispatcher.TryEnqueue(LoadTopReminder);
                }
                else
                {
                    LoadTopReminder();
                }
            }
            catch { }
        }

        private async void LoadTopReminder()
        {
            try
            {
                int userId = UserSession.UserId ?? 0;
                if (userId == 0) return;

                var next =
                    await _reminderService.GetNextReminder(userId);

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
                app.window != null)
            {
                app.window.Content =
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
                if (userId == 0) return;

                var reminder =
                    await _reminderService.GetNextReminder(userId);

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
    }
}