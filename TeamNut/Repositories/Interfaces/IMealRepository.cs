using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IMealRepository
    {
        Task Add(Meal entity);
        Task Delete(int id);
        Task<IEnumerable<Meal>> GetAll();
        Task<Meal?> GetById(int id);
        Task<IEnumerable<Meal>> GetFilteredMeals(MealFilter filter);
        Task<List<string>> GetIngredientLinesForMealAsync(int mealId);
        List<Meal> GetMeals();
        Task SetFavoriteAsync(int userId, int mealId, bool isFavorite);
        Task Update(Meal entity);
    }
}