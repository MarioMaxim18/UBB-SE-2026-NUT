using System.Collections.Generic;
using TeamNut.Models;

namespace TeamNut.Views.MealPlanView
{
    public class MealViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string MealType { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        
        public bool IsVegan { get; set; }
        public bool IsKeto { get; set; }
        public bool IsGlutenFree { get; set; }
        public bool IsLactoseFree { get; set; }
        public bool IsNutFree { get; set; }

        public List<IngredientViewModel> Ingredients { get; set; } = new List<IngredientViewModel>();

        public static MealViewModel FromMeal(Meal meal, string mealType)
        {
            return new MealViewModel
            {
                Id = meal.Id,
                Name = meal.Name ?? string.Empty,
                Description = meal.Description ?? string.Empty,
                ImageUrl = meal.ImageUrl ?? string.Empty,
                MealType = mealType,
                Calories = meal.Calories,
                Protein = meal.Protein,
                Carbs = meal.Carbs,
                Fat = meal.Fat,
                IsVegan = meal.IsVegan,
                IsKeto = meal.IsKeto,
                IsGlutenFree = meal.IsGlutenFree,
                IsLactoseFree = meal.IsLactoseFree,
                IsNutFree = meal.IsNutFree
            };
        }
    }

    public class IngredientViewModel
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
    }
}