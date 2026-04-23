namespace TeamNut.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services;
    using Windows.Networking.NetworkOperators;
    using Xunit;

    public class MealServiceTests
    {
        [Fact]
        public async Task GetMealsAsync_WhenFilterIsNull_CallsGetAll()
        {
            var fakeRepo = Substitute.For<IMealRepository>();
            var service = new MealService(fakeRepo);

            await service.GetMealsAsync(null!);

            await fakeRepo.Received(1).GetAll();
            await fakeRepo.DidNotReceiveWithAnyArgs().GetFilteredMeals(default);
        }

        [Fact]
        public async Task GetMealsAsync_WhenFilterIsProvided_CallsGetFilteredMeals()
        {
            var fakeRepo = Substitute.For<IMealRepository>();
            var service = new MealService(fakeRepo);
            var filter = new MealFilter { SearchTerm = "Chicken" };

            await service.GetMealsAsync(filter);

            await fakeRepo.Received(1).GetFilteredMeals(filter);
            await fakeRepo.DidNotReceive().GetAll();
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenMealIsNull_DoesNothing()
        {
            var fakeRepo = Substitute.For<IMealRepository>();
            var service = new MealService(fakeRepo);

            await service.ToggleFavoriteAsync(null!);

            await fakeRepo.DidNotReceiveWithAnyArgs().SetFavoriteAsync(default, default, default);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenUserIdIsNull_DoesNothing()
        {
            var fakeRepo = Substitute.For<IMealRepository>();
            var service = new MealService(fakeRepo);
            var validMeal = new Meal { Id = 1, IsFavorite = true };

            UserSession.Logout();

            await service.ToggleFavoriteAsync(validMeal);

            await fakeRepo.DidNotReceiveWithAnyArgs().SetFavoriteAsync(default, default, default);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenValid_CallsSetFavoriteAsync()
        {
            var fakeRepo = Substitute.For<IMealRepository>();
            var service = new MealService(fakeRepo);
            var validMeal = new Meal { Id = 5, IsFavorite = true };

            UserSession.Login(99, "TestUser", "User");

            await service.ToggleFavoriteAsync(validMeal);

            await fakeRepo.Received(1).SetFavoriteAsync(99, 5, true);

            UserSession.Logout();
        }
    }
}