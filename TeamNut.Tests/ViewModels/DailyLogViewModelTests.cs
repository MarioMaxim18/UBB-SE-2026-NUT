namespace TeamNut.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    public class DailyLogViewModelTests
    {
        [Fact]
        public void MealSearchText_Setter_UpdatesFilteredMeals()
        {
            var dailyLogService = Substitute.For<IDailyLogService>();
            var formattingService = Substitute.For<IFormattingService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new DailyLogViewModel(dailyLogService, formattingService, filteringService);

            var meal = new Meal { Name = "Apple" };
            filteringService.FilterMeals(Arg.Any<IEnumerable<Meal>>(), "Apple").Returns(new List<Meal> { meal });

            vm.MealSearchText = "Apple";

            Assert.Contains(meal, vm.FilteredMeals);
        }

        [Fact]
        public async Task LogSelectedMealAsync_WhenSelectedMealIsNull_SetsStatusMessage()
        {
            var dailyLogService = Substitute.For<IDailyLogService>();
            var formattingService = Substitute.For<IFormattingService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new DailyLogViewModel(dailyLogService, formattingService, filteringService);

            vm.SelectedMeal = null;

            await vm.LogSelectedMealAsync();

            Assert.Equal("Select a meal first.", vm.LogMealStatusMessage);
        }

        [Fact]
        public async Task LogSelectedMealAsync_WhenSelectedMealIsValid_LogsMealAndClearsSelection()
        {
            var dailyLogService = Substitute.For<IDailyLogService>();
            var formattingService = Substitute.For<IFormattingService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new DailyLogViewModel(dailyLogService, formattingService, filteringService);

            var meal = new Meal { Name = "Pizza" };
            vm.SelectedMeal = meal;
            dailyLogService.HasAnyLogsAsync().Returns(false);

            await vm.LogSelectedMealAsync();

            await dailyLogService.Received(1).LogMealAsync(meal);
            Assert.Equal("Logged Pizza.", vm.LogMealStatusMessage);
            Assert.Null(vm.SelectedMeal);
            Assert.Equal(string.Empty, vm.MealSearchText);
        }

        [Fact]
        public async Task LoadAsync_WhenNoLogs_SetsHasDataToFalse()
        {
            var dailyLogService = Substitute.For<IDailyLogService>();
            var formattingService = Substitute.For<IFormattingService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new DailyLogViewModel(dailyLogService, formattingService, filteringService);

            dailyLogService.HasAnyLogsAsync().Returns(false);

            await vm.LoadAsync();

            Assert.False(vm.HasData);
            Assert.Equal("You need to have had atleast one consumed meal.", vm.StatusMessage);
        }

        [Fact]
        public async Task LoadAsync_WhenHasLogs_UpdatesPropertiesAndFormatting()
        {
            var dailyLogService = Substitute.For<IDailyLogService>();
            var formattingService = Substitute.For<IFormattingService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new DailyLogViewModel(dailyLogService, formattingService, filteringService);

            var userData = new UserData { CalorieNeeds = 2500, ProteinNeeds = 180, CarbNeeds = 300, FatNeeds = 80 };
            var dailyTotals = new DailyLog { Calories = 1000 };
            var weeklyTotals = new DailyLog { Calories = 5000 };

            dailyLogService.HasAnyLogsAsync().Returns(true);
            dailyLogService.GetCurrentUserNutritionTargetsAsync().Returns(userData);
            dailyLogService.GetTodayTotalsAsync().Returns(dailyTotals);
            dailyLogService.GetCurrentWeekTotalsAsync().Returns(weeklyTotals);

            formattingService.FormatMetricWithGoal(1000, 2500, "kcal").Returns("1000 / 2500 kcal");

            await vm.LoadAsync();

            Assert.True(vm.HasData);
            Assert.Equal(string.Empty, vm.StatusMessage);
            Assert.Equal(2500, vm.DailyCaloriesGoal);
            Assert.Equal(180, vm.DailyProteinGoal);
            Assert.Equal(dailyTotals, vm.DailyTotals);
            Assert.Equal(weeklyTotals, vm.WeeklyTotals);
            Assert.Equal("1000 / 2500 kcal", vm.DailyCaloriesText);
        }
    }
}
