using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using System.Linq;

namespace TeamNut.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly MealPlanRepository _mealPlanRepository;
        private readonly IngredientRepository _ingredientRepository;

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
            _mealPlanRepository = new MealPlanRepository();
            _ingredientRepository = new IngredientRepository();
        }

        public async Task<bool> DeductMealIngredientsAsync(int userId, int mealId)
        {
            var requiredIngredients = await _mealPlanRepository.GetIngredientsForMeal(mealId);
            var inventoryItems = await _inventoryRepository.GetAllByUserId(userId);

            foreach (var req in requiredIngredients)
            {
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);

                if (stock != null)
                {
                    int qtyToRemove = (int)Math.Round(req.Quantity);
                    stock.QuantityGrams -= qtyToRemove;

                    // Remove item completely if stock is depleted
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

        public async Task AddToInventoryAsync(int userId, int ingredientId, int quantity)
        {
            var newItem = new Inventory
            {
                UserId = userId,
                IngredientId = ingredientId,
                QuantityGrams = quantity
            };

            await _inventoryRepository.Add(newItem);
        }

        public async Task AddIngredientByNameAsync(int userId, string ingredientName)
        {
            int ingredientId = await _ingredientRepository.GetOrCreateIngredientIdByNameAsync(ingredientName);
            await AddToInventoryAsync(userId, ingredientId, 100);
        }

        public async Task<IEnumerable<Inventory>> GetInventoryAsync(int userId)
        {
            return await _inventoryRepository.GetAllByUserId(userId);
        }

        public async Task RemoveItemAsync(int inventoryId)
        {
            await _inventoryRepository.Delete(inventoryId);
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredientsAsync()
        {
            return await _ingredientRepository.GetAllAsync();
        }
    }
}