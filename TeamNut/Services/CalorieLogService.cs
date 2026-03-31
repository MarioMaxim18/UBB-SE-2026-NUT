using System;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class CalorieLogService
    {
        private readonly CalorieLogRepository _repository;

        public CalorieLogService()
        {
            _repository = new CalorieLogRepository();
        }

        public async Task<CalorieLog> GetDailyLog(int userId)
        {
            var today = DateTime.Now.Date;

            var logs = await _repository.GetByUserAndDateRange(userId, today, today);

            if (logs == null || logs.Count == 0)
                return new CalorieLog
                {
                    Date = today
                };

            return new CalorieLog
            {
                Date = today,
                CaloriesConsumed = logs.Sum(x => x.CaloriesConsumed),
                Protein = logs.Sum(x => x.Protein),
                Carbs = logs.Sum(x => x.Carbs),
                Fats = logs.Sum(x => x.Fats)
            };
        }

        public async Task SaveMealLog(Meal meal, int userId)
        {
            var today = DateTime.Now.Date;

            var existing = await _repository.GetByUserAndDate(userId, today);

            if (existing != null)
            {
                existing.CaloriesConsumed += meal.Calories;
                existing.Protein += meal.Protein;
                existing.Carbs += meal.Carbs;
                existing.Fats += meal.Fat;

                await _repository.Update(existing);
            }
            else
            {
                var log = new CalorieLog
                {
                    UserId = userId,
                    Date = today,
                    CaloriesConsumed = meal.Calories,
                    Protein = meal.Protein,
                    Carbs = meal.Carbs,
                    Fats = meal.Fat
                };

                await _repository.Add(log);
            }
        }

        public async Task<CalorieLog> GetWeeklyTotals(int userId, DateTime date)
        {
            DateTime startOfWeek = GetStartOfWeek(date);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var logs = await _repository.GetByUserAndDateRange(userId, startOfWeek, endOfWeek);

            if (logs == null || !logs.Any())
                return new CalorieLog();

            return new CalorieLog
            {
                CaloriesConsumed = logs.Sum(x => x.CaloriesConsumed),
                Protein = logs.Sum(x => x.Protein),
                Carbs = logs.Sum(x => x.Carbs),
                Fats = logs.Sum(x => x.Fats)
            };
        }

        public bool HasDayPassed(DateTime mealPlanDate)
        {
            return (DateTime.Now.Date - mealPlanDate.Date).TotalDays >= 1;
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - DayOfWeek.Monday;

            if (diff < 0)
                diff += 7;

            return date.AddDays(-diff).Date;
        }
    }
}