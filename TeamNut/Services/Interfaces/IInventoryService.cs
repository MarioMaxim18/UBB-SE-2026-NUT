using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IInventoryService
    {
        Task AddIngredientByNameToPantry(int userId, string ingredientName);
        Task AddToPantry(int userId, int ingredientId, int quantity);
        Task<bool> ConsumeMeal(int userId, int mealId);
        Task<IEnumerable<Ingredient>> GetAllIngredients();
        Task<IEnumerable<Inventory>> GetUserInventory(int userId);
        Task RemoveItem(int inventoryId);
    }
}