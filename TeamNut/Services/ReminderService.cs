namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository reminderRepository;

        public event EventHandler<int>? RemindersChanged;

        private const int MaxReminderNameLength = 50;

        private const int InvalidUserId = 0;

        private const string ResultSuccess = "Success";

        private const string ErrorInvalidName = "Error: Name must be between 1 and 50 characters.";

        private const string ConfirmConsumptionLogFormat = "User {0} confirmed meal {1}. Updating logs...";

        public ReminderService(IReminderRepository reminderRepository)
        {
            this.reminderRepository = reminderRepository;
        }

        public async Task<Reminder?> GetNextReminder(int userId)
        {
            return await reminderRepository.GetNextReminder(userId);
        }

        public async Task<Reminder?> GetReminderById(int id)
        {
            return await reminderRepository.GetById(id);
        }

        public async Task<string> SaveReminder(Reminder reminder)
        {
            if ((reminder.UserId == InvalidUserId ||
                 reminder.UserId == default) &&
                UserSession.UserId != null)
            {
                reminder.UserId =
                    UserSession.UserId ?? reminder.UserId;
            }

            if (string.IsNullOrWhiteSpace(reminder.Name) ||
                reminder.Name.Length > MaxReminderNameLength)
            {
                return ErrorInvalidName;
            }

            if (reminder.Id == InvalidUserId)
            {
                await reminderRepository.Add(reminder);
            }
            else
            {
                await reminderRepository.Update(reminder);
            }

            try
            {
                RemindersChanged?.Invoke(this, reminder.UserId);
            }
            catch
            {
            }

            return ResultSuccess;
        }

        public async Task ConfirmConsumption(int userId, int mealId)
        {
            Console.WriteLine(
                string.Format(
                    ConfirmConsumptionLogFormat,
                    userId,
                    mealId));
        }

        public async Task<IEnumerable<Reminder>> GetUserReminders(int userId)
        {
            return await reminderRepository.GetAllByUserId(userId);
        }

        public async Task DeleteReminder(int id)
        {
            var existing =
                await reminderRepository.GetById(id);

            await reminderRepository.Delete(id);

            if (existing != null)
            {
                try
                {
                    RemindersChanged?.Invoke(this, existing.UserId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RemindersChanged handler error: {ex.Message}");
                }
            }
        }

        public void NotifyRemindersChangedForUser(int userId)
        {
            try
            {
                RemindersChanged?.Invoke(null, userId);
            }
            catch
            {
            }
        }
    }
}
