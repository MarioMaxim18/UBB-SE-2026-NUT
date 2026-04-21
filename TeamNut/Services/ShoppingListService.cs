using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services.Interfaces;

namespace TeamNut.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly IShoppingListRepository repository;
        private readonly IIngredientRepository ingredientRepository;
        private readonly IInventoryRepository inventoryRepository;

        public ShoppingListService(IShoppingListRepository sshoppingListRepository, IIngredientRepository iingredientRepository, IInventoryRepository iinventoryRepository)
        {
            repository = sshoppingListRepository;
            ingredientRepository = iingredientRepository;
            inventoryRepository = iinventoryRepository;
        }

        /// <summary>Gets all shopping items for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A list of shopping items.</returns>
        public async Task<List<ShoppingItem>> GetShoppingItemsAsync(int userId)
        {
            return await repository.GetAllByUserId(userId);
        }

        /// <summary>Adds an ingredient to the shopping list, or increases its quantity if it already exists.</summary>
        /// <param name="itemName">The ingredient name.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="quantity">The quantity in grams.</param>
        /// <returns>The added or updated <see cref="ShoppingItem"/>, or <c>null</c> on error.</returns>
        public async Task<ShoppingItem?> AddItemAsync(string itemName, int userId, double quantity = 0)
        {
            try
            {
                int ingredientId = await ingredientRepository.GetOrCreateIngredientIdAsync(itemName);

                var existing = await repository.GetByUserAndIngredient(userId, ingredientId);
                if (existing != null)
                {
                    existing.QuantityGrams += quantity;
                    await repository.Update(existing);
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

                await repository.Add(newItem);
                return newItem;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Removes a shopping item from the list.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public async Task<bool> RemoveItemAsync(ShoppingItem item)
        {
            try
            {
                await repository.Delete(item.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Updates a shopping item in the database.</summary>
        /// <param name="item">The item to update.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public async Task<bool> UpdateItemAsync(ShoppingItem item)
        {
            try
            {
                await repository.Update(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Searches for ingredients matching the given text.</summary>
        /// <param name="search">The search string.</param>
        /// <returns>A list of ingredient id/name pairs.</returns>
        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            return await ingredientRepository.SearchIngredientsAsync(search);
        }

        /// <summary>Moves a shopping item to the user's inventory.</summary>
        /// <param name="item">The item to move.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public async Task<bool> MoveToPantryAsync(ShoppingItem item)
        {
            try
            {
                await inventoryRepository.Add(new Inventory
                {
                    UserId = item.UserId,
                    IngredientId = item.IngredientId,
                    QuantityGrams = item.QuantityGrams > 0 ? (int)item.QuantityGrams : 100,
                });
                await repository.Delete(item.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Generates shopping list items from the user's active meal plan.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The number of items added, 0 if nothing was needed, or -1 on error.</returns>
        public async Task<int> GenerateListAsync(int userId)
        {
            try
            {
                var neededIngredients = await repository.GetIngredientsNeededFromMealPlan(userId);
                if (!neededIngredients.Any())
                {
                    return 0;
                }

                var inventory = (await inventoryRepository.GetAllByUserId(userId)).ToList();
                var currentList = await repository.GetAllByUserId(userId);
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
                        var added = await AddItemAsync(needed.IngredientName, userId, totalNeeded);
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
