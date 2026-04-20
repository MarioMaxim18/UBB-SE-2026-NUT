using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository inventoryRepository;
        private readonly MealPlanRepository mealPlanRepository;
        private readonly IngredientRepository ingredientRepository;

        public InventoryService()
        {
            inventoryRepository = new InventoryRepository();
            mealPlanRepository = new MealPlanRepository();
            ingredientRepository = new IngredientRepository();
        }

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

        public async Task AddIngredientByNameToPantry(int userId, string ingredientName)
        {
            int ingredientId = await ingredientRepository.GetOrCreateIngredientIdByNameAsync(ingredientName);
            await AddToPantry(userId, ingredientId, 100);
        }

        public async Task<IEnumerable<Inventory>> GetUserInventory(int userId)
        {
            return await inventoryRepository.GetAllByUserId(userId);
        }

        public async Task RemoveItem(int inventoryId)
        {
            await inventoryRepository.Delete(inventoryId);
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredients()
        {
            return await ingredientRepository.GetAllAsync();
        }
    }
}
