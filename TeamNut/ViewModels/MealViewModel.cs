using System.Collections.Generic;
using TeamNut.Models;

namespace TeamNut.Views.MealPlanView
{
    /// <summary>Represents a meal entry within a generated meal plan.</summary>
    public class MealViewModel
    {
        /// <summary>Gets or sets the meal identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the meal name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the meal description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets the URL of the meal image.</summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the meal type (e.g. BREAKFAST, LUNCH, DINNER).</summary>
        public string MealType { get; set; } = string.Empty;

        /// <summary>Gets or sets the calorie count.</summary>
        public int Calories { get; set; }

        /// <summary>Gets or sets the protein grams.</summary>
        public int Protein { get; set; }

        /// <summary>Gets or sets the carbohydrate grams.</summary>
        public int Carbs { get; set; }

        /// <summary>Gets or sets the fat grams.</summary>
        public int Fat { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is vegan.</summary>
        public bool IsVegan { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is keto-friendly.</summary>
        public bool IsKeto { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is gluten-free.</summary>
        public bool IsGlutenFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is lactose-free.</summary>
        public bool IsLactoseFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is nut-free.</summary>
        public bool IsNutFree { get; set; }

        /// <summary>Gets or sets the list of ingredient view models for this meal.</summary>
        public List<IngredientViewModel> Ingredients { get; set; } = new List<IngredientViewModel>();

        /// <summary>Creates a <see cref="MealViewModel"/> from a <see cref="Meal"/> model.</summary>
        /// <param name="meal">The source meal model.</param>
        /// <param name="mealType">The meal type label (e.g. BREAKFAST).</param>
        /// <returns>A new <see cref="MealViewModel"/> populated from the given meal.</returns>
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
                IsNutFree = meal.IsNutFree,
            };
        }
    }

    /// <summary>Represents an ingredient within a meal view model.</summary>
    public class IngredientViewModel
    {
        /// <summary>Gets or sets the ingredient identifier.</summary>
        public int IngredientId { get; set; }

        /// <summary>Gets or sets the ingredient name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the quantity in grams.</summary>
        public double Quantity { get; set; }

        /// <summary>Gets or sets the calorie contribution.</summary>
        public double Calories { get; set; }

        /// <summary>Gets or sets the protein contribution in grams.</summary>
        public double Protein { get; set; }

        /// <summary>Gets or sets the carbohydrate contribution in grams.</summary>
        public double Carbs { get; set; }

        /// <summary>Gets or sets the fat contribution in grams.</summary>
        public double Fat { get; set; }
    }
}
