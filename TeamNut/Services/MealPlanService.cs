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

        /// <summary>
        /// Generates a personalized daily meal plan for the user based on their nutritional needs
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The ID of the generated meal plan</returns>
        /// <exception cref="InvalidOperationException">Thrown when user ID is invalid</exception>
        public async Task<int> GeneratePersonalizedMealPlanAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new InvalidOperationException("Invalid user ID. Please ensure you are logged in.");
            }

            try
            {
                int mealPlanId = await _mealPlanRepository.GeneratePersonalizedDailyMealPlan(userId);

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

        /// <summary>
        /// Retrieves all meals for a specific meal plan
        /// </summary>
        /// <param name="mealPlanId">The ID of the meal plan</param>
        /// <returns>List of meals with nutritional information</returns>
        /// <exception cref="ArgumentException">Thrown when meal plan ID is invalid</exception>
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

        /// <summary>
        /// Gets a meal plan by ID
        /// </summary>
        /// <param name="mealPlanId">The ID of the meal plan</param>
        /// <returns>The meal plan if found, null otherwise</returns>
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

        /// <summary>
        /// Gets today's meal plan for the specified user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The meal plan if found, null if no plan exists for today</returns>
        public async Task<MealPlan> GetTodaysMealPlanAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            try
            {
                return await _mealPlanRepository.GetTodaysMealPlan(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving today's meal plan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all meal plans
        /// </summary>
        /// <returns>List of all meal plans</returns>
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

        /// <summary>
        /// Calculates total nutritional values for a list of meals
        /// </summary>
        /// <param name="meals">List of meals to calculate totals for</param>
        /// <returns>Tuple containing total calories, protein, carbs, and fat</returns>
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

        /// <summary>
        /// Validates if a meal plan meets the user's nutritional goals
        /// </summary>
        /// <param name="meals">List of meals to validate</param>
        /// <param name="targetCalories">Target calories</param>
        /// <param name="targetProtein">Target protein</param>
        /// <param name="targetCarbs">Target carbs</param>
        /// <param name="targetFat">Target fat</param>
        /// <param name="tolerance">Acceptable tolerance percentage (default 10%)</param>
        /// <returns>True if meal plan is within acceptable range, false otherwise</returns>
        public bool ValidateMealPlan(List<Meal> meals, int targetCalories, int targetProtein, int targetCarbs, int targetFat, double tolerance = 0.10)
        {
            var (totalCalories, totalProtein, totalCarbs, totalFat) = CalculateTotalNutrition(meals);

            bool caloriesValid = Math.Abs(totalCalories - targetCalories) <= (targetCalories * tolerance);
            bool proteinValid = Math.Abs(totalProtein - targetProtein) <= (targetProtein * tolerance);
            bool carbsValid = Math.Abs(totalCarbs - targetCarbs) <= (targetCarbs * tolerance);
            bool fatValid = Math.Abs(totalFat - targetFat) <= (targetFat * tolerance);

            return caloriesValid && proteinValid && carbsValid && fatValid;
        }

        /// <summary>
        /// Gets a description of how calories are adjusted based on the user's goal
        /// </summary>
        /// <param name="goal">The user's fitness goal (bulk, cut, maintenance, well-being)</param>
        /// <param name="baseTDEE">The base Total Daily Energy Expenditure</param>
        /// <returns>A formatted string explaining the calorie adjustment</returns>
        public string GetCalorieAdjustmentDescription(string goal, int baseTDEE)
        {
            string adjustment = goal?.ToLower() switch
            {
                "bulk" => $"+300 kcal (Bulking phase: {baseTDEE} + 300 = {baseTDEE + 300} kcal)",
                "cut" => $"-300 kcal (Cutting phase: {baseTDEE} - 300 = {baseTDEE - 300} kcal)",
                "maintenance" => $"+100 kcal (Maintenance: {baseTDEE} + 100 = {baseTDEE + 100} kcal)",
                "well-being" => $"+100 kcal (Well-being: {baseTDEE} + 100 = {baseTDEE + 100} kcal)",
                _ => $"No adjustment (Base TDEE: {baseTDEE} kcal)"
            };

            return adjustment;
        }

        /// <summary>
        /// Gets an emoji representing the user's goal
        /// </summary>
        /// <param name="goal">The user's fitness goal</param>
        /// <returns>An emoji representing the goal</returns>
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

        /// <summary>
        /// Gets the user's goal from the database
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>The user's goal or "maintenance" as default</returns>
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

        /// <summary>
        /// Saves individual meals from a meal plan to the DailyLogs table
        /// </summary>
        /// <param name="mealPlanId">The meal plan ID to get meals from</param>
        public async Task SaveMealsToDailyLogAsync(int mealPlanId)
        {
            if (!UserSession.UserId.HasValue)
            {
                throw new InvalidOperationException("User must be logged in to save daily logs.");
            }

            var meals = await GetMealsForMealPlanAsync(mealPlanId);
            await _mealPlanRepository.SaveMealsToDailyLog(UserSession.UserId.Value, meals);
        }
    }
}

