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

        /// <summary>Initializes a new instance of the <see cref="DailyLogService"/> class.</summary>
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

        /// <summary>Returns whether the current user has any log entries.</summary>
        /// <returns><c>true</c> if at least one log entry exists.</returns>
        public async Task<bool> HasAnyLogsAsync()
        {
            return await repository.HasAnyLogs(GetUserId());
        }

        /// <summary>Gets today's nutrition totals for the current user.</summary>
        /// <returns>A <see cref="DailyLog"/> with totals for today.</returns>
        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(OneDay);

            return await repository
                .GetNutritionTotalsForRange(userId, start, end);
        }

        /// <summary>Gets this week's nutrition totals for the current user.</summary>
        /// <returns>A <see cref="DailyLog"/> with totals for the current week.</returns>
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

        /// <summary>Gets the current user's nutrition targets from their health profile.</summary>
        /// <returns>The <see cref="UserData"/> for the current user, or <c>null</c>.</returns>
        public async Task<UserData?> GetCurrentUserNutritionTargetsAsync()
        {
            return await userRepository
                .GetUserDataByUserId(GetUserId());
        }

        /// <summary>Gets the estimated burned calories for today (constant placeholder).</summary>
        /// <returns>The estimated burned calories.</returns>
        public Task<double> GetTodayBurnedCaloriesAsync()
        {
            return Task.FromResult(DefaultBurnedCalories);
        }

        /// <summary>Searches for meals matching the given search term.</summary>
        /// <param name="searchTerm">The search term, or <c>null</c> for all meals.</param>
        /// <returns>A list of matching meals.</returns>
        public async Task<List<Meal>> SearchMealsAsync(string? searchTerm)
        {
            var filter = new MealFilter
            {
                SearchTerm = searchTerm ?? EmptySearchTerm,
            };

            return await mealService
                .GetFilteredMealsAsync(filter);
        }

        /// <summary>Gets all meals suitable for autocomplete suggestions.</summary>
        /// <returns>A list of all available meals.</returns>
        public async Task<List<Meal>> GetMealsForAutocompleteAsync()
        {
            return await mealService
                .GetFilteredMealsAsync(new MealFilter());
        }

        /// <summary>Logs a meal for the current user.</summary>
        /// <param name="meal">The meal to log.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
