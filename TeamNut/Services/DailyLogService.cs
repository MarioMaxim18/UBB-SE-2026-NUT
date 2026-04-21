using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    /// <summary>Service for logging meals and retrieving nutrition totals.</summary>
    public class DailyLogService
    {
        private readonly DailyLogRepository repository;
        private readonly UserRepository userRepository;
        private readonly MealService mealService;
        private const int DaysInWeek = 7;
        private const int OneDay = 1;
        private const DayOfWeek StartOfWeek = DayOfWeek.Monday;
        private const double DefaultBurnedCalories = 500d;
        private const string EmptySearchTerm = "";
        private const string ErrorUserNotLoggedIn = "User is not logged in.";

        public DailyLogService()
        {
            repository = new DailyLogRepository();
            userRepository = new UserRepository();
            mealService = new MealService();
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException(ErrorUserNotLoggedIn);
        }

        public async Task<bool> HasAnyLogsAsync()
        {
            return await repository.HasAnyLogs(GetUserId());
        }

        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(OneDay);

            return await repository
                .GetNutritionTotalsForRange(userId, start, end);
        }

        public async Task<DailyLog> GetCurrentWeekTotalsAsync()
        {
            var userId = GetUserId();
            var today = DateTime.Today;

            int diff =
                (DaysInWeek + (today.DayOfWeek - StartOfWeek)) % DaysInWeek;

            var startOfWeek = today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(DaysInWeek);

            return await repository
                .GetNutritionTotalsForRange(
                    userId,
                    startOfWeek,
                    endOfWeek);
        }

        public async Task<UserData?> GetCurrentUserNutritionTargetsAsync()
        {
            return await userRepository
                .GetUserDataByUserId(GetUserId());
        }

        public Task<double> GetTodayBurnedCaloriesAsync()
        {
            return Task.FromResult(DefaultBurnedCalories);
        }

        public async Task<List<Meal>> SearchMealsAsync(string? searchTerm)
        {
            var filter = new MealFilter
            {
                SearchTerm = searchTerm ?? EmptySearchTerm,
            };

            return await mealService
                .GetFilteredMealsAsync(filter);
        }

        public async Task<List<Meal>> GetMealsForAutocompleteAsync()
        {
            return await mealService
                .GetFilteredMealsAsync(new MealFilter());
        }

        public async Task LogMealAsync(Meal meal)
        {
            if (meal == null)
            {
                throw new ArgumentNullException(nameof(meal));
            }

            await repository.Add(
                new DailyLog
                {
                    UserId = GetUserId(),
                    MealId = meal.Id,
                    Calories = meal.Calories,
                    LoggedAt = DateTime.Now,
                });
        }
    }
}
