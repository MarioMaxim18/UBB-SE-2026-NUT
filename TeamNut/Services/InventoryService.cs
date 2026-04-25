namespace TeamNut.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;

        private readonly IMealPlanRepository mealPlanRepository;

        private readonly IIngredientRepository ingredientRepository;

        private readonly int ingredientsQuantity = 100;

        public InventoryService(IIngredientRepository ingredientRepository, IInventoryRepository inventoryRepository, IMealPlanRepository mealPlanRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.mealPlanRepository = mealPlanRepository;
            this.ingredientRepository = ingredientRepository;
        }

        public async Task<bool> ConsumeMeal(int userId, int mealId)
        {
            var requiredIngredients = await this.mealPlanRepository.GetIngredientsForMeal(mealId);
            var inventoryItems = (await this.inventoryRepository.GetAllByUserId(userId)).ToList();

            foreach (var req in requiredIngredients)
            {
                var stock = inventoryItems.FirstOrDefault(i => i.IngredientId == req.IngredientId);
                int quantityToRemove = (int)Math.Round(req.Quantity);

                if (stock == null || stock.QuantityGrams < quantityToRemove)
                {
                    return false;
                }

                stock.QuantityGrams -= quantityToRemove;

                if (stock.QuantityGrams <= 0)
                {
                    await this.inventoryRepository.Delete(stock.Id);
                }
                else
                {
                    await this.inventoryRepository.Update(stock);
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

            await this.inventoryRepository.Add(newItem);
        }

        public async Task AddIngredientByNameToPantry(int userId, string ingredientName)
        {
            int ingredientId = await this.ingredientRepository.GetOrCreateIngredientIdByNameAsync(ingredientName);
            await this.AddToPantry(userId, ingredientId, this.ingredientsQuantity);
        }

        public async Task<IEnumerable<Inventory>> GetUserInventory(int userId)
        {
            return await this.inventoryRepository.GetAllByUserId(userId);
        }

        public async Task RemoveItem(int inventoryId)
        {
            await this.inventoryRepository.Delete(inventoryId);
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredients()
        {
            return await this.ingredientRepository.GetAllAsync();
        }
    }
}
