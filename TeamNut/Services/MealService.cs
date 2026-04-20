using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    /// <summary>Service for retrieving and managing meal data.</summary>
    public class MealService
    {
        private readonly MealRepository mealRepository;

        /// <summary>Initializes a new instance of the <see cref="MealService"/> class.</summary>
        public MealService()
        {
            mealRepository = new MealRepository();
        }

        /// <summary>Gets meals, optionally filtered by a <see cref="MealFilter"/>.</summary>
        /// <param name="filter">Optional filter to apply.</param>
        /// <returns>A list of meals.</returns>
        public async Task<List<Meal>> GetMealsAsync(MealFilter? filter = null)
        {
            if (filter == null)
            {
                var allMeals = await mealRepository.GetAll();
                return allMeals.ToList();
            }

            var results = await mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

        /// <summary>Gets meals matching the given filter.</summary>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>A list of filtered meals.</returns>
        public async Task<List<Meal>> GetFilteredMealsAsync(MealFilter filter)
        {
            var results = await mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

        /// <summary>Gets a meal by its identifier.</summary>
        /// <param name="id">The meal identifier.</param>
        /// <returns>The <see cref="Meal"/>, or <c>null</c> if not found.</returns>
        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await mealRepository.GetById(id);
        }

        /// <summary>Gets all meals.</summary>
        /// <returns>A list of all meals.</returns>
        public async Task<List<Meal>> GetAllAsync()
        {
            var list = await mealRepository.GetAll();
            return list.ToList();
        }

        /// <summary>Toggles the favourite state of a meal for the current user.</summary>
        /// <param name="meal">The meal to toggle.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ToggleFavoriteAsync(Meal meal)
        {
            if (meal == null || !UserSession.UserId.HasValue)
            {
                return;
            }

            await mealRepository.SetFavoriteAsync(UserSession.UserId.Value, meal.Id, meal.IsFavorite);
        }

        /// <summary>Gets a formatted ingredient list for the given meal.</summary>
        /// <param name="mealId">The meal identifier.</param>
        /// <returns>A list of ingredient description strings.</returns>
        public async Task<List<string>> GetMealIngredientLinesAsync(int mealId)
        {
            return await mealRepository.GetIngredientLinesForMealAsync(mealId);
        }
    }
}
