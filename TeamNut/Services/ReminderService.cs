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

        public ReminderService()
        {
            _reminderRepository = new ReminderRepository();
        }

        public async Task<Reminder?> GetNextReminderAsync(int userId)
        {
            return await _reminderRepository.GetNextReminder(userId);
        }

        public async Task<Reminder?> GetReminderByIdAsync(int id)
        {
            return await _reminderRepository.GetById(id);
        }

        public async Task<string> SaveReminderAsync(Reminder reminder)
        {
            if ((reminder.UserId == 0 || reminder.UserId == default) && UserSession.UserId != null)
            {
                reminder.UserId = UserSession.UserId ?? reminder.UserId;
            }

            if (string.IsNullOrWhiteSpace(reminder.Name) || reminder.Name.Length > 50)
            {
                return "Error: Name must be between 1 and 50 characters.";
            }

            // Fixed missing braces bug here
            if (reminder.Id == 0)
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
            catch { /* Ignore UI event failures */ }

            return "Success";
        }

        public Task ConfirmConsumptionAsync(int userId, int mealId)
        {
            Console.WriteLine($"User {userId} confirmed meal {mealId}. Updating logs...");
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Reminder>> GetRemindersByUserAsync(int userId)
        {
            return await _reminderRepository.GetAllByUserId(userId);
        }

        public async Task DeleteReminderAsync(int id)
        {
            try
            {
                var existing = await _reminderRepository.GetById(id);
                await _reminderRepository.Delete(id);

                if (existing != null)
                {
                    RemindersChanged?.Invoke(this, existing.UserId);
                }
            }
            catch { /* Prevent deletion failures from crashing app */ }
        }

        public static void NotifyRemindersChangedForUser(int userId)
        {
            try
            {
                RemindersChanged?.Invoke(null, userId);
            }
            catch { }
        }
    }
}