using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services
{
    public class ShoppingListService
    {
        private readonly TeamNut.Repositories.ShoppingListRepository _repository;
        private readonly TeamNut.Repositories.IngredientRepository _ingredientRepository;
        private readonly TeamNut.Repositories.InventoryRepository _inventoryRepository;

        public ShoppingListService()
        {
            _repository = new TeamNut.Repositories.ShoppingListRepository();
            _ingredientRepository = new TeamNut.Repositories.IngredientRepository();
            _inventoryRepository = new TeamNut.Repositories.InventoryRepository();
        }

        public async Task<List<ShoppingItem>> GetShoppingItemsAsync(int userId)
        {
            return await _repository.GetAllByUserId(userId);
        }

        public async Task<ShoppingItem> AddItemAsync(string itemName, int userId, double quantity = 0)
        {
            try
            {
                int ingredientId = await _ingredientRepository.GetOrCreateIngredientIdAsync(itemName);

                // Check if already exists for this user
                var existing = await _repository.GetByUserAndIngredient(userId, ingredientId);
                if (existing != null)
                {
                    existing.QuantityGrams += quantity;
                    await _repository.Update(existing);
                    return existing;
                }

                var newItem = new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = ingredientId,
                    IngredientName = itemName,
                    QuantityGrams = quantity,
                    IsChecked = false
                };

                await _repository.Add(newItem);
                return newItem;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RemoveItemAsync(ShoppingItem item)
        {
            try
            {
                await _repository.Delete(item.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(ShoppingItem item)
        {
            try
            {
                await _repository.Update(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            return await _ingredientRepository.SearchIngredientsAsync(search);
        }

        public async Task<bool> MoveToPantryAsync(ShoppingItem item)
        {
            try
            {
                await _inventoryRepository.Add(new Inventory
                {
                    UserId = item.UserId,
                    IngredientId = item.IngredientId,
                    QuantityGrams = item.QuantityGrams > 0 ? (int)item.QuantityGrams : 100
                });
                await _repository.Delete(item.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GenerateListAsync(int userId)
        {
            try
            {
                // 1. Get what we need from Meal Plan
                var neededIngredients = await _repository.GetIngredientsNeededFromMealPlan(userId);
                if (!neededIngredients.Any()) return 0;

                // 2. Get what we already have in Inventory
                var inventory = (await _inventoryRepository.GetAllByUserId(userId)).ToList();

                // 3. Get what is already in Shopping List
                var currentList = await _repository.GetAllByUserId(userId);

                int itemsAddedCount = 0;

                foreach (var needed in neededIngredients)
                {
                    double totalNeeded = needed.QuantityGrams;

                    // Subtract what we have in inventory
                    var inStock = inventory.FirstOrDefault(i => i.IngredientId == needed.IngredientId);
                    if (inStock != null)
                    {
                        totalNeeded -= inStock.QuantityGrams;
                    }

                    // Subtract what is already in shopping list
                    var alreadyInList = currentList.FirstOrDefault(s => s.IngredientId == needed.IngredientId);
                    if (alreadyInList != null)
                    {
                        totalNeeded -= alreadyInList.QuantityGrams;
                    }

                    // If we still need more, add/update it
                    if (totalNeeded > 0)
                    {
                        await AddItemAsync(needed.IngredientName, userId, totalNeeded);
                        itemsAddedCount++;
                    }
                }

                return itemsAddedCount;
            }
            catch
            {
                return -1;
            }
        }
    }
}
