using System.Collections.Generic;
using TeamNut.Models;

namespace TeamNut.ViewModels
{
    public class MealSearchViewModel
    {
        public MealSearchViewModel()
        {
            // Ready for future service integration
        }

        public List<Meal> SearchMeals(MealFilter filter)
        {
            return new List<Meal>(); // vnothing until
        }

        public void ToggleFavorite(Meal meal)
        {
            if (meal != null)
                meal.IsFavorite = !meal.IsFavorite;
        }
    }
}