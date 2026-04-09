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

                var existingShoppingItem = await _repository.GetByUserAndIngredient(userId, ingredientId);
                if (existingShoppingItem != null)
                {
                    existingShoppingItem.QuantityGrams += quantity;
                    await _repository.Update(existingShoppingItem);
                    return existingShoppingItem;
                }

                var newShoppingItem = new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = ingredientId,
                    IngredientName = itemName,
                    QuantityGrams = quantity,
                    IsChecked = false
                };

                await _repository.Add(newShoppingItem);
                return newShoppingItem;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RemoveItemAsync(ShoppingItem shoppingItem)
        {
            try
            {
                await _repository.Delete(shoppingItem.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(ShoppingItem shoppingItem)
        {
            try
            {
                await _repository.Update(shoppingItem);
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
                var neededIngredients = await _repository.GetIngredientsNeededFromMealPlan(userId);
                if (!neededIngredients.Any()) return 0;

                var inventory = (await _inventoryRepository.GetAllByUserId(userId)).ToList();

                var currentList = await _repository.GetAllByUserId(userId);

                int itemsAddedCount = 0;

                foreach (var needed in neededIngredients)
                {
                    double totalNeeded = needed.QuantityGrams;

                    var inStock = inventory.FirstOrDefault(i => i.IngredientId == needed.IngredientId);
                    if (inStock != null)
                    {
                        totalNeeded -= inStock.QuantityGrams;
                    }

                    var alreadyInList = currentList.FirstOrDefault(s => s.IngredientId == needed.IngredientId);
                    if (alreadyInList != null)
                    {
                        totalNeeded -= alreadyInList.QuantityGrams;
                    }

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
