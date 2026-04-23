// <copyright file="IDailyLogService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IDailyLogService
    {
        Task<UserData?> GetCurrentUserNutritionTargetsAsync();
        Task<DailyLog> GetCurrentWeekTotalsAsync();
        Task<List<Meal>> GetMealsForAutocompleteAsync();
        Task<double> GetTodayBurnedCaloriesAsync();
        Task<DailyLog> GetTodayTotalsAsync();
        Task<bool> HasAnyLogsAsync();
        Task LogMealAsync(Meal meal);
        Task<List<Meal>> SearchMealsAsync(string? searchTerm);
    }
}
