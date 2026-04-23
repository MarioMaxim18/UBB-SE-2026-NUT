using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class ShoppingListService
    {
        private readonly ShoppingListRepository _repository;
        private readonly IngredientRepository _ingredientRepository;
        private readonly InventoryRepository _inventoryRepository;

        public ShoppingListService()
        {
            _repository = new ShoppingListRepository();
            _ingredientRepository = new IngredientRepository();
            _inventoryRepository = new InventoryRepository();
        }

        public async Task<List<ShoppingItem>> GetListItemsAsync(int userId)
        {
            return await _repository.GetAllByUserId(userId);
        }

        public async Task<ShoppingItem> AddItemAsync(string itemName, int userId, double quantity = 0)
        {
            try
            {
                int ingredientId = await _ingredientRepository.GetOrCreateIngredientIdAsync(itemName);

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

        public async Task<bool> MoveToInventoryAsync(ShoppingItem item)
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

        public async Task<int> GenerateFromMealPlanAsync(int userId)
        {
            try
            {
                var neededIngredients = await _repository.GetIngredientsNeededFromMealPlan(userId);
                if (!neededIngredients.Any()) return 0;

                var inventory = (await _inventoryRepository.GetAllByUserId(userId)).ToList();
                var currentList = await _repository.GetAllByUserId(userId);

                int itemsAddedCount = 0;

                foreach (var needed in neededIngredients)
                {
                    double totalNeeded = needed.QuantityGrams;

                    
                    var inStock = inventory.FirstOrDefault(i => i.IngredientId == needed.IngredientId);
                    if (inStock != null) totalNeeded -= inStock.QuantityGrams;

                  
                    var alreadyInList = currentList.FirstOrDefault(s => s.IngredientId == needed.IngredientId);
                    if (alreadyInList != null) totalNeeded -= alreadyInList.QuantityGrams;

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