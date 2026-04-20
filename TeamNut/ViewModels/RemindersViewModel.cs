using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using System;

namespace TeamNut.ViewModels
{
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly ReminderService _reminderService;
        private readonly DispatcherQueue? _dispatcher;
        private const int InvalidUserId = 0;
        private const string SaveSuccessResult = "Success";
        private const string InvalidReminderResult = "Error: invalid reminder";
        private const string LoadErrorLogFormat = "Reminders Load Error: {0}";
        public ObservableCollection<Reminder> Reminders { get; } = new();
        [ObservableProperty]
        private bool isBusy;
        [ObservableProperty]
        private Reminder? selectedReminder;
        [ObservableProperty]
        private Reminder? nextReminder;
        public RemindersViewModel()
        {
            _reminderService = new ReminderService();
            _dispatcher = DispatcherQueue.GetForCurrentThread();

            ReminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            int currentUserId = UserSession.UserId ?? InvalidUserId;

            if (currentUserId == userId)
                await LoadReminders();
        }

        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null)
                return;

            await _reminderService.DeleteReminder(reminder.Id);

            EnqueueUI(() => Reminders.Remove(reminder));

            ReminderService.NotifyRemindersChangedForUser(
                UserSession.UserId ?? InvalidUserId);
        }

        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            if (reminder == null)
                return;

            await SaveReminderAsync(reminder);
        }

        public async Task<string> SaveReminderAsync(Reminder reminder)
        {
            if (reminder == null)
                return InvalidReminderResult;

            string result = await _reminderService.SaveReminder(reminder);

            if (result == SaveSuccessResult)
                await LoadReminders();

            return result;
        }

        [RelayCommand]
        public async Task LoadReminders()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                int userId = UserSession.UserId ?? InvalidUserId;
                if (userId == InvalidUserId)
                    return;

                var reminders = (await _reminderService.GetUserReminders(userId)).ToList();
                var next = await _reminderService.GetNextReminder(userId);

                EnqueueUI(() =>
                {
                    Reminders.Clear();
                    foreach (var reminder in reminders)
                        Reminders.Add(reminder);

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
                return;

            EnqueueUI(() => SelectedReminder = reminder);
        }

        private void EnqueueUI(Action action)
        {
            if (_dispatcher != null)
                _dispatcher.TryEnqueue(() => action());
            else
                action();
        }
    }
}