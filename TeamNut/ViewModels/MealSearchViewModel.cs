using System.Collections.Generic;
using System.Linq;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.ViewModels
{
    public class MealSearchViewModel
    {
        private List<Meal> meals;

        public MealSearchViewModel()
        {
            try
            {
                var repo = new MealRepository();
                meals = repo.GetMeals(); // DATABASE CALL
            }
            catch
            {
                // check crash 
                meals = new List<Meal>();
            }
        }

        public List<Meal> SearchMeals(MealFilter filter)
        {
            var result = meals;

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                result = result
                    .Where(m => m.Name.ToLower().Contains(filter.SearchTerm.ToLower()))
                    .ToList();
            }

            // 🔥 FILTERS
            if (filter.IsVegan)
                result = result.Where(m => m.IsVegan).ToList();

            if (filter.IsKeto)
                result = result.Where(m => m.IsKeto).ToList();

            if (filter.IsGlutenFree)
                result = result.Where(m => m.IsGlutenFree).ToList();

            if (filter.IsLactoseFree)
                result = result.Where(m => m.IsLactoseFree).ToList();

            if (filter.IsNutFree)
                result = result.Where(m => m.IsNutFree).ToList();

            return result;
        }

        public void ToggleFavorite(Meal meal)
        {
            if (meal != null)
                meal.IsFavorite = !meal.IsFavorite;
        }
    }
}