namespace TeamNut.Views.RemindersView
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Microsoft.Extensions.DependencyInjection;
    using TeamNut.ViewModels;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// RemindersPage.
    /// </summary>
    public sealed partial class RemindersPage : Page
    {
        private const int DialogStackSpacing = 8;

        private const int FrequencyComboWidth = 200;

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

        private static readonly Thickness FrequencyComboMargin = new Thickness(0, 4, 0, 0);

        public TeamNut.ViewModels.RemindersViewModel ViewModel { get; }

        private readonly IValidationService validationService;

        public RemindersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<RemindersViewModel>();
            validationService = App.Services.GetRequiredService<IValidationService>();
            this.DataContext = ViewModel;

            this.Loaded += async (_, _) => await this.ViewModel.LoadReminders();

            this.ViewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName != nameof(this.ViewModel.SelectedReminder))
                {
                    return;
                }

                var reminder = this.ViewModel.SelectedReminder;
                if (reminder == null)
                {
                    return;
                }

                try
                {
                    var panel = new StackPanel
                    {
                        Spacing = DialogStackSpacing,
                    };

                    var nameBox = new TextBox
                    {
                        Text = reminder.Name ?? string.Empty,
                    };

                    var datePicker = new DatePicker();
                    if (!string.IsNullOrWhiteSpace(reminder.ReminderDate) &&
                        DateTime.TryParse(reminder.ReminderDate, out var parsed))
                    {
                        datePicker.Date = parsed;
                    }

                    var timePicker = new TimePicker
                    {
                        Time = reminder.Time,
                    };

                    var soundToggle = new ToggleSwitch
                    {
                        IsOn = reminder.HasSound,
                    };

                    var freqCombo = new ComboBox
                    {
                        Width = FrequencyComboWidth,
                        Margin = FrequencyComboMargin,
                    };
                    freqCombo.Items.Add(FrequencyOnce);
                    freqCombo.Items.Add(FrequencyDaily);
                    freqCombo.Items.Add(FrequencyWeekly);
                    freqCombo.Items.Add(FrequencyMonthly);

                    int freqIndex = 0;
                    if (!string.IsNullOrWhiteSpace(reminder.Frequency))
                    {
                        for (int fi = 0; fi < freqCombo.Items.Count; fi++)
                        {
                            if (string.Equals(freqCombo.Items[fi]?.ToString(), reminder.Frequency, StringComparison.OrdinalIgnoreCase))
                            {
                                freqIndex = fi;
                                break;
                            }
                        }
                    }

                    freqCombo.SelectedIndex = freqIndex;

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
                        if (!validationService.IsValidReminderName(name, MaxReminderNameLength))
                        {
                            return false;
                        }

                        if (freqCombo.SelectedItem == null)
                        {
                            return false;
                        }

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
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        },
                        PrimaryButtonText = ButtonSave,
                        CloseButtonText = ButtonCancel,
                        XamlRoot = this.XamlRoot,
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
                        reminder.ReminderDate = datePicker.Date.ToString(DateFormatIso);
                        reminder.Time = timePicker.Time;
                        reminder.HasSound = soundToggle.IsOn;
                        reminder.Frequency =
                            freqCombo.SelectedItem?.ToString()
                            ?? FrequencyOnce;

                        var saveResult = await this.ViewModel.SaveReminderAsync(reminder);

                        if (saveResult != SaveSuccessText)
                        {
                            await new ContentDialog
                            {
                                Title = TitleSaveFailed,
                                Content = saveResult,
                                CloseButtonText = ButtonOk,
                                XamlRoot = this.XamlRoot,
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
                    this.ViewModel.SelectedReminder = null;
                }
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await this.ViewModel.LoadReminders();
        }

        private async void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
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
                    XamlRoot = this.XamlRoot,
                };

                if (await confirm.ShowAsync() == ContentDialogResult.Primary)
                {
                    await this.ViewModel.DeleteReminder(reminder);
                    this.ViewModel?.Reminders.Remove(reminder);
                }
            }
        }
    }
}
