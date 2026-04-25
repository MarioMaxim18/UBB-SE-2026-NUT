namespace TeamNut.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class ShoppingListService : IShoppingListService
    {
        private readonly IShoppingListRepository repository;

        private readonly IIngredientRepository ingredientRepository;

        private readonly IInventoryRepository inventoryRepository;

        public ShoppingListService(IShoppingListRepository shoppingListRepository, IIngredientRepository ingredientRepository, IInventoryRepository inventoryRepository)
        {
            this.repository = shoppingListRepository;
            this.ingredientRepository = ingredientRepository;
            this.inventoryRepository = inventoryRepository;
        }

        public async Task<List<ShoppingItem>> GetShoppingItemsAsync(int userId)
        {
            return await this.repository.GetAllByUserId(userId);
        }

        public async Task<ShoppingItem?> AddItemAsync(string itemName, int userId, double quantity = 0)
        {
            try
            {
                int ingredientId = await this.ingredientRepository.GetOrCreateIngredientIdAsync(itemName);

                var existing = await this.repository.GetByUserAndIngredient(userId, ingredientId);
                if (existing != null)
                {
                    existing.QuantityGrams += quantity;
                    await this.repository.Update(existing);
                    return existing;
                }

                var newItem = new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = ingredientId,
                    IngredientName = itemName,
                    QuantityGrams = quantity,
                    IsChecked = false,
                };

                await this.repository.Add(newItem);
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
                await this.repository.Delete(item.Id);
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
                await this.repository.Update(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            return await this.ingredientRepository.SearchIngredientsAsync(search);
        }

        public async Task<bool> MoveToPantryAsync(ShoppingItem item)
        {
            try
            {
                await this.inventoryRepository.Add(new Inventory
                {
                    UserId = item.UserId,
                    IngredientId = item.IngredientId,
                    QuantityGrams = item.QuantityGrams > 0 ? (int)item.QuantityGrams : 100,
                });
                await this.repository.Delete(item.Id);
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
                var neededIngredients = await this.repository.GetIngredientsNeededFromMealPlan(userId);
                if (!neededIngredients.Any())
                {
                    return 0;
                }

                var inventory = (await this.inventoryRepository.GetAllByUserId(userId)).ToList();
                var currentList = await this.repository.GetAllByUserId(userId);
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
                        var added = await this.AddItemAsync(needed.IngredientName, userId, totalNeeded);
                        if (added != null)
                        {
                            itemsAddedCount++;
                        }
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
