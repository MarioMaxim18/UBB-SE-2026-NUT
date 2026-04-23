using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IShoppingListRepository
    {
        Task Add(ShoppingItem item);
        Task Delete(int id);
        Task<IEnumerable<ShoppingItem>> GetAll();
        Task<List<ShoppingItem>> GetAllByUserId(int userId);
        Task<ShoppingItem?> GetById(int id);
        Task<ShoppingItem?> GetByUserAndIngredient(int userId, int ingredientId);
        Task<List<ShoppingItem>> GetIngredientsNeededFromMealPlan(int userId);
        Task Update(ShoppingItem item);
    }
}
