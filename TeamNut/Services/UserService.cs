using System;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services.Interfaces;

namespace TeamNut.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        public UserService(IUserRepository uuserRepository)
        {
            userRepository = uuserRepository;
        }

        /// <summary>Checks whether a username is already taken.</summary>
        /// <param name="username">The username to check.</param>
        /// <returns><c>true</c> if the username exists; otherwise <c>false</c>.</returns>
        public async Task<bool> CheckIfUsernameExistsAsync(string username)
        {
            var users = await userRepository.GetAll();
            return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Attempts to log in with the given credentials.</summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The authenticated <see cref="User"/>, or <c>null</c> on failure.</returns>
        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await userRepository.GetByUsernameAndPassword(username, password);
            if (user != null)
            {
                UserSession.Login(user.Id, user.Username, user.Role);
                return user;
            }

            return null;
        }

        /// <summary>Registers a new user if the username is not already taken.</summary>
        /// <param name="user">The user to register.</param>
        /// <returns>The registered <see cref="User"/>, or <c>null</c> if the username exists.</returns>
        public async Task<User?> RegisterUserAsync(User user)
        {
            if (await CheckIfUsernameExistsAsync(user.Username))
            {
                return null;
            }

            await userRepository.Add(user);
            UserSession.Login(user.Id, user.Username, user.Role);
            return user;
        }

        /// <summary>Persists health data for a user.</summary>
        /// <param name="data">The health data to add.</param>
        /// <returns>The saved <see cref="UserData"/>.</returns>
        public async Task<UserData> AddUserDataAsync(UserData data)
        {
            ApplyCalculatedNutrition(data);
            await userRepository.AddUserData(data);
            return data;
        }

        /// <summary>Retrieves health data for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The <see cref="UserData"/> or <c>null</c> if not found.</returns>
        public async Task<UserData?> GetUserDataAsync(int userId)
        {
            return await userRepository.GetUserDataByUserId(userId);
        }

        /// <summary>Updates existing health data for a user.</summary>
        /// <param name="data">The updated health data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateUserDataAsync(UserData data)
        {
            ApplyCalculatedNutrition(data);
            await userRepository.UpdateUserData(data);
        }

        private static void ApplyCalculatedNutrition(UserData data)
        {
            if (data == null)
            {
                return;
            }

            data.Bmi = data.CalculateBmi();
            data.CalorieNeeds = data.CalculateCalorieNeeds();
            data.ProteinNeeds = data.CalculateProteinNeeds();
            data.FatNeeds = data.CalculateFatNeeds();
            data.CarbNeeds = data.CalculateCarbNeeds();
        }
    }
}
