using System.Collections.Generic;
using TeamNut.Models;

namespace TeamNut.ViewModels
{
    public class MealSearchViewModel
    {
        public MealSearchViewModel()
        {
            // TODO: inject MealService in future PR
        }

        public List<Meal> SearchMeals(object filter)
        {
            return new List<Meal>(); // vazio por enquanto
        }

        public void ToggleFavorite(Meal meal)
        {
            if (meal != null)
                meal.IsFavorite = !meal.IsFavorite;
        }
    }
}