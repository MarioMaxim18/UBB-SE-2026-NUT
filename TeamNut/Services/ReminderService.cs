using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Domain;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class ReminderService
    {
        private readonly ReminderRepository _reminderRepository;

        public ReminderService()
        {
            _reminderRepository = new ReminderRepository();
            
        }

        
        public async Task<string> SaveReminder(Reminder reminder)
        {
            
            if (string.IsNullOrWhiteSpace(reminder.Name) || reminder.Name.Length > 50)
            {
                return "Error: Name must be between 1 and 50 characters.";
            }

            if (reminder.Id == 0)
                await _reminderRepository.Add(reminder);
            else
                await _reminderRepository.Update(reminder);

            return "Success";
        }

        
        public async Task ConfirmConsumption(int userId, int mealId)
        {
            // 1. Get the meal details to get calories/macros
            // 2. Add to DailyLogs table (Raw SQL via a LogRepository)
            // 3. Subtract ingredients from Inventory table

            // This needs to be completed with future repositories and models, but here's a placeholder for the logic flow:
            Console.WriteLine($"User {userId} confirmed meal {mealId}. Updating logs...");
        }

        
        public async Task<IEnumerable<Reminder>> GetUserReminders()
        {
            return await _reminderRepository.GetAll();
        }

        public async Task DeleteReminder(int id)
        {
            await _reminderRepository.Delete(id);
        }
    }
}