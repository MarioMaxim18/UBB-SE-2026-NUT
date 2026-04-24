namespace TeamNut.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;
    using TeamNut.Services;
    using Xunit;

    public class ShoppingListServiceTests
    {
        private readonly IShoppingListRepository shoppingListRepo;
        private readonly IIngredientRepository ingredientRepo;
        private readonly IInventoryRepository inventoryRepo;
        private readonly ShoppingListService service;

        public ShoppingListServiceTests()
        {
            shoppingListRepo = Substitute.For<IShoppingListRepository>();
            ingredientRepo = Substitute.For<IIngredientRepository>();
            inventoryRepo = Substitute.For<IInventoryRepository>();
            service = new ShoppingListService(shoppingListRepo, ingredientRepo, inventoryRepo);
        }

        [Fact]
        public async Task AddItemAsync_WhenItemExists_CallsUpdate()
        {
            var existingItem = new ShoppingItem
            {
                Id = 1,
                UserId = 1,
                IngredientId = 5,
                QuantityGrams = 10
            };

            ingredientRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>()).Returns(5);
            shoppingListRepo.GetByUserAndIngredient(1, 5).Returns(existingItem);

            var result = await service.AddItemAsync("Rosie", 1, 5);

            Assert.NotNull(result);
            Assert.Equal(15, result.QuantityGrams);
            await shoppingListRepo.Received(1).Update(existingItem);
        }

        [Fact]
        public async Task AddItemAsync_WhenItemDoesNotExist_CallsAdd()
        {
            ingredientRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>()).Returns(5);
            shoppingListRepo.GetByUserAndIngredient(1, 5).Returns((ShoppingItem?)null);

            var result = await service.AddItemAsync("Rosie", 1, 5);

            Assert.NotNull(result);
            Assert.Equal(5, result.QuantityGrams);
            Assert.Equal("Rosie", result.IngredientName);
            Assert.False(result.IsChecked);
            await shoppingListRepo.Received(1).Add(Arg.Any<ShoppingItem>());
        }

        [Fact]
        public async Task AddItemAsync_WhenExceptionOccurs_ReturnsNull()
        {
            ingredientRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>())
                .Returns(Task.FromException<int>(new System.Exception()));

            var result = await service.AddItemAsync("Rosie", 1, 5);

            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveItemAsync_WhenSuccessful_ReturnsTrue()
        {
            var item = new ShoppingItem { Id = 1 };

            var result = await service.RemoveItemAsync(item);

            Assert.True(result);
            await shoppingListRepo.Received(1).Delete(1);
        }

        [Fact]
        public async Task RemoveItemAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var item = new ShoppingItem { Id = 1 };
            shoppingListRepo.Delete(1).Returns(Task.FromException(new System.Exception()));

            var result = await service.RemoveItemAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenSuccessful_ReturnsTrue()
        {
            var item = new ShoppingItem { Id = 1 };

            var result = await service.UpdateItemAsync(item);

            Assert.True(result);
            await shoppingListRepo.Received(1).Update(item);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var item = new ShoppingItem { Id = 1 };
            shoppingListRepo.Update(item).Returns(Task.FromException(new System.Exception()));

            var result = await service.UpdateItemAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task MoveToPantryAsync_WhenSuccessful_ReturnsTrue()
        {
            var item = new ShoppingItem { Id = 1, UserId = 1, IngredientId = 5, QuantityGrams = 200 };

            var result = await service.MoveToPantryAsync(item);

            Assert.True(result);
            await inventoryRepo.Received(1).Add(Arg.Is<Inventory>(i => i.QuantityGrams == 200));
            await shoppingListRepo.Received(1).Delete(1);
        }

        [Fact]
        public async Task MoveToPantryAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var item = new ShoppingItem { Id = 1 };
            inventoryRepo.Add(Arg.Any<Inventory>()).Returns(Task.FromException(new System.Exception()));

            var result = await service.MoveToPantryAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenNoIngredientsNeeded_ReturnsZero()
        {
            shoppingListRepo.GetIngredientsNeededFromMealPlan(1).Returns(new List<ShoppingItem>());

            var result = await service.GenerateListAsync(1);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenIngredientsNeeded_ReturnsAddedCount()
        {
            var needed = new List<ShoppingItem>
            {
                new ShoppingItem { IngredientId = 5, IngredientName = "Mar", QuantityGrams = 100 }
            };

            shoppingListRepo.GetIngredientsNeededFromMealPlan(1).Returns(needed);
            inventoryRepo.GetAllByUserId(1).Returns(new List<Inventory>());
            shoppingListRepo.GetAllByUserId(1).Returns(new List<ShoppingItem>());

            ingredientRepo.GetOrCreateIngredientIdAsync("Mar").Returns(5);
            shoppingListRepo.GetByUserAndIngredient(1, 5).Returns((ShoppingItem?)null);

            var result = await service.GenerateListAsync(1);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenExceptionOccurs_ReturnsMinusOne()
        {
            shoppingListRepo.GetIngredientsNeededFromMealPlan(1)
                .Returns(Task.FromException<List<ShoppingItem>>(new System.Exception()));

            var result = await service.GenerateListAsync(1);

            Assert.Equal(-1, result);
        }
    }
}
