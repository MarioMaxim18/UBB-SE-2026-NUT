using System;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        public UserService()
        {
            _userRepository = new UserRepository();
        }

        public async Task<bool> CheckIfUsernameExistsAsync(string username)
        {
            var users = await _userRepository.GetAll();
            return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAndPassword(username, password);
            if (user != null)
            {
                UserSession.Login(user.Id, user.Username, user.Role);
                return user;
            }

            return null;
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            if (await CheckIfUsernameExistsAsync(user.Username))
            {
                return null;
            }

            await _userRepository.Add(user);
            UserSession.Login(user.Id, user.Username, user.Role);
            return user;
        }

        public async Task<UserData> AddUserDataAsync(UserData data)
        {
            ApplyCalculatedNutrition(data);
            await _userRepository.AddUserData(data);
            return data;
        }

        public async Task<UserData> GetUserDataAsync(int userId)
        {
            return await _userRepository.GetUserDataByUserId(userId);
        }

        public async Task UpdateUserDataAsync(UserData data)
        {
            ApplyCalculatedNutrition(data);
            await _userRepository.UpdateUserData(data);
        }

        private static void ApplyCalculatedNutrition(UserData data)
        {
            if (data == null) return;

            data.Bmi = data.CalculateBmi();
            data.CalorieNeeds = data.CalculateCalorieNeeds();
            data.ProteinNeeds = data.CalculateProteinNeeds();
            data.FatNeeds = data.CalculateFatNeeds();
            data.CarbNeeds = data.CalculateCarbNeeds();
        }
    }
}
