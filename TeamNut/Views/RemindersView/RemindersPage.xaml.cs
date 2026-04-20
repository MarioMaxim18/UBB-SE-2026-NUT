using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using TeamNut.ViewModels;


namespace TeamNut.Views.RemindersView
{
    public sealed partial class RemindersPage : Page
    {
        public TeamNut.ViewModels.RemindersViewModel ViewModel { get; }

        public RemindersPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetService<RemindersViewModel>();

            this.DataContext = ViewModel;

            this.Loaded += async (s, e) =>
            {
                await ViewModel.LoadReminders();
            };

            ViewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.SelectedReminder))
                {
                    var reminder = ViewModel.SelectedReminder;
                    if (reminder == null) return;

                    try
                    {
                        var panel = new StackPanel { Spacing = 8 };

                        var nameBox = new TextBox { Text = reminder.Name ?? string.Empty };

                        var datePicker = new DatePicker();
                        if (!string.IsNullOrWhiteSpace(reminder.ReminderDate) && DateTime.TryParse(reminder.ReminderDate, out var parsed))
                            datePicker.Date = parsed;

                        var timePicker = new TimePicker { Time = reminder.Time };

                        var soundToggle = new ToggleSwitch { IsOn = reminder.HasSound };

                        var freqCombo = new ComboBox { Width = 200, Margin = new Microsoft.UI.Xaml.Thickness(0,4,0,0) };
                        freqCombo.Items.Add("Once");
                        freqCombo.Items.Add("Daily");
                        freqCombo.Items.Add("Weekly");
                        freqCombo.Items.Add("Monthly");

                        
                        if (string.IsNullOrWhiteSpace(reminder.Frequency))
                        {
                            freqCombo.SelectedIndex = 0;
                        }
                        else
                        {
                            var idx = freqCombo.Items.IndexOf(reminder.Frequency);
                            freqCombo.SelectedIndex = idx >= 0 ? idx : 0;
                        }

                        panel.Children.Add(new TextBlock { Text = "Name" });
                        panel.Children.Add(nameBox);
                        panel.Children.Add(new TextBlock { Text = "Date" });
                        panel.Children.Add(datePicker);
                        panel.Children.Add(new TextBlock { Text = "Time" });
                        panel.Children.Add(timePicker);
                        panel.Children.Add(new TextBlock { Text = "Sound" });
                        panel.Children.Add(soundToggle);
                        panel.Children.Add(new TextBlock { Text = "Frequency" });
                        panel.Children.Add(freqCombo);

                        bool ValidateInputs()
                        {
                            var name = nameBox.Text ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(name)) return false;
                            if (name.Length > 50) return false;
                            if (freqCombo.SelectedItem == null) return false;
                            return true;
                        }

                        var dialog = new ContentDialog()
                        {
                            Title = reminder.Id == 0 ? "Add Reminder" : "Edit Reminder",
                            Content = new ScrollViewer
                            {
                                Content = panel,
                                VerticalScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode.Auto,
                                VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto
                            },
                            PrimaryButtonText = "Save",
                            CloseButtonText = "Cancel",
                            XamlRoot = this.XamlRoot
                        };
                        
                        dialog.IsPrimaryButtonEnabled = ValidateInputs();
                        nameBox.TextChanged += (ss, ee) => dialog.IsPrimaryButtonEnabled = ValidateInputs();
                        freqCombo.SelectionChanged += (ss, ee) => dialog.IsPrimaryButtonEnabled = ValidateInputs();

                        var result = await dialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            reminder.Name = nameBox.Text;
                            reminder.ReminderDate = datePicker.Date.ToString("yyyy-MM-dd");
                            reminder.Time = timePicker.Time;
                            reminder.HasSound = soundToggle.IsOn;
                            reminder.Frequency = freqCombo.SelectedItem?.ToString() ?? "Once";

                            var saveResult = await ViewModel.SaveReminderAsync(reminder);
                            if (saveResult != "Success")
                            {
                                var errDialog = new ContentDialog
                                {
                                    Title = "Save Failed",
                                    Content = saveResult,
                                    CloseButtonText = "OK",
                                    XamlRoot = this.XamlRoot
                                };
                                await errDialog.ShowAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Reminder dialog error: {ex.Message}");
                    }
                    finally
                    {
                        ViewModel.SelectedReminder = null;
                    }
                }
            };
        }

        private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Button btn && btn.DataContext is TeamNut.Models.Reminder reminder)
            {
                var confirm = new ContentDialog
                {
                    Title = "Delete Reminder",
                    Content = "Are you sure you want to delete this reminder?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await confirm.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteReminder(reminder);
                    if (ViewModel != null)
                    {
                        ViewModel.Reminders.Remove(reminder);
                    }
                }
            }
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
