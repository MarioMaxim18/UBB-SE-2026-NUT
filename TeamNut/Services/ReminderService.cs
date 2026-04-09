using Microsoft.WindowsAppSDK.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<Reminder>> GetDueRemindersAsync(int userId, DateTime today, TimeSpan currentTime, TimeSpan triggerWindow)
        {
            var reminders = await GetUserReminders(userId);
            var todayString = today.ToString("yyyy-MM-dd");

            return reminders.Where(reminder =>
                reminder != null &&
                reminder.ReminderDate == todayString &&
                (reminder.Time - currentTime).Duration() <= triggerWindow);
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
            
            if ((reminder.UserId == 0 || reminder.UserId == default) && TeamNut.Models.UserSession.UserId != null)
            {
                reminder.UserId = TeamNut.Models.UserSession.UserId ?? reminder.UserId;
            }

            var errors = reminder.GetValidationErrors();
            if (errors.Any())
            {
                return string.Join(Environment.NewLine, errors);
            }

            if (reminder.Id == 0)
                await _reminderRepository.Add(reminder);
            else
                await _reminderRepository.Update(reminder);
                try
                {
                    RemindersChanged?.Invoke(this, reminder.UserId);
                }
                catch { }

            return "Success";

            
          
        }

        
        public async Task ConfirmConsumption(int userId, int mealId)
        {

            Console.WriteLine($"User {userId} confirmed meal {mealId}. Updating logs...");
        }

        public async Task<bool> ConfirmReminderConsumptionAsync(Reminder reminder, int userId)
        {
            if (reminder == null)
            {
                return false;
            }

            var mealService = new MealService();
            var meals = await mealService.GetMealsAsync();
            var matchedMeal = meals.FirstOrDefault(meal => string.Equals(meal.Name?.Trim(), reminder.Name?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (matchedMeal != null)
            {
                var mealPlanService = new MealPlanService();
                await mealPlanService.SaveMealToDailyLogForUserAsync(userId, matchedMeal.Id, matchedMeal.Calories);

                var inventoryService = new InventoryService();
                await inventoryService.ConsumeMeal(userId, matchedMeal.Id);
            }

            await DeleteReminder(reminder.Id);
            NotifyRemindersChangedForUser(userId);
            return true;
        }

        public async Task<IEnumerable<Reminder>> GetUserReminders(int userId)
        {
            
            return await _reminderRepository.GetAllByUserId(userId);
        }
        

        public async Task DeleteReminder(int id)
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
            catch { }
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
