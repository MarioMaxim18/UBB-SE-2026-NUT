using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class DailyMealLogService
    {
        private readonly DailyMealLogRepository _repository;
        private readonly DatabaseInitializer _dbInitializer;
        private static bool _tableChecked = false;

        public DailyMealLogService()
        {
            _repository = new DailyMealLogRepository();
            _dbInitializer = new DatabaseInitializer();
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException("User is not logged in.");
        }

        public async Task SaveMealToLog(int mealId, string mealName, int calories, int protein, int carbs, int fat, string mealType)
        {
            // Ensure table exists on first use
            if (!_tableChecked)
            {
                await _dbInitializer.EnsureDailyMealLogsTableExists();
                _tableChecked = true;
            }

            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var log = new DailyMealLog
            {
                UserId = userId,
                MealId = mealId,
                MealName = mealName,
                LogDate = today,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                MealType = mealType
            };

            await _repository.Add(log);
        }

        public async Task<IEnumerable<DailyMealLog>> GetTodaysLog()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            return await _repository.GetByUserAndDate(userId, today);
        }

        public async Task<IEnumerable<DailyMealLog>> GetLogsByDateRange(DateTime startDate, DateTime endDate)
        {
            var userId = GetUserId();
            return await _repository.GetByUserAndDateRange(userId, startDate, endDate);
        }

        public async Task DeleteLog(int logId)
        {
            await _repository.Delete(logId);
        }
    }
}
