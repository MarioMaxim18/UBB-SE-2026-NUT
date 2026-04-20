using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class DailyLogService
    {
        private readonly DailyLogRepository repository;
        private readonly UserRepository userRepository;
        private readonly MealService mealService;

        public DailyLogService()
        {
            repository = new DailyLogRepository();
            userRepository = new UserRepository();
            mealService = new MealService();
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException("User is not logged in.");
        }

        public async Task<bool> HasAnyLogsAsync()
        {
            return await repository.HasAnyLogs(GetUserId());
        }

        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(1);
            return await repository.GetNutritionTotalsForRange(userId, start, end);
        }

        public async Task<DailyLog> GetCurrentWeekTotalsAsync()
        {
            var userId = GetUserId();
            var today = DateTime.Today;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(7);

            return await repository.GetNutritionTotalsForRange(userId, startOfWeek, endOfWeek);
        }

        public async Task<UserData> GetCurrentUserNutritionTargetsAsync()
        {
            return await userRepository.GetUserDataByUserId(GetUserId());
        }

        public Task<double> GetTodayBurnedCaloriesAsync()
        {
            return Task.FromResult(500d);
        }

        public async Task<List<Meal>> SearchMealsAsync(string? searchTerm)
        {
            var filter = new MealFilter
            {
                SearchTerm = searchTerm ?? string.Empty,
            };

            return await mealService.GetFilteredMealsAsync(filter);
        }

        public async Task<List<Meal>> GetMealsForAutocompleteAsync()
        {
            return await mealService.GetFilteredMealsAsync(new MealFilter());
        }

        public async Task LogMealAsync(Meal meal)
        {
            if (meal == null)
            {
                throw new ArgumentNullException(nameof(meal));
            }

            await repository.Add(new DailyLog
            {
                UserId = GetUserId(),
                MealId = meal.Id,
                Calories = meal.Calories,
                LoggedAt = DateTime.Now,
            });
        }
    }
}
