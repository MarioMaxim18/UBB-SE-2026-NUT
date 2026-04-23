using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task Add(User entity);
        Task AddUserData(UserData data);
        Task Delete(int id);
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(int id);
        Task<User?> GetByUsernameAndPassword(string username, string password);
        Task<UserData?> GetUserDataByUserId(int userId);
        Task Update(User entity);
        Task UpdateUserData(UserData data);
    }
}