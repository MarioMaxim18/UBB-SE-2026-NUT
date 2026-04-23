using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Views.MealPlanView;

namespace TeamNut.Repositories.Interfaces
{
    public interface IMealPlanRepository
    {
        Task Add(MealPlan entity);
        Task Delete(int id);
        Task<int> GeneratePersonalizedDailyMealPlan(int userId);
        Task<IEnumerable<MealPlan>> GetAll();
        Task<MealPlan?> GetById(int id);
        Task<List<IngredientViewModel>> GetIngredientsForMeal(int mealId);
        Task<MealPlan?> GetLatestMealPlan(int userId);
        Task<List<Meal>> GetMealsForMealPlan(int mealPlanId);
        Task<MealPlan?> GetTodaysMealPlan(int userId);
        Task SaveMealsToDailyLog(int userId, List<Meal> meals);
        Task SaveMealToDailyLog(int userId, int mealId, int calories);
        Task Update(MealPlan entity);
    }
}