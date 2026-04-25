namespace TeamNut.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.UI.Dispatching;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;

    public partial class RemindersViewModel : ObservableObject, IDisposable
    {
        private bool disposed;
        private readonly IReminderService reminderService;
        private readonly DispatcherQueue? dispatcher;

        private const int InvalidUserId = 0;
        private const string SaveSuccessResult = "Success";
        private const string InvalidReminderResult = "Error: invalid reminder";
        private const string LoadErrorLogFormat = "Reminders Load Error: {0}";

        public ObservableCollection<Reminder> Reminders { get; } = new ObservableCollection<Reminder>();

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial Reminder? SelectedReminder { get; set; }

        [ObservableProperty]
        public partial Reminder? NextReminder { get; set; }

        public RemindersViewModel(IReminderService reminderService)
        {
            this.reminderService = reminderService;
            try
            {
                dispatcher = DispatcherQueue.GetForCurrentThread();
            }
            catch
            {
                dispatcher = null;
            }
            this.reminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            int currentUserId = UserSession.UserId ?? InvalidUserId;

            if (currentUserId == userId)
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

            EnqueueUI(() => Reminders.Remove(reminder));

            reminderService.NotifyRemindersChangedForUser(
                UserSession.UserId ?? InvalidUserId);
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
                return InvalidReminderResult;
            }

            string result = await reminderService.SaveReminder(reminder);

            if (result == SaveSuccessResult)
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

                int userId = UserSession.UserId ?? InvalidUserId;
                if (userId == InvalidUserId)
                {
                    return;
                }

                var reminders = (await reminderService.GetUserReminders(userId)).ToList();
                var next = await reminderService.GetNextReminder(userId);

                EnqueueUI(() =>
                {
                    Reminders.Clear();
                    foreach (var reminder in reminders)
                    {
                        Reminders.Add(reminder);
                    }

                    NextReminder = next;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format(LoadErrorLogFormat, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void PrepareNewReminder()
        {
            var reminder = new Reminder
            {
                UserId = UserSession.UserId ?? InvalidUserId
            };

            EnqueueUI(() => SelectedReminder = reminder);
        }

        [RelayCommand]
        public void EditReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            EnqueueUI(() => SelectedReminder = reminder);
        }

        private void EnqueueUI(Action action)
        {
            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() => action());
            }
            else
            {
                action();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                reminderService.RemindersChanged -= OnRemindersChanged;
                disposed = true;
            }
        }
    }
}