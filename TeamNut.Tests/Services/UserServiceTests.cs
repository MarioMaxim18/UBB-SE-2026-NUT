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
                new User { Username = "PopescuIon" },
                new User { Username = "MarinescuAna" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("PopescuIon");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_WithNonExistingUsername_ReturnsFalse()
        {
            var users = new List<User>
            {
                new User { Username = "PopescuIon" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("GheorghescuMaria");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_IsCaseInsensitive_ReturnsTrue()
        {
            var users = new List<User>
            {
                new User { Username = "PopescuIon" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("popescuion");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsUser()
        {
            var expectedUser = new User
            {
                Id = 1,
                Username = "PopescuIon",
                Password = "Parola123",
                Role = "User"
            };
            mockUserRepo.GetByUsernameAndPassword("PopescuIon", "Parola123")
                .Returns(expectedUser);

            var result = await service.LoginAsync("PopescuIon", "Parola123");

            result.Should().NotBeNull();
            result.Username.Should().Be("PopescuIon");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ReturnsNull()
        {
            mockUserRepo.GetByUsernameAndPassword("PopescuIon", "ParolaGresita")
                .Returns((User?)null);

            var result = await service.LoginAsync("PopescuIon", "ParolaGresita");

            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUserAsync_WithNewUsername_ReturnsUser()
        {
            var newUser = new User
            {
                Id = 1,
                Username = "GigelNoul",
                Password = "Parola123",
                Role = "User"
            };
            mockUserRepo.GetAll().Returns(new List<User>());

            var result = await service.RegisterUserAsync(newUser);

            result.Should().NotBeNull();
            result.Username.Should().Be("GigelNoul");
            await mockUserRepo.Received(1).Add(Arg.Any<User>());
        }

        [Fact]
        public async Task RegisterUserAsync_WithExistingUsername_ReturnsNull()
        {
            var existingUsers = new List<User>
            {
                new User { Username = "DorelExistent" }
            };
            mockUserRepo.GetAll().Returns(existingUsers);

            var newUser = new User { Username = "DorelExistent" };

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

        [Fact]
        public async Task CheckIfUsernameExistsAsync_WithNullUsernameInDb_DoesNotCrash()
        {
            var users = new List<User>
            {
                new User { Username = null! },
                new User { Username = "PopescuIon" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync("PopescuIon");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckIfUsernameExistsAsync_WithNullQuery_ReturnsFalse()
        {
            var users = new List<User>
            {
                new User { Username = "PopescuIon" }
            };
            mockUserRepo.GetAll().Returns(users);

            var result = await service.CheckIfUsernameExistsAsync(null!);

            result.Should().BeFalse();
        }
    }
}
