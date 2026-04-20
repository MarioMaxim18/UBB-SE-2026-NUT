using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class MealService
    {
        private readonly MealRepository _mealRepository;

        public MealService()
        {
            _mealRepository = new MealRepository();
        }

        
        public async Task<List<Meal>> GetMealsAsync(MealFilter? filter = null)
        {
            
            if (filter == null)
            {
                var allMeals = await _mealRepository.GetAll();
                return allMeals.ToList();
            }

            
            var results = await _mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

      
        public async Task<List<Meal>> GetFilteredMealsAsync(MealFilter filter)
        {
            var results = await _mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await _mealRepository.GetById(id);
        }

        public async Task<List<Meal>> GetAllAsync()
        {
            var list = await _mealRepository.GetAll();
            return list.ToList();
        }

        public async Task ToggleFavoriteAsync(Meal meal)
        {
            if (meal == null || !UserSession.UserId.HasValue) return;
            await _mealRepository.SetFavoriteAsync(UserSession.UserId.Value, meal.Id, meal.IsFavorite);
        }

        public async Task<List<string>> GetMealIngredientLinesAsync(int mealId)
        {
            return await _mealRepository.GetIngredientLinesForMealAsync(mealId);
        }
    }
}