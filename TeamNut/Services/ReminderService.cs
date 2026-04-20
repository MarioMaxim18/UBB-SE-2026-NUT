using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class ReminderService
    {
        private readonly ReminderRepository _reminderRepository;

        public static event EventHandler<int>? RemindersChanged;
        private const int MaxReminderNameLength = 50;
        private const int InvalidUserId = 0;
        private const string ResultSuccess = "Success";
        private const string ErrorInvalidName = "Error: Name must be between 1 and 50 characters.";
        private const string ConfirmConsumptionLogFormat = "User {0} confirmed meal {1}. Updating logs...";

        public ReminderService()
        {
            _reminderRepository = new ReminderRepository();
        }

        public async Task<Reminder?> GetNextReminder(int userId)
        {
            return await _reminderRepository.GetNextReminder(userId);
        }

        public async Task<Reminder?> GetReminderById(int id)
        {
            return await _reminderRepository.GetById(id);
        }

        public async Task<string> SaveReminder(Reminder reminder)
        {
            // Ensure UserId is set
            if ((reminder.UserId == InvalidUserId ||
                 reminder.UserId == default) &&
                UserSession.UserId != null)
            {
                reminder.UserId =
                    UserSession.UserId ?? reminder.UserId;
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(reminder.Name) ||
                reminder.Name.Length > MaxReminderNameLength)
            {
                return ErrorInvalidName;
            }

            if (reminder.Id == InvalidUserId)
            {
                await _reminderRepository.Add(reminder);
            }
            else
            {
                await _reminderRepository.Update(reminder);
            }

            try
            {
                RemindersChanged?.Invoke(this, reminder.UserId);
            }
            catch
            {
                // swallow event errors
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
            return await _reminderRepository.GetAllByUserId(userId);
        }

        public async Task DeleteReminder(int id)
        {
            try
            {
                var existing =
                    await _reminderRepository.GetById(id);

                await _reminderRepository.Delete(id);

                if (existing != null)
                {
                    RemindersChanged?.Invoke(this, existing.UserId);
                }
            }
            catch
            {
                // swallow delete errors
            }
        }

        public static void NotifyRemindersChangedForUser(int userId)
        {
            try
            {
                RemindersChanged?.Invoke(null, userId);
            }
            catch
            {
                // swallow notify errors
            }
        }
    }
}