using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class MealPlanService
    {
        private readonly MealPlanRepository _mealPlanRepository;
        private readonly UserRepository _userRepository;

        public MealPlanService()
        {
            _mealPlanRepository = new MealPlanRepository();
            _userRepository = new UserRepository();
        }

        public async Task<int> GeneratePersonalizedMealPlanAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new InvalidOperationException("Invalid user ID. Please ensure you are logged in.");
            }

            try
            {
                int mealPlanId = await _mealPlanRepository.GeneratePersonalizedDailyMealPlan(userId);

                try
                {
                    var reminderService = new ReminderService();
                    var todays = DateTime.Today.ToString("yyyy-MM-dd");
                    var existing = (await reminderService.GetUserReminders(userId));
                    bool anyToday = false;
                    foreach (var r in existing)
                    {
                        if (r.ReminderDate == todays) { anyToday = true; break; }
                    }

                    if (!anyToday)
                    {
                        var meals = await GetMealsForMealPlanAsync(mealPlanId);

                        string breakfastName = "Breakfast";
                        string lunchName = "Lunch";
                        string dinnerName = "Dinner";

                        if (meals != null && meals.Count > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(meals[0].Name)) breakfastName = meals[0].Name.Trim();
                        }
                        if (meals != null && meals.Count > 1)
                        {
                            if (!string.IsNullOrWhiteSpace(meals[1].Name)) lunchName = meals[1].Name.Trim();
                        }
                        if (meals != null && meals.Count > 2)
                        {
                            if (!string.IsNullOrWhiteSpace(meals[2].Name)) dinnerName = meals[2].Name.Trim();
                        }

                        var breakfast = new Reminder { UserId = userId, Name = breakfastName, ReminderDate = todays, Time = new TimeSpan(8,0,0), HasSound = false, Frequency = "Once" };
                        var lunch = new Reminder { UserId = userId, Name = lunchName, ReminderDate = todays, Time = new TimeSpan(13,0,0), HasSound = false, Frequency = "Once" };
                        var dinner = new Reminder { UserId = userId, Name = dinnerName, ReminderDate = todays, Time = new TimeSpan(17,0,0), HasSound = false, Frequency = "Once" };

                        await reminderService.SaveReminder(breakfast);
                        await reminderService.SaveReminder(lunch);
                        await reminderService.SaveReminder(dinner);

                        ReminderService.NotifyRemindersChangedForUser(userId);
                    }
                }
                catch { }
                if (mealPlanId <= 0)
                {
                    throw new InvalidOperationException("Failed to generate meal plan. Please try again.");
                }

                return mealPlanId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating meal plan: {ex.Message}", ex);
            }
        }

        public async Task<List<Meal>> GetMealsForMealPlanAsync(int mealPlanId)
        {
            if (mealPlanId <= 0)
            {
                throw new ArgumentException("Invalid meal plan ID.", nameof(mealPlanId));
            }

            try
            {
                var meals = await _mealPlanRepository.GetMealsForMealPlan(mealPlanId);

                if (meals == null || meals.Count == 0)
                {
                    return new List<Meal>();
                }

                return meals;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving meals: {ex.Message}", ex);
            }
        }

        public async Task<MealPlan> GetMealPlanByIdAsync(int mealPlanId)
        {
            if (mealPlanId <= 0)
            {
                return null;
            }

            try
            {
                return await _mealPlanRepository.GetById(mealPlanId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving meal plan: {ex.Message}", ex);
            }
        }

        public async Task<MealPlan> GetTodaysMealPlanAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            try
            {
                var latestPlan = await _mealPlanRepository.GetLatestMealPlan(userId);

                if (latestPlan != null && latestPlan.CreatedAt.Date == DateTime.Today)
                {
                    return latestPlan;
                }

                int newPlanId = await GeneratePersonalizedMealPlanAsync(userId);
                return await _mealPlanRepository.GetById(newPlanId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving today's meal plan: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<MealPlan>> GetAllMealPlansAsync()
        {
            try
            {
                return await _mealPlanRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving meal plans: {ex.Message}", ex);
            }
        }

        public (int totalCalories, int totalProtein, int totalCarbs, int totalFat) CalculateTotalNutrition(List<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
            {
                return (0, 0, 0, 0);
            }

            int totalCalories = 0;
            int totalProtein = 0;
            int totalCarbs = 0;
            int totalFat = 0;

            foreach (var meal in meals)
            {
                totalCalories += meal.Calories;
                totalProtein += meal.Protein;
                totalCarbs += meal.Carbs;
                totalFat += meal.Fat;
            }

            return (totalCalories, totalProtein, totalCarbs, totalFat);
        }

        public bool ValidateMealPlan(List<Meal> meals, int targetCalories, int targetProtein, int targetCarbs, int targetFat, double tolerance = 0.10)
        {
            var (totalCalories, totalProtein, totalCarbs, totalFat) = CalculateTotalNutrition(meals);

            bool caloriesValid = Math.Abs(totalCalories - targetCalories) <= (targetCalories * tolerance);
            bool proteinValid = Math.Abs(totalProtein - targetProtein) <= (targetProtein * tolerance);
            bool carbsValid = Math.Abs(totalCarbs - targetCarbs) <= (targetCarbs * tolerance);
            bool fatValid = Math.Abs(totalFat - targetFat) <= (targetFat * tolerance);

            return caloriesValid && proteinValid && carbsValid && fatValid;
        }

        public string GetCalorieAdjustmentDescription(string goal, int baseTDEE)
        {
            string adjustment = goal?.ToLower() switch
            {
                "bulk" => $"+300 kcal (Bulking phase: {baseTDEE} + 300 = {baseTDEE + 300} kcal)",
                "cut" => $"-300 kcal (Cutting phase: {baseTDEE} - 300 = {baseTDEE - 300} kcal)",
                "maintenance" => $"No adjustment (Maintenance: {baseTDEE} kcal)",
                "well-being" => $"No adjustment (Well-being: {baseTDEE} kcal)",
                _ => $"No adjustment (Base TDEE: {baseTDEE} kcal)"
            };

            return adjustment;
        }

        public string GetGoalEmoji(string goal)
        {
            return goal?.ToLower() switch
            {
                "bulk" => "💪",
                "cut" => "🔥",
                "maintenance" => "⚖️",
                "well-being" => "🧘",
                _ => "🎯"
            };
        }

        public async Task<string> GetUserGoalAsync(int userId)
        {
            try
            {
                var userData = await _userRepository.GetUserDataByUserId(userId);
                return userData?.Goal?.ToLower() ?? "maintenance";
            }
            catch
            {
                return "maintenance";
            }
        }

        public async Task SaveMealsToDailyLogAsync(int mealPlanId)
        {
            if (!UserSession.UserId.HasValue)
            {
                throw new InvalidOperationException("User must be logged in to save daily logs.");
            }

            var meals = await GetMealsForMealPlanAsync(mealPlanId);
            await _mealPlanRepository.SaveMealsToDailyLog(UserSession.UserId.Value, meals);
        }

        public async Task SaveMealToDailyLogAsync(int mealId, int calories)
        {
            if (!UserSession.UserId.HasValue)
            {
                throw new InvalidOperationException("User must be logged in to save daily logs.");
            }

            await _mealPlanRepository.SaveMealToDailyLog(UserSession.UserId.Value, mealId, calories);
        }
    }
}

