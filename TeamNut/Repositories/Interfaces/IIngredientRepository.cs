// <copyright file="IIngredientRepository.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IIngredientRepository
    {
        Task<List<Ingredient>> GetAllAsync();
        Task<int> GetOrCreateIngredientIdAsync(string name);
        Task<int> GetOrCreateIngredientIdByNameAsync(string name);
        Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search);
    }
}
