// <copyright file="IShoppingListService.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IShoppingListService
    {
        Task<ShoppingItem?> AddItemAsync(string itemName, int userId, double quantity = 0);
        Task<int> GenerateListAsync(int userId);
        Task<List<ShoppingItem>> GetShoppingItemsAsync(int userId);
        Task<bool> MoveToPantryAsync(ShoppingItem item);
        Task<bool> RemoveItemAsync(ShoppingItem item);
        Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search);
        Task<bool> UpdateItemAsync(ShoppingItem item);
    }
}
