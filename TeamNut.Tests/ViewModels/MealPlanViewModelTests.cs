namespace TeamNut.Tests.ModelViews
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.ModelViews;
    using TeamNut.Services.Interfaces;
    using Xunit;

    [Collection("UsesStaticUserSession")]
    public class MealPlanViewModelTests : IDisposable
    {
        public void Dispose()
        {
            UserSession.Logout();
        }

        [Fact]
        public async Task LoadOrGenerateTodaysMealPlanAsync_WhenPlanExists_LoadsMealsAndCalculatesTotals()
        {
            var mealPlanService = Substitute.For<IMealPlanService>();
            var vm = new MealPlanViewModel(mealPlanService);
            var existingPlan = new MealPlan { Id = 15 };
            var meals = new List<Meal>
            {
                new Meal { Id = 1, Name = "Breakfast", Calories = 400, Protein = 30, Carbs = 40, Fat = 15 },
            };

            UserSession.Login(1, "TestUser", "User");

            mealPlanService.GetTodaysMealPlanAsync(Arg.Any<int>()).Returns(Task.FromResult(existingPlan));
            mealPlanService.GetMealsForMealPlanAsync(Arg.Any<int>()).Returns(Task.FromResult(new List<Meal>(meals)));
            mealPlanService.GetUserGoalAsync(Arg.Any<int>()).Returns(Task.FromResult("maintenance"));

            await vm.LoadOrGenerateTodaysMealPlanAsync();

            await mealPlanService.Received(1).GetTodaysMealPlanAsync(Arg.Any<int>());
            await mealPlanService.Received(1).GetMealsForMealPlanAsync(15);
            await mealPlanService.Received(1).GetUserGoalAsync(Arg.Any<int>());
            Assert.Equal(15, vm.CurrentMealPlanId);
            Assert.Single(vm.GeneratedMeals);
            Assert.Equal(400, vm.TotalCalories);
            Assert.Equal("Maintenance Goal", vm.GoalDescription);
        }

        [Fact]
        public async Task LoadOrGenerateTodaysMealPlanAsync_WhenPlanDoesNotExist_GeneratesNewPlan()
        {
            var mealPlanService = Substitute.For<IMealPlanService>();
            var vm = new MealPlanViewModel(mealPlanService);
            var generatedMeals = new List<Meal> { new Meal { Id = 5 } };

            UserSession.Login(1, "TestUser", "User");
            mealPlanService.GetTodaysMealPlanAsync(1).Returns(Task.FromResult<MealPlan>(null!));
            mealPlanService.GeneratePersonalizedMealPlanAsync(1).Returns(20);
            mealPlanService.GetMealsForMealPlanAsync(20).Returns(Task.FromResult(new List<Meal>(generatedMeals)));
            mealPlanService.GetUserGoalAsync(1).Returns(Task.FromResult("muscle"));

            await vm.LoadOrGenerateTodaysMealPlanAsync();

            Assert.True(vm.HasMeals);
            Assert.Equal(20, vm.CurrentMealPlanId);
            Assert.Equal("New meal plan generated for today!", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task ForceRegenerateMealPlanAsync_WhenUserLoggedIn_GeneratesNewPlan()
        {
            var mealPlanService = Substitute.For<IMealPlanService>();
            var vm = new MealPlanViewModel(mealPlanService);
            var generatedMeals = new List<Meal> { new Meal { Id = 8 } };

            UserSession.Login(1, "TestUser", "User");
            mealPlanService.GeneratePersonalizedMealPlanAsync(1).Returns(25);
            mealPlanService.GetMealsForMealPlanAsync(25).Returns(Task.FromResult(new List<Meal>(generatedMeals)));
            mealPlanService.GetUserGoalAsync(1).Returns(Task.FromResult("cut"));

            await vm.ForceRegenerateMealPlanAsync();

            Assert.True(vm.HasMeals);
            Assert.Equal(25, vm.CurrentMealPlanId);
            Assert.Single(vm.GeneratedMeals);

            UserSession.Logout();
        }

        [Fact]
        public async Task SaveToDailyLogAsync_WhenPlanIdIsInvalid_ShowsErrorDialog()
        {
            var mealPlanService = Substitute.For<IMealPlanService>();
            var vm = new MealPlanViewModel(mealPlanService);
            vm.CurrentMealPlanId = 0;

            await vm.SaveToDailyLogAsync();

            Assert.True(vm.ShowErrorDialog);
            Assert.Equal("No Meal Plan", vm.ErrorDialogTitle);
        }

        [Fact]
        public async Task SaveToDailyLogAsync_WhenPlanIdIsValid_CallsServiceAndUpdatesStatus()
        {
            var mealPlanService = Substitute.For<IMealPlanService>();
            var vm = new MealPlanViewModel(mealPlanService);
            vm.CurrentMealPlanId = 5;

            vm.GeneratedMeals.Add(new TeamNut.Views.MealPlanView.MealViewModel());
            vm.GeneratedMeals.Add(new TeamNut.Views.MealPlanView.MealViewModel());

            await vm.SaveToDailyLogAsync();

            await mealPlanService.Received(1).SaveMealsToDailyLogAsync(5);
            Assert.Equal("All 2 meals saved to daily log!", vm.StatusMessage);
        }
    }
}