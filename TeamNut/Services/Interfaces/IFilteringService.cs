namespace TeamNut.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using TeamNut.Models;

    public interface IFilteringService
    {
        List<Meal> FilterMeals(IEnumerable<Meal> meals, string searchText, StringComparison comparison = StringComparison.OrdinalIgnoreCase);
        List<Ingredient> FilterIngredients(IEnumerable<Ingredient> ingredients, string searchText, StringComparison comparison = StringComparison.OrdinalIgnoreCase);
    }
}
