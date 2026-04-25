namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly IMealPlanRepository mealPlanRepository;
        private readonly IIngredientRepository ingredientRepository;

        private const int DefaultIngredientsQuantity = 100;

        public InventoryService(
            IIngredientRepository ingredientRepository,
            IInventoryRepository inventoryRepository,
            IMealPlanRepository mealPlanRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.mealPlanRepository = mealPlanRepository;
            this.ingredientRepository = ingredientRepository;
        }

        public async Task<bool> ConsumeMeal(int userId, int mealId)
        {
            var requiredIngredients = await mealPlanRepository.GetIngredientsForMeal(mealId);
            var inventoryItems = (await inventoryRepository.GetAllByUserId(userId)).ToList();

            foreach (var req in requiredIngredients)
            {
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);
                int qtyToRemove = (int)Math.Round(req.Quantity);

                if (stock == null || stock.QuantityGrams < qtyToRemove)
                {
                    return false;
                }

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
            await AddToPantry(userId, ingredientId, DefaultIngredientsQuantity);
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
