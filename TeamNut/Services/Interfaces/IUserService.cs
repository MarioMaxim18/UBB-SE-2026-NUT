using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserData> AddUserDataAsync(UserData data);
        Task<bool> CheckIfUsernameExistsAsync(string username);
        Task<UserData?> GetUserDataAsync(int userId);
        Task<User?> LoginAsync(string username, string password);
        Task<User?> RegisterUserAsync(User user);
        Task UpdateUserDataAsync(UserData data);
    }
}