using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IMealPlanService
    {
        (int totalCalories, int totalProtein, int totalCarbohydrates, int totalFat) CalculateTotalNutrition(List<Meal> meals);
        Task<int> GeneratePersonalizedMealPlanAsync(int userId);
        Task<IEnumerable<MealPlan>> GetAllMealPlansAsync();
        string GetCalorieAdjustmentDescription(string goal, int baseTDEE);
        string GetGoalEmoji(string goal);
        Task<MealPlan?> GetMealPlanByIdAsync(int mealPlanId);
        Task<List<Meal>> GetMealsForMealPlanAsync(int mealPlanId);
        Task<MealPlan?> GetTodaysMealPlanAsync(int userId);
        Task<string> GetUserGoalAsync(int userId);
        Task SaveMealsToDailyLogAsync(int mealPlanId);
        Task SaveMealToDailyLogAsync(int mealId, int calories);
        bool ValidateMealPlan(List<Meal> meals, int targetCalories, int targetProtein, int targetCarbohydrates, int targetFat, double tolerance = 0.1);
    }
}
