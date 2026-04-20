using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IReminderService
    {
        event EventHandler<int>? RemindersChanged;

        void NotifyRemindersChangedForUser(int userId);
        Task ConfirmConsumption(int userId, int mealId);
        Task DeleteReminder(int id);
        Task<Reminder?> GetNextReminder(int userId);
        Task<Reminder?> GetReminderById(int id);
        Task<IEnumerable<Reminder>> GetUserReminders(int userId);
        Task<string> SaveReminder(Reminder reminder);
    }
}