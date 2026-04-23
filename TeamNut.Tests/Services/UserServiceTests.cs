using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IUserRepository mockUserRepo;
        private readonly INutritionCalculationService mockNutritionService;
        private readonly UserService service;

        public UserServiceTests()
        {
            mockUserRepo = Substitute.For<IUserRepository>();
            mockNutritionService = Substitute.For<INutritionCalculationService>();
            service = new UserService(mockUserRepo, mockNutritionService);
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_WithExistingUsername_ReturnsTrue()
        {
            var users = new List<User>
            {
                new User { Username = "JohnDoe" },
                new User { Username = "JaneDoe" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("JohnDoe");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_WithNonExistingUsername_ReturnsFalse()
        {
            var users = new List<User>
            {
                new User { Username = "JohnDoe" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("Alice");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_IsCaseInsensitive_ReturnsTrue()
        {
            var users = new List<User>
            {
                new User { Username = "JohnDoe" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("johndoe");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsUser()
        {
            var expectedUser = new User
            {
                Id = 1,
                Username = "JohnDoe",
                Password = "Password123",
                Role = "User"
            };
            mockUserRepo.GetByUsernameAndPassword("JohnDoe", "Password123")
                .Returns(expectedUser);

            var result = await service.LoginAsync("JohnDoe", "Password123");

            result.Should().NotBeNull();
            result.Username.Should().Be("JohnDoe");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ReturnsNull()
        {
            mockUserRepo.GetByUsernameAndPassword("JohnDoe", "WrongPassword")
                .Returns((User?)null);

            var result = await service.LoginAsync("JohnDoe", "WrongPassword");

            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUserAsync_WithNewUsername_ReturnsUser()
        {
            var newUser = new User
            {
                Id = 1,
                Username = "NewUser",
                Password = "Password123",
                Role = "User"
            };
            mockUserRepo.GetAll().Returns(new List<User>());

            var result = await service.RegisterUserAsync(newUser);

            result.Should().NotBeNull();
            result.Username.Should().Be("NewUser");
            await mockUserRepo.Received(1).Add(Arg.Any<User>());
        }

        [Fact]
        public async Task RegisterUserAsync_WithExistingUsername_ReturnsNull()
        {
            var existingUsers = new List<User>
            {
                new User { Username = "ExistingUser" }
            };
            mockUserRepo.GetAll().Returns(existingUsers);

            var newUser = new User { Username = "ExistingUser" };

            var result = await service.RegisterUserAsync(newUser);

            result.Should().BeNull();
            await mockUserRepo.DidNotReceive().Add(Arg.Any<User>());
        }

        [Fact]
        public async Task AddUserDataAsync_CallsApplyCalculations()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "bulk"
            };
            var birthDate = new System.DateTimeOffset(new System.DateTime(1998, 1, 1));

            await service.AddUserDataAsync(userData, birthDate);

            mockNutritionService.Received(1).ApplyCalculations(userData, birthDate);
            await mockUserRepo.Received(1).AddUserData(userData);
        }

        [Fact]
        public async Task UpdateUserDataAsync_CallsApplyCalculations()
        {
            var userData = new UserData
            {
                Weight = 80,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "cut"
            };

            await service.UpdateUserDataAsync(userData);

            mockNutritionService.Received(1).ApplyCalculations(userData, null);
            await mockUserRepo.Received(1).UpdateUserData(userData);
        }

        [Fact]
        public async Task GetUserDataAsync_ReturnsUserData()
        {
            var expectedData = new UserData
            {
                UserId = 1,
                Weight = 75,
                Height = 180
            };
            mockUserRepo.GetUserDataByUserId(1).Returns(expectedData);

            var result = await service.GetUserDataAsync(1);

            result.Should().NotBeNull();
            result.Weight.Should().Be(75);
        }
    }
}