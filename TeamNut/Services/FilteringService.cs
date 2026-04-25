namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;

    public class FilteringService : IFilteringService
    {
        public List<Meal> FilterMeals(
            IEnumerable<Meal> meals,
            string searchText,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (meals == null)
            {
                return new List<Meal>();
            }

            var query = searchText?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return meals.ToList();
            }

            return meals.Where(m => m.Name?.Contains(query, comparison) == true).ToList();
        }

        public List<Ingredient> FilterIngredients(
            IEnumerable<Ingredient> ingredients,
            string searchText,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (ingredients == null)
            {
                return new List<Ingredient>();
            }

            var query = searchText?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return ingredients.ToList();
            }

            return ingredients.Where(i => i.Name.Contains(query, comparison)).ToList();
        }
    }
}