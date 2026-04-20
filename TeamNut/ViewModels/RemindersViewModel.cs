using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    /// <summary>View model for managing health reminders.</summary>
    public partial class RemindersViewModel : ObservableObject
    {
        private readonly ReminderService reminderService;
        private readonly DispatcherQueue? dispatcher;
        private const int InvalidUserId = 0;
        private const string SaveSuccessResult = "Success";
        private const string InvalidReminderResult = "Error: invalid reminder";
        private const string LoadErrorLogFormat = "Reminders Load Error: {0}";

        /// <summary>Gets the collection of reminders for the current user.</summary>
        public ObservableCollection<Reminder> Reminders { get; } = new ObservableCollection<Reminder>();

        /// <summary>Gets or sets a value indicating whether a background operation is running.</summary>
        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        /// <summary>Gets or sets the currently selected reminder for editing.</summary>
        [ObservableProperty]
        public partial Reminder? SelectedReminder { get; set; }

        /// <summary>Gets or sets the next upcoming reminder.</summary>
        [ObservableProperty]
        public partial Reminder? NextReminder { get; set; }

        /// <summary>Initializes a new instance of the <see cref="RemindersViewModel"/> class.</summary>
        public RemindersViewModel()
        {
            reminderService = new ReminderService();
            dispatcher = DispatcherQueue.GetForCurrentThread();

            ReminderService.RemindersChanged += OnRemindersChanged;
        }

        private async void OnRemindersChanged(object? sender, int userId)
        {
            int currentUserId = UserSession.UserId ?? InvalidUserId;

            if (currentUserId == userId)
            {
                await LoadReminders();
            }
        }

        /// <summary>Deletes a reminder by its identifier.</summary>
        /// <param name="reminder">The reminder to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task DeleteReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            await reminderService.DeleteReminder(reminder.Id);

            EnqueueUI(() => Reminders.Remove(reminder));

            ReminderService.NotifyRemindersChangedForUser(
                UserSession.UserId ?? InvalidUserId);
        }

        /// <summary>Saves a reminder (adds or updates).</summary>
        /// <param name="reminder">The reminder to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task SaveReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                return;
            }

            await SaveReminderAsync(reminder);
        }

        /// <summary>Saves a reminder and returns a result string.</summary>
        /// <param name="reminder">The reminder to save.</param>
        /// <returns>"Success" on success, or an error message.</returns>
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

        /// <summary>Loads all reminders for the current user.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>Prepares a blank reminder for creation.</summary>
        [RelayCommand]
        public void PrepareNewReminder()
        {
            var reminder = new Reminder
            {
                UserId = UserSession.UserId ?? InvalidUserId
            };

            EnqueueUI(() => SelectedReminder = reminder);
        }

        /// <summary>Sets the given reminder as the currently selected reminder for editing.</summary>
        /// <param name="reminder">The reminder to edit.</param>
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
    }
}
