using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly ReminderService reminderService;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue? dispatcher;

        public ObservableCollection<Reminder> Reminders { get; } = new ObservableCollection<Reminder>();

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial Reminder? SelectedReminder { get; set; }

        [ObservableProperty]
        public partial Reminder? NextReminder { get; set; }

        public RemindersViewModel()
        {
            reminderService = new ReminderService();
            dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            ReminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            var current = UserSession.UserId ?? 0;
            if (current == userId)
            {
                await LoadReminders();
            }
        }

        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            await reminderService.DeleteReminder(reminder.Id);
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => Reminders.Remove(reminder));
            }
            else
            {
                Reminders.Remove(reminder);
            }

            ReminderService.NotifyRemindersChangedForUser(UserSession.UserId ?? 0);
        }

        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }
            await SaveReminderAsync(reminder);
        }

        public async Task<string> SaveReminderAsync(Reminder reminder)
        {
            if (reminder == null)
            {
                return "Error: invalid reminder";
            }

            string result = await reminderService.SaveReminder(reminder);

            if (result == "Success")
            {
                await LoadReminders();
            }

            return result;
        }

        [RelayCommand]
        public async Task LoadReminders()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                int currentId = UserSession.UserId ?? 0;

                if (currentId != 0)
                {
                    var items = (await reminderService.GetUserReminders(currentId)).ToList();
                    var next = await reminderService.GetNextReminder(currentId);

                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            Reminders.Clear();
                            foreach (var item in items)
                            {
                                Reminders.Add(item);
                            }

                            NextReminder = next;
                        });
                    }
                    else
                    {
                        Reminders.Clear();
                        foreach (var item in items)
                        {
                            Reminders.Add(item);
                        }
                        NextReminder = next;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reminders Load Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void PrepareNewReminder()
        {
            var newReminder = new Reminder { UserId = UserSession.UserId ?? 0 };
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => SelectedReminder = newReminder);
            }
            else
            {
                SelectedReminder = newReminder;
            }
        }

        [RelayCommand]
        public void EditReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => SelectedReminder = reminder);
            }
            else
            {
                SelectedReminder = reminder;
            }
        }
    }
}
