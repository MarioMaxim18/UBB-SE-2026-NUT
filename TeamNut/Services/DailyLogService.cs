namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class DailyLogService : IDailyLogService
    {
        private readonly IDailyLogRepository repository;

        private readonly IUserRepository userRepository;

        private readonly IMealService mealService;

        private const int DaysInWeek = 7;

        private const int OneDay = 1;

        private const DayOfWeek StartOfWeek = DayOfWeek.Monday;

        private const double DefaultBurnedCalories = 500d;

        private const string EmptySearchTerm = "";

        private const string ErrorUserNotLoggedIn = "User is not logged in.";

        public DailyLogService(IDailyLogRepository dailyLogRepository, IUserRepository userRepository, IMealService mealService)
        {
            this.repository = dailyLogRepository;
            this.userRepository = userRepository;
            this.mealService = mealService;
        }

        private int GetUserId()
        {
            return UserSession.UserId
                ?? throw new InvalidOperationException(ErrorUserNotLoggedIn);
        }

        public async Task<bool> HasAnyLogsAsync()
        {
            return await this.repository.HasAnyLogs(this.GetUserId());
        }

        public async Task<DailyLog> GetTodayTotalsAsync()
        {
            var userId = this.GetUserId();
            var start = DateTime.Today;
            var end = start.AddDays(OneDay);

            return await this.repository
                .GetNutritionTotalsForRange(userId, start, end);
        }

        public async Task<DailyLog> GetCurrentWeekTotalsAsync()
        {
            var userId = this.GetUserId();
            var today = DateTime.Today;

            int diff =
                (DaysInWeek + (today.DayOfWeek - StartOfWeek)) % DaysInWeek;

            var startOfWeek = today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(DaysInWeek);

            return await this.repository
                .GetNutritionTotalsForRange(
                    userId,
                    startOfWeek,
                    endOfWeek);
        }

        public async Task<UserData?> GetCurrentUserNutritionTargetsAsync()
        {
            return await this.userRepository
                .GetUserDataByUserId(this.GetUserId());
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

            return await this.mealService
                .GetFilteredMealsAsync(filter);
        }

        public async Task<List<Meal>> GetMealsForAutocompleteAsync()
        {
            return await this.mealService
                .GetFilteredMealsAsync(new MealFilter());
        }

        public async Task LogMealAsync(Meal meal)
        {
            if (meal == null)
            {
                throw new ArgumentNullException(nameof(meal));
            }

            await this.repository.Add(
                new DailyLog
                {
                    UserId = this.GetUserId(),
                    MealId = meal.Id,
                    Calories = meal.Calories,
                    LoggedAt = DateTime.Now,
                });
        }
    }
}
