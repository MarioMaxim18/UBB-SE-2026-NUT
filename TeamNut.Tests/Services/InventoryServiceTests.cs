// <copyright file="InventoryServiceTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿namespace TeamNut.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services;
    using TeamNut.Views.MealPlanView;
    using Xunit;

    public class InventoryServiceTests
    {
        [Fact]
        public async Task ConsumeMeal_WhenStockIsInsufficient_ReturnsFalse()
        {
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var mealPlanFakeRepo = Substitute.For<IMealPlanRepository>();
            var service = new InventoryService(ingredientFakeRepo, inventoryFakeRepo, mealPlanFakeRepo);

            var requiredIngredients = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };

            var inventoryItems = new List<Inventory>
            {
                new Inventory { IngredientId = 5, QuantityGrams = 20 }
            };

            mealPlanFakeRepo.GetIngredientsForMeal(1).Returns(requiredIngredients);
            inventoryFakeRepo.GetAllByUserId(1).Returns(inventoryItems);

            var result = await service.ConsumeMeal(1, 1);

            Assert.False(result);
        }

        [Fact]
        public async Task ConsumeMeal_WhenStockIsDepleted_CallsDeleteAndReturnsTrue()
        {
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var mealPlanFakeRepo = Substitute.For<IMealPlanRepository>();
            var service = new InventoryService(ingredientFakeRepo, inventoryFakeRepo, mealPlanFakeRepo);

            var requiredIngredients = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };

            var inventoryItems = new List<Inventory>
            {
                new Inventory { Id = 10, IngredientId = 5, QuantityGrams = 50 }
            };

            mealPlanFakeRepo.GetIngredientsForMeal(1).Returns(requiredIngredients);
            inventoryFakeRepo.GetAllByUserId(1).Returns(inventoryItems);

            var result = await service.ConsumeMeal(1, 1);

            Assert.True(result);
            await inventoryFakeRepo.Received(1).Delete(10);
        }

        [Fact]
        public async Task ConsumeMeal_WhenStockRemains_CallsUpdateAndReturnsTrue()
        {
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var mealPlanFakeRepo = Substitute.For<IMealPlanRepository>();
            var service = new InventoryService(ingredientFakeRepo, inventoryFakeRepo, mealPlanFakeRepo);

            var requiredIngredients = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };

            var inventoryItems = new List<Inventory>
            {
                new Inventory { Id = 10, IngredientId = 5, QuantityGrams = 120 }
            };

            mealPlanFakeRepo.GetIngredientsForMeal(1).Returns(requiredIngredients);
            inventoryFakeRepo.GetAllByUserId(1).Returns(inventoryItems);

            var result = await service.ConsumeMeal(1, 1);

            Assert.True(result);
            await inventoryFakeRepo.Received(1).Update(Arg.Is<Inventory>(i => i.QuantityGrams == 70));
        }
    }
}
