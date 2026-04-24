using System;
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
    public class DailyLogServiceTests
    {
        private readonly IDailyLogRepository mockDailyLogRepo;
        private readonly IUserRepository mockUserRepo;
        private readonly IMealService mockMealService;
        private readonly DailyLogService service;

        public DailyLogServiceTests()
        {
            mockDailyLogRepo = Substitute.For<IDailyLogRepository>();
            mockUserRepo = Substitute.For<IUserRepository>();
            mockMealService = Substitute.For<IMealService>();
            service = new DailyLogService(mockDailyLogRepo, mockUserRepo, mockMealService);
        }

        [Fact]
        public async Task SearchMealsAsync_WithSearchTerm_CallsMealService()
        {
            var expectedMeals = new List<Meal>
            {
                new Meal { Name = "Salata de pui", Calories = 500 }
            };
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(expectedMeals);

            var result = await service.SearchMealsAsync("pui");

            result.Should().HaveCount(1);
            await mockMealService.Received(1).GetFilteredMealsAsync(
                Arg.Is<MealFilter>(f => f.SearchTerm == "pui"));
        }

        [Fact]
        public async Task SearchMealsAsync_WithNull_UsesEmptyString()
        {
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(new List<Meal>());

            await service.SearchMealsAsync(null!);

            await mockMealService.Received(1).GetFilteredMealsAsync(
                Arg.Is<MealFilter>(f => f.SearchTerm == string.Empty));
        }

        [Fact]
        public async Task GetMealsForAutocompleteAsync_ReturnsAllMeals()
        {
            var meals = new List<Meal>
            {
                new Meal { Name = "Ciorba de legume" },
                new Meal { Name = "Paste carbonara" }
            };
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(meals);

            var result = await service.GetMealsForAutocompleteAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task LogMealAsync_WithValidMeal_CallsRepoWithCorrectData()
        {
            UserSession.Login(1, "MarcelCroitoru", "User");
            var meal = new Meal { Id = 7, Name = "Ciorba de burta", Calories = 350 };

            await service.LogMealAsync(meal);

            await mockDailyLogRepo.Received(1).Add(
                Arg.Is<DailyLog>(d => d.UserId == 1 && d.MealId == 7 && d.Calories == 350));
            UserSession.Logout();
        }

        [Fact]
        public async Task LogMealAsync_WhenNotLoggedIn_Throws()
        {
            UserSession.Logout();
            var meal = new Meal { Id = 1, Name = "Sarmale", Calories = 400 };

            Func<Task> act = async () => await service.LogMealAsync(meal);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetTodayTotalsAsync_ReturnsCaloriesFromRepo()
        {
            UserSession.Login(5, "CostelBoboc", "User");
            var expected = new DailyLog { Calories = 1200 };
            mockDailyLogRepo.GetNutritionTotalsForRange(5, Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns(expected);

            var result = await service.GetTodayTotalsAsync();

            result.Calories.Should().Be(1200);
            UserSession.Logout();
        }

        [Fact]
        public async Task HasAnyLogsAsync_WhenLogsExist_ReturnsTrue()
        {
            UserSession.Login(1, "MarcelCroitoru", "User");
            mockDailyLogRepo.HasAnyLogs(1).Returns(true);

            var result = await service.HasAnyLogsAsync();

            result.Should().BeTrue();
            UserSession.Logout();
        }

        [Fact]
        public async Task HasAnyLogsAsync_WhenEmpty_ReturnsFalse()
        {
            UserSession.Login(1, "MarcelCroitoru", "User");
            mockDailyLogRepo.HasAnyLogs(1).Returns(false);

            (await service.HasAnyLogsAsync()).Should().BeFalse();
            UserSession.Logout();
        }

        [Fact]
        public async Task SearchMealsAsync_NoResults_ReturnsEmpty()
        {
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(new List<Meal>());

            var result = await service.SearchMealsAsync("ceva inexistent");

            result.Should().BeEmpty();
        }
    }
}
