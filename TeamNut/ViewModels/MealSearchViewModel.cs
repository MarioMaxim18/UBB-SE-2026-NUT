using System.Collections.Generic;
using System.Linq;
using TeamNut.Models;

namespace TeamNut.ViewModels
{
    public class MealSearchViewModel
    {
        private List<Meal> meals;
        public MealSearchViewModel()
        {
            meals = new List<Meal>
    {
        new Meal { Name = "Chicken Rice", Calories = 500 },
        new Meal { Name = "Beef Burger", Calories = 700 },
        new Meal { Name = "Salad", Calories = 200 },
        new Meal { Name = "Pasta", Calories = 600 }
    };
        }

        public List<Meal> SearchMeals(MealFilter filter)
        {
            if (filter == null || string.IsNullOrEmpty(filter.SearchTerm))
                return meals;

            return meals
                .Where(m => m.Name.ToLower().Contains(filter.SearchTerm.ToLower()))
                .ToList();
        }

        public void ToggleFavorite(Meal meal)
        {
            if (meal != null)
                meal.IsFavorite = !meal.IsFavorite;
        }
    }
}