using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using System.Linq;

namespace TeamNut.Backend.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly MealPlanRepository _mealPlanRepository;

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
            _mealPlanRepository = new MealPlanRepository();
        }

        /// <summary>
        /// Requirement: "Whenever an user logs a meal the ingredients for it 
        /// will be removed from the database."
        /// </summary>
        public async Task<bool> ConsumeMeal(int userId, int mealId)
        {
            // get the list of ingredients required for this meal
            
            var requiredIngredients = await _mealPlanRepository.GetIngredientsForMeal(mealId);

            foreach (var req in requiredIngredients)
            {
                // fetch the user's current inventory for this specific ingredient
                var inventoryItems = await _inventoryRepository.GetAllByUserId(userId);
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);

                if (stock != null)
                {
                    // ingredient quantities from recipes can be fractional (double). Inventory stores grams as int,
                    // so round the required quantity to the nearest gram before subtracting.
                    int qtyToRemove = (int)Math.Round(req.Quantity);
                    stock.QuantityGrams -= qtyToRemove;

                    if (stock.QuantityGrams <= 0)
                    {
                        await _inventoryRepository.Delete(stock.Id);
                    }
                    else
                    {
                        await _inventoryRepository.Update(stock);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Requirement: "Whenever an user marks off items from their shopping list 
        /// they will be added to the database."
        /// </summary>
        public async Task AddToPantry(int userId, int ingredientId, int quantity)
        {
            var newItem = new Inventory
            {
                UserId = userId,
                IngredientId = ingredientId,
                QuantityGrams = quantity
            };

            await _inventoryRepository.Add(newItem);
        }

        /// <summary>
        /// Fetches all items for the "Inventory" tab display
        /// </summary>
        public async Task<IEnumerable<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryRepository.GetAllByUserId(userId);
        }

        /// <summary>
        /// Requirement: Handles the [X] button logic on the UI
        /// </summary>
        public async Task RemoveItem(int inventoryId)
        {
            await _inventoryRepository.Delete(inventoryId);
        }
    }
}