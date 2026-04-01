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

        public async Task<List<Meal>> GetMealsAsync(string? filter = null)
        {
            var meals = (await _mealRepository.GetAll()).ToList();

            if (string.IsNullOrWhiteSpace(filter) || filter == "All")
                return meals;

            var filtered = meals.Where(m => (m.Name ?? string.Empty).Contains(filter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            return filtered;
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
    }
}
