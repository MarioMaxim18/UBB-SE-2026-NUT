// <copyright file="IMealService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IMealService
    {
        Task<List<Meal>> GetAllAsync();
        Task<Meal?> GetByIdAsync(int id);
        Task<List<Meal>> GetFilteredMealsAsync(MealFilter filter);
        Task<List<string>> GetMealIngredientLinesAsync(int mealId);
        Task<List<Meal>> GetMealsAsync(MealFilter? filter = null);
        Task ToggleFavoriteAsync(Meal meal);
    }
}
