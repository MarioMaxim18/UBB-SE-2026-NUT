namespace TeamNut.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services.Interfaces;

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly INutritionCalculationService nutritionCalculationService;

        public UserService(
            IUserRepository userRepository,
            INutritionCalculationService nutritionCalculationService)
        {
            this.userRepository = userRepository;
            this.nutritionCalculationService = nutritionCalculationService;
        }

        public async Task<bool> CheckIfUsernameExistsAsync(string username)
        {
            var users = await userRepository.GetAll();

            return users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

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

        public async Task<User?> RegisterUserAsync(User user)
        {
            bool userExists = await CheckIfUsernameExistsAsync(user.Username);

            if (userExists)
            {
                return null;
            }

            await userRepository.Add(user);
            UserSession.Login(user.Id, user.Username, user.Role);

            return user;
        }

        public async Task<UserData> AddUserDataAsync(UserData data, DateTimeOffset? birthDate)
        {
            nutritionCalculationService.ApplyCalculations(data, birthDate);
            await userRepository.AddUserData(data);

            return data;
        }

        public async Task<UserData?> GetUserDataAsync(int userId)
        {
            return await userRepository.GetUserDataByUserId(userId);
        }

        public async Task UpdateUserDataAsync(UserData data)
        {
            nutritionCalculationService.ApplyCalculations(data);
            await userRepository.UpdateUserData(data);
        }
    }
}
