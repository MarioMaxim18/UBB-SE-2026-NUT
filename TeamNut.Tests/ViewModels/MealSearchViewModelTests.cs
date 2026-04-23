// <copyright file="MealSearchViewModelTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿namespace TeamNut.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    [Collection("UsesStaticUserSession")]
    public class MealSearchViewModelTests
    {
        [Fact]
        public void SetAllMeals_UpdatesPaginationProperties()
        {
            var mealService = Substitute.For<IMealService>();
            var paginationService = Substitute.For<IPaginationService>();
            var vm = new MealSearchViewModel(mealService, paginationService);
            var meals = new List<Meal> { new Meal(), new Meal() };

            paginationService.GetTotalPages(2, 5).Returns(1);
            paginationService.GetPage(meals, 1, 5).Returns(meals);

            vm.SetAllMeals(meals);

            Assert.Equal(2, vm.Meals.Count);
            Assert.Equal("1 / 1", vm.PageText);
            Assert.False(vm.CanGoToNextPage);
            Assert.False(vm.CanGoToPreviousPage);
        }

        [Fact]
        public void GoToNextPage_WhenPossible_IncrementsPage()
        {
            var mealService = Substitute.For<IMealService>();
            var paginationService = Substitute.For<IPaginationService>();
            var vm = new MealSearchViewModel(mealService, paginationService);
            var meals = new List<Meal> { new Meal(), new Meal() };

            paginationService.GetTotalPages(2, 5).Returns(2);
            paginationService.GetPage(Arg.Any<List<Meal>>(), Arg.Any<int>(), Arg.Any<int>()).Returns(meals);
            vm.SetAllMeals(meals);

            vm.GoToNextPage();

            Assert.Equal("2 / 2", vm.PageText);
            Assert.True(vm.CanGoToPreviousPage);
        }

        [Fact]
        public void GoToPreviousPage_WhenPossible_DecrementsPage()
        {
            var mealService = Substitute.For<IMealService>();
            var paginationService = Substitute.For<IPaginationService>();
            var vm = new MealSearchViewModel(mealService, paginationService);
            var meals = new List<Meal> { new Meal(), new Meal() };

            paginationService.GetTotalPages(2, 5).Returns(2);
            paginationService.GetPage(Arg.Any<List<Meal>>(), Arg.Any<int>(), Arg.Any<int>()).Returns(meals);

            vm.SetAllMeals(meals);
            vm.GoToNextPage();

            vm.GoToPreviousPage();

            Assert.Equal("1 / 2", vm.PageText);
            Assert.False(vm.CanGoToPreviousPage);
        }

        [Fact]
        public async Task GetMealIngredientsTextAsync_WhenIngredientsExist_ReturnsJoinedString()
        {
            var mealService = Substitute.For<IMealService>();
            var paginationService = Substitute.For<IPaginationService>();
            var vm = new MealSearchViewModel(mealService, paginationService);
            var lines = new List<string> { "100g Chicken", "50g Rice" };

            mealService.GetMealIngredientLinesAsync(1).Returns(lines);

            var result = await vm.GetMealIngredientsTextAsync(1);

            Assert.Equal("100g Chicken\n50g Rice", result);
        }

        [Fact]
        public async Task GetMealIngredientsTextAsync_WhenNoIngredients_ReturnsNotFoundMessage()
        {
            var mealService = Substitute.For<IMealService>();
            var paginationService = Substitute.For<IPaginationService>();
            var vm = new MealSearchViewModel(mealService, paginationService);

            mealService.GetMealIngredientLinesAsync(1).Returns(new List<string>());

            var result = await vm.GetMealIngredientsTextAsync(1);

            Assert.Equal("No ingredients found.", result);
        }
    }
}
