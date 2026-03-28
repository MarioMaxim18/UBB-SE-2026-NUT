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
            meals = new List<Meal>()
            {
                new Meal
                {
                    Name = "Chicken Rice",
                    Calories = 500,
                    Protein = 35,
                    Carbs = 50,
                    Fat = 10,
                    IsVegan = false,
                    IsKeto = false,
                    IsGlutenFree = true,
                    IsLactoseFree = true,
                    IsNutFree = true
                },

                new Meal
                {
                    Name = "Beef Burger",
                    Calories = 700,
                    Protein = 40,
                    Carbs = 30,
                    Fat = 35,
                    IsVegan = false,
                    IsKeto = true,
                    IsGlutenFree = false,
                    IsLactoseFree = true,
                    IsNutFree = true
                },

                new Meal
                {
                    Name = "Salad",
                    Calories = 200,
                    Protein = 10,
                    Carbs = 15,
                    Fat = 5,
                    IsVegan = true,
                    IsKeto = true,
                    IsGlutenFree = true,
                    IsLactoseFree = true,
                    IsNutFree = true
                },

                new Meal
                {
                    Name = "Pasta",
                    Calories = 600,
                    Protein = 15,
                    Carbs = 70,
                    Fat = 12,
                    IsVegan = false,
                    IsKeto = false,
                    IsGlutenFree = false,
                    IsLactoseFree = false,
                    IsNutFree = true
                }
            };
        }

        public List<Meal> SearchMeals(string query, bool vegan, bool keto, bool gluten, bool lactose, bool nuts, bool favorites)
        {
            return meals
                .Where(m => m.Name.ToLower().Contains(query.ToLower()))
                .Where(m => !vegan || m.IsVegan)
                .Where(m => !keto || m.IsKeto)
                .Where(m => !gluten || m.IsGlutenFree)
                .Where(m => !lactose || m.IsLactoseFree)
                .Where(m => !nuts || m.IsNutFree)
                .Where(m => !favorites || m.IsFavorite)
                .ToList();
        }

        public void ToggleFavorite(Meal meal)
        {
            meal.IsFavorite = !meal.IsFavorite;
        }
    }
}