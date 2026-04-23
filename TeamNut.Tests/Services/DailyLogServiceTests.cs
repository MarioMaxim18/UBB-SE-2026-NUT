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
                new Meal { Name = "Chicken Salad", Calories = 500 }
            };
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(expectedMeals);

            var result = await service.SearchMealsAsync("Chicken");

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            await mockMealService.Received(1).GetFilteredMealsAsync(
                Arg.Is<MealFilter>(f => f.SearchTerm == "Chicken"));
        }

        [Fact]
        public async Task SearchMealsAsync_WithNullSearchTerm_UsesEmptyString()
        {
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(new List<Meal>());

            await service.SearchMealsAsync(null);

            await mockMealService.Received(1).GetFilteredMealsAsync(
                Arg.Is<MealFilter>(f => f.SearchTerm == string.Empty));
        }

        [Fact]
        public async Task GetMealsForAutocompleteAsync_CallsMealServiceWithEmptyFilter()
        {
            var expectedMeals = new List<Meal>
            {
                new Meal { Name = "Meal 1" },
                new Meal { Name = "Meal 2" }
            };
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(expectedMeals);

            var result = await service.GetMealsForAutocompleteAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            await mockMealService.Received(1).GetFilteredMealsAsync(Arg.Any<MealFilter>());
        }

        [Fact]
        public async Task LogMealAsync_WithNullMeal_ThrowsArgumentNullException()
        {
            Func<Task> act = async () => await service.LogMealAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("meal");
        }

        [Fact]
        public async Task SearchMealsAsync_WithEmptyResult_ReturnsEmptyList()
        {
            mockMealService.GetFilteredMealsAsync(Arg.Any<MealFilter>())
                .Returns(new List<Meal>());

            var result = await service.SearchMealsAsync("NonExistent");

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}