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
        private readonly int IngredientsQuantity = 100;

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
            _mealPlanRepository = new MealPlanRepository();
            _ingredientRepository = new IngredientRepository();
        }

        public async Task<bool> ConsumeMeal(int userId, int mealId)
        {
            
            var requiredIngredients = await _mealPlanRepository.GetIngredientsForMeal(mealId);

            foreach (var req in requiredIngredients)
            {
                var inventoryItems = await _inventoryRepository.GetAllByUserId(userId);
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);

                if (stock != null)
                {
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

        public async Task AddIngredientByNameToPantry(int userId, string ingredientName)
        {
            int ingredientId = await _ingredientRepository.GetOrCreateIngredientIdByNameAsync(ingredientName);
            await AddToPantry(userId, ingredientId, IngredientsQuantity);
        }

        public async Task<IEnumerable<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryRepository.GetAllByUserId(userId);
        }

        public async Task RemoveItem(int inventoryId)
        {
            await _inventoryRepository.Delete(inventoryId);
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredients()
        {
            return await _ingredientRepository.GetAllAsync();
        }
    }
}
