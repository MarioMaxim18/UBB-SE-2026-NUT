using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using Windows.System;
using System;

namespace TeamNut.ViewModels
{
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly ReminderService _reminderService;
        private readonly DispatcherQueue? _dispatcher;

        public ObservableCollection<Reminder> Reminders { get; } = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private Reminder? _selectedReminder;

        [ObservableProperty]
        private Reminder? _nextReminder;

        public RemindersViewModel()
        {
            _reminderService = new ReminderService();
            _dispatcher = DispatcherQueue.GetForCurrentThread();

            ReminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            var current = UserSession.UserId ?? 0;
            if (current == userId)
            {
                await LoadRemindersAsync();
            }
        }

        [RelayCommand]
        public async Task LoadRemindersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                int currentId = UserSession.UserId ?? 0;

                if (currentId != 0)
                {
                    var items = (await _reminderService.GetUserReminders(currentId)).ToList();
                    var next = await _reminderService.GetNextReminder(currentId);

                    if (_dispatcher != null)
                    {
                        _dispatcher.TryEnqueue(() =>
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
                        foreach (var item in items) Reminders.Add(item);
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
        public async Task DeleteReminderAsync(Reminder reminder)
        {
            if (reminder == null) return;

            await _reminderService.DeleteReminder(reminder.Id);

            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => Reminders.Remove(reminder));
            }
            else
            {
                Reminders.Remove(reminder);
            }

            ReminderService.NotifyRemindersChangedForUser(UserSession.UserId ?? 0);
        }

        [RelayCommand]
        public async Task SaveReminderAsync(Reminder reminder)
        {
            if (reminder == null) return;

            string result = await _reminderService.SaveReminder(reminder);

            if (result == "Success")
            {
                await LoadRemindersAsync();
            }
        }

        [RelayCommand]
        public void PrepareNewReminder()
        {
            var newReminder = new Reminder { UserId = UserSession.UserId ?? 0 };

            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => SelectedReminder = newReminder);
            }
            else
            {
                SelectedReminder = newReminder;
            }
        }

        [RelayCommand]
        public void EditReminder(Reminder reminder)
        {
            if (reminder == null) return;

            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => SelectedReminder = reminder);
            }
            else
            {
                SelectedReminder = reminder;
            }
        }
    }
}