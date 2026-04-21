using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services.Interfaces;

namespace TeamNut.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly IMealPlanRepository mealPlanRepository;
        private readonly IIngredientRepository ingredientRepository;
        private readonly int ingredientsQuantity = 100;

        public InventoryService(IIngredientRepository iingredientRepository, IInventoryRepository iinventoryRepository, IMealPlanRepository mmealPlanRepository)
        {
            inventoryRepository = iinventoryRepository;
            mealPlanRepository = mmealPlanRepository;
            ingredientRepository = iingredientRepository;
        }

        /// <summary>Deducts ingredients from inventory when a meal is consumed.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="mealId">The meal identifier.</param>
        /// <returns><c>true</c> after consumption is processed.</returns>
        public async Task<bool> ConsumeMeal(int userId, int mealId)
        {
            var requiredIngredients = await mealPlanRepository.GetIngredientsForMeal(mealId);
            var inventoryItems = (await inventoryRepository.GetAllByUserId(userId)).ToList();

            foreach (var req in requiredIngredients)
            {
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);

                if (stock != null)
                {
                    int qtyToRemove = (int)Math.Round(req.Quantity);
                    stock.QuantityGrams -= qtyToRemove;

                    if (stock.QuantityGrams <= 0)
                    {
                        await inventoryRepository.Delete(stock.Id);
                    }
                    else
                    {
                        await inventoryRepository.Update(stock);
                    }
                }
            }

            return true;
        }

        /// <summary>Adds an ingredient to the user's pantry inventory.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ingredientId">The ingredient identifier.</param>
        /// <param name="quantity">The quantity in grams.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddToPantry(int userId, int ingredientId, int quantity)
        {
            var newItem = new Inventory
            {
                UserId = userId,
                IngredientId = ingredientId,
                QuantityGrams = quantity,
            };

            await inventoryRepository.Add(newItem);
        }

        /// <summary>Looks up or creates an ingredient by name and adds it to the pantry.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ingredientName">The ingredient name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddIngredientByNameToPantry(int userId, string ingredientName)
        {
            int ingredientId = await ingredientRepository.GetOrCreateIngredientIdByNameAsync(ingredientName);
            await AddToPantry(userId, ingredientId, ingredientsQuantity);
        }

        /// <summary>Gets all inventory items for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's inventory items.</returns>
        public async Task<IEnumerable<Inventory>> GetUserInventory(int userId)
        {
            return await inventoryRepository.GetAllByUserId(userId);
        }

        /// <summary>Removes an inventory item by its identifier.</summary>
        /// <param name="inventoryId">The inventory item identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveItem(int inventoryId)
        {
            await inventoryRepository.Delete(inventoryId);
        }

        /// <summary>Gets all available ingredients.</summary>
        /// <returns>All ingredients in the database.</returns>
        public async Task<IEnumerable<Ingredient>> GetAllIngredients()
        {
            return await ingredientRepository.GetAllAsync();
        }
    }
}
