namespace TeamNut.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services;
    using TeamNut.Views.MealPlanView;
    using Xunit;

    public class InventoryServiceTests
    {
        private readonly IIngredientRepository ingredientRepo;
        private readonly IInventoryRepository inventoryRepo;
        private readonly IMealPlanRepository mealPlanRepo;
        private readonly InventoryService service;

        public InventoryServiceTests()
        {
            ingredientRepo = Substitute.For<IIngredientRepository>();
            inventoryRepo = Substitute.For<IInventoryRepository>();
            mealPlanRepo = Substitute.For<IMealPlanRepository>();
            service = new InventoryService(ingredientRepo, inventoryRepo, mealPlanRepo);
        }

        [Fact]
        public async Task ConsumeMeal_WhenNotEnoughStock_ReturnsFalse()
        {
            var required = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };
            var inventory = new List<Inventory>
            {
                new Inventory { IngredientId = 5, QuantityGrams = 20 }
            };
            mealPlanRepo.GetIngredientsForMeal(1).Returns(required);
            inventoryRepo.GetAllByUserId(1).Returns(inventory);

            var result = await service.ConsumeMeal(1, 1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ConsumeMeal_WhenExactAmount_DeletesInventoryRow()
        {
            var required = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };
            var inventory = new List<Inventory>
            {
                new Inventory { Id = 10, IngredientId = 5, QuantityGrams = 50 }
            };
            mealPlanRepo.GetIngredientsForMeal(1).Returns(required);
            inventoryRepo.GetAllByUserId(1).Returns(inventory);

            var result = await service.ConsumeMeal(1, 1);

            result.Should().BeTrue();
            await inventoryRepo.Received(1).Delete(10);
        }

        [Fact]
        public async Task ConsumeMeal_WhenExcessStock_UpdatesQuantity()
        {
            var required = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 5, Quantity = 50 }
            };
            var inventory = new List<Inventory>
            {
                new Inventory { Id = 10, IngredientId = 5, QuantityGrams = 120 }
            };
            mealPlanRepo.GetIngredientsForMeal(1).Returns(required);
            inventoryRepo.GetAllByUserId(1).Returns(inventory);

            var result = await service.ConsumeMeal(1, 1);

            result.Should().BeTrue();
            await inventoryRepo.Received(1).Update(Arg.Is<Inventory>(i => i.QuantityGrams == 70));
        }

        [Fact]
        public async Task ConsumeMeal_NoIngredients_ReturnsTrueWithoutDbCalls()
        {
            mealPlanRepo.GetIngredientsForMeal(1).Returns(new List<IngredientViewModel>());
            inventoryRepo.GetAllByUserId(1).Returns(new List<Inventory>());

            var result = await service.ConsumeMeal(1, 1);

            result.Should().BeTrue();
            await inventoryRepo.DidNotReceive().Delete(Arg.Any<int>());
            await inventoryRepo.DidNotReceive().Update(Arg.Any<Inventory>());
        }

        [Fact]
        public async Task ConsumeMeal_TwoIngredients_OneInsufficient_Fails()
        {
            var required = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 1, Quantity = 100 },
                new IngredientViewModel { IngredientId = 2, Quantity = 50 }
            };
            var inventory = new List<Inventory>
            {
                new Inventory { Id = 1, IngredientId = 1, QuantityGrams = 200 },
                new Inventory { Id = 2, IngredientId = 2, QuantityGrams = 30 }
            };
            mealPlanRepo.GetIngredientsForMeal(7).Returns(required);
            inventoryRepo.GetAllByUserId(3).Returns(inventory);

            (await service.ConsumeMeal(3, 7)).Should().BeFalse();
        }

        [Fact]
        public async Task ConsumeMeal_IngredientNotInPantry_ReturnsFalse()
        {
            var required = new List<IngredientViewModel>
            {
                new IngredientViewModel { IngredientId = 99, Quantity = 50 }
            };
            inventoryRepo.GetAllByUserId(1).Returns(new List<Inventory>());
            mealPlanRepo.GetIngredientsForMeal(1).Returns(required);

            (await service.ConsumeMeal(1, 1)).Should().BeFalse();
        }

        [Fact]
        public async Task AddToPantry_CreatesInventoryWithCorrectFields()
        {
            await service.AddToPantry(1, 5, 250);

            await inventoryRepo.Received(1).Add(
                Arg.Is<Inventory>(i => i.UserId == 1 && i.IngredientId == 5 && i.QuantityGrams == 250));
        }

        [Fact]
        public async Task GetUserInventory_ReturnsItemsFromRepo()
        {
            var items = new List<Inventory>
            {
                new Inventory { Id = 1, IngredientId = 10, QuantityGrams = 500 }
            };
            inventoryRepo.GetAllByUserId(1).Returns(items);

            var result = await service.GetUserInventory(1);

            result.Should().HaveCount(1);
            result.First().QuantityGrams.Should().Be(500);
        }

        [Fact]
        public async Task GetAllIngredients_ReturnsList()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient { FoodId = 1, Name = "Ceapa" },
                new Ingredient { FoodId = 2, Name = "Usturoi" }
            };
            ingredientRepo.GetAllAsync().Returns(ingredients);

            var result = await service.GetAllIngredients();

            result.Should().HaveCount(2);
        }
    }
}
