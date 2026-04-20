using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace TeamNut.Views.RemindersView
{
    public sealed partial class RemindersPage : Page
    {
        public TeamNut.ViewModels.RemindersViewModel ViewModel { get; } = new();
        private const int DialogStackSpacing = 8;
        private const int FrequencyComboWidth = 200;
        private static readonly Microsoft.UI.Xaml.Thickness FrequencyComboMargin = new(0, 4, 0, 0);
        private const int MaxReminderNameLength = 50;
        private const string DateFormatIso = "yyyy-MM-dd";
        private const string ButtonSave = "Save";
        private const string ButtonCancel = "Cancel";
        private const string ButtonYes = "Yes";
        private const string ButtonOk = "OK";
        private const string TitleAddReminder = "Add Reminder";
        private const string TitleEditReminder = "Edit Reminder";
        private const string TitleDeleteReminder = "Delete Reminder";
        private const string TitleSaveFailed = "Save Failed";
        private const string LabelName = "Name";
        private const string LabelDate = "Date";
        private const string LabelTime = "Time";
        private const string LabelSound = "Sound";
        private const string LabelFrequency = "Frequency";
        private const string FrequencyOnce = "Once";
        private const string FrequencyDaily = "Daily";
        private const string FrequencyWeekly = "Weekly";
        private const string FrequencyMonthly = "Monthly";
        private const string MsgConfirmDelete = "Are you sure you want to delete this reminder?";
        private const string SaveSuccessText = "Success";

        public RemindersPage()
        {
            InitializeComponent();
            DataContext = ViewModel;

            Loaded += async (_, _) => await ViewModel.LoadReminders();

            ViewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName != nameof(ViewModel.SelectedReminder))
                    return;

                var reminder = ViewModel.SelectedReminder;
                if (reminder == null) return;

                try
                {
                    var panel = new StackPanel
                    {
                        Spacing = DialogStackSpacing
                    };

                    var nameBox = new TextBox
                    {
                        Text = reminder.Name ?? string.Empty
                    };

                    var datePicker = new DatePicker();
                    if (!string.IsNullOrWhiteSpace(reminder.ReminderDate) &&
                        DateTime.TryParse(reminder.ReminderDate, out var parsed))
                    {
                        datePicker.Date = parsed;
                    }

                    var timePicker = new TimePicker
                    {
                        Time = reminder.Time
                    };

                    var soundToggle = new ToggleSwitch
                    {
                        IsOn = reminder.HasSound
                    };

                    var freqCombo = new ComboBox
                    {
                        Width = FrequencyComboWidth,
                        Margin = FrequencyComboMargin
                    };
                    freqCombo.Items.Add(FrequencyOnce);
                    freqCombo.Items.Add(FrequencyDaily);
                    freqCombo.Items.Add(FrequencyWeekly);
                    freqCombo.Items.Add(FrequencyMonthly);

                    freqCombo.SelectedIndex =
                        string.IsNullOrWhiteSpace(reminder.Frequency)
                            ? 0
                            : Math.Max(
                                freqCombo.Items.IndexOf(reminder.Frequency),
                                0);

                    panel.Children.Add(new TextBlock { Text = LabelName });
                    panel.Children.Add(nameBox);
                    panel.Children.Add(new TextBlock { Text = LabelDate });
                    panel.Children.Add(datePicker);
                    panel.Children.Add(new TextBlock { Text = LabelTime });
                    panel.Children.Add(timePicker);
                    panel.Children.Add(new TextBlock { Text = LabelSound });
                    panel.Children.Add(soundToggle);
                    panel.Children.Add(new TextBlock { Text = LabelFrequency });
                    panel.Children.Add(freqCombo);

                    bool ValidateInputs()
                    {
                        var name = nameBox.Text ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(name)) return false;
                        if (name.Length > MaxReminderNameLength) return false;
                        if (freqCombo.SelectedItem == null) return false;
                        return true;
                    }

                    var dialog = new ContentDialog
                    {
                        Title = reminder.Id == 0
                            ? TitleAddReminder
                            : TitleEditReminder,
                        Content = new ScrollViewer
                        {
                            Content = panel,
                            VerticalScrollMode = ScrollMode.Auto,
                            VerticalScrollBarVisibility =
                                ScrollBarVisibility.Auto
                        },
                        PrimaryButtonText = ButtonSave,
                        CloseButtonText = ButtonCancel,
                        XamlRoot = XamlRoot
                    };

                    dialog.IsPrimaryButtonEnabled = ValidateInputs();
                    nameBox.TextChanged += (_, _) =>
                        dialog.IsPrimaryButtonEnabled = ValidateInputs();
                    freqCombo.SelectionChanged += (_, _) =>
                        dialog.IsPrimaryButtonEnabled = ValidateInputs();

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        reminder.Name = nameBox.Text;
                        reminder.ReminderDate =
                            datePicker.Date.ToString(DateFormatIso);
                        reminder.Time = timePicker.Time;
                        reminder.HasSound = soundToggle.IsOn;
                        reminder.Frequency =
                            freqCombo.SelectedItem?.ToString()
                            ?? FrequencyOnce;

                        var saveResult =
                            await ViewModel.SaveReminderAsync(reminder);

                        if (saveResult != SaveSuccessText)
                        {
                            await new ContentDialog
                            {
                                Title = TitleSaveFailed,
                                Content = saveResult,
                                CloseButtonText = ButtonOk,
                                XamlRoot = XamlRoot
                            }.ShowAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Reminder dialog error: {ex.Message}");
                }
                finally
                {
                    ViewModel.SelectedReminder = null;
                }
            };
        }

        private async void DeleteButton_Click(
            object sender,
            Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is TeamNut.Models.Reminder reminder)
            {
                var confirm = new ContentDialog
                {
                    Title = TitleDeleteReminder,
                    Content = MsgConfirmDelete,
                    PrimaryButtonText = ButtonYes,
                    CloseButtonText = ButtonCancel,
                    XamlRoot = XamlRoot
                };

                if (await confirm.ShowAsync() ==
                    ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteReminder(reminder);
                    ViewModel?.Reminders.Remove(reminder);
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadReminders();
        }
    }
}