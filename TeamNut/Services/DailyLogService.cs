using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class DailyLogService
    {
        private readonly DailyLogRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly MealService _mealService;

        public DailyLogService()
        {
            _repository = new DailyLogRepository();
            _userRepository = new UserRepository();
            _mealService = new MealService();
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException("User is not logged in.");
        }

        public async Task<bool> HasAnyLogsAsync()
        {
            return await _repository.HasAnyLogs(GetUserId());
        }

        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(1);
            return await _repository.GetNutritionTotalsForRange(userId, start, end);
        }

        public async Task<DailyLog> GetCurrentWeekTotalsAsync()
        {
            var userId = GetUserId();
            var today = DateTime.Today;

            int differenceBetweenDays = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-differenceBetweenDays);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _repository.GetNutritionTotalsForRange(userId, startOfWeek, endOfWeek);
        }

        public async Task<UserData> GetCurrentUserNutritionTargetsAsync()
        {
            return await _userRepository.GetUserDataByUserId(GetUserId());
        }

        public Task<double> GetTodayBurnedCaloriesAsync()
        {
            return Task.FromResult(500d);
        }

        public async Task<List<Meal>> SearchMealsAsync(string? searchTerm)
        {
            var filter = new MealFilter
            {
                SearchTerm = searchTerm ?? string.Empty
            };

            return await _mealService.GetFilteredMealsAsync(filter);
        }

        public async Task<List<Meal>> GetMealsForAutocompleteAsync()
        {
            return await _mealService.GetFilteredMealsAsync(new MealFilter());
        }

        public async Task LogMealAsync(Meal meal)
        {
            if (meal == null)
            {
                throw new ArgumentNullException(nameof(meal));
            }

            await _repository.Add(new DailyLog
            {
                UserId = GetUserId(),
                MealId = meal.Id,
                Calories = meal.Calories,
                LoggedAt = DateTime.Now
            });
        }
    }
}
