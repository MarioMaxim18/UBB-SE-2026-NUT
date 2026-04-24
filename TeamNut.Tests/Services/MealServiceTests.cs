namespace TeamNut.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services;
    using Xunit;

    public class MealServiceTests
    {
        private readonly IMealRepository fakeRepo;
        private readonly MealService service;

        public MealServiceTests()
        {
            fakeRepo = Substitute.For<IMealRepository>();
            service = new MealService(fakeRepo);
        }

        [Fact]
        public async Task GetMealsAsync_WhenFilterIsNull_CallsGetAll()
        {
            await service.GetMealsAsync(null!);

            await fakeRepo.Received(1).GetAll();
            await fakeRepo.DidNotReceiveWithAnyArgs().GetFilteredMeals(Arg.Any<MealFilter>());
        }

        [Fact]
        public async Task GetMealsAsync_WhenFilterIsProvided_CallsGetFilteredMeals()
        {
            var filter = new MealFilter { SearchTerm = "Chicken" };

            await service.GetMealsAsync(filter);

            await fakeRepo.Received(1).GetFilteredMeals(filter);
            await fakeRepo.DidNotReceive().GetAll();
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenMealIsNull_DoesNothing()
        {
            await service.ToggleFavoriteAsync(null!);

            await fakeRepo.DidNotReceiveWithAnyArgs().SetFavoriteAsync(default, default, default);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenUserIdIsNull_DoesNothing()
        {
            var validMeal = new Meal { Id = 1, IsFavorite = true };

            UserSession.Logout();

            await service.ToggleFavoriteAsync(validMeal);

            await fakeRepo.DidNotReceiveWithAnyArgs().SetFavoriteAsync(default, default, default);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenValid_CallsSetFavoriteAsync()
        {
            var validMeal = new Meal { Id = 5, IsFavorite = true };

            UserSession.Login(99, "MarcelCroitoru", "User");

            await service.ToggleFavoriteAsync(validMeal);

            await fakeRepo.Received(1).SetFavoriteAsync(99, 5, true);

            UserSession.Logout();
        }
    }
}
