// <copyright file="MealService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class MealService : IMealService
    {
        private readonly IMealRepository mealRepository;

        public MealService(IMealRepository mmealRepository)
        {
            mealRepository = mmealRepository;
        }

        public async Task<List<Meal>> GetMealsAsync(MealFilter? filter = null)
        {
            if (filter == null)
            {
                var allMeals = await mealRepository.GetAll();
                return allMeals.ToList();
            }

            var results = await mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

        public async Task<List<Meal>> GetFilteredMealsAsync(MealFilter filter)
        {
            var results = await mealRepository.GetFilteredMeals(filter);
            return results.ToList();
        }

        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await mealRepository.GetById(id);
        }

        public async Task<List<Meal>> GetAllAsync()
        {
            var list = await mealRepository.GetAll();
            return list.ToList();
        }

        public async Task ToggleFavoriteAsync(Meal meal)
        {
            if (meal == null || !UserSession.UserId.HasValue)
            {
                return;
            }

            await mealRepository.SetFavoriteAsync(UserSession.UserId.Value, meal.Id, meal.IsFavorite);
        }

        public async Task<List<string>> GetMealIngredientLinesAsync(int mealId)
        {
            return await mealRepository.GetIngredientLinesForMealAsync(mealId);
        }
    }
}
