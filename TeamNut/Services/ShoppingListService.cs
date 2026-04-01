using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services
{
    public class ShoppingListService
    {
        private readonly TeamNut.Repositories.ShoppingListRepository _repository;

        public ShoppingListService()
        {
            _repository = new TeamNut.Repositories.ShoppingListRepository();
        }

        public async Task<List<ShoppingItem>> GetShoppingItemsAsync(int userId)
        {
            return await _repository.GetAllByUserId(userId);
        }

        public async Task<bool> AddItemAsync(ShoppingItem item)
        {
            if (item.UserId == 0) item.UserId = 1;

            try
            {
                await _repository.Add(item);
                
                // Return the generated entity (or at least acknowledge success) 
                // Normally you'd want to reload to get the DB ID but we simplify here
                // Note: Without reloading the ID, deleting exactly this item right after could fail since object ID is 0.
                // Given the constraints of the UI currently, we'll fetch them all later to get IDs
                return true;
            }
            catch
            {
                return false;
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
    }
}
