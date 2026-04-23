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

    public class ShoppingListServiceTests
    {
        [Fact]
        public async Task AddItemAsync_WhenItemExists_CallsUpdate()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();

            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            var existingItem = new ShoppingItem
            {
                Id = 1,
                UserId = 1,
                IngredientId = 5,
                QuantityGrams = 10
            };

            ingredientFakeRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>()).Returns(5);
            shoppingListFakeRepo.GetByUserAndIngredient(1, 5).Returns(existingItem);

            var result = await service.AddItemAsync("Tomato", 1, 5);

            Assert.NotNull(result);
            Assert.Equal(15, result.QuantityGrams);

            await shoppingListFakeRepo.Received(1).Update(existingItem);
        }

        [Fact]
        public async Task AddItemAsync_WhenItemDoesNotExist_CallsAdd()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();

            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            ingredientFakeRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>()).Returns(5);
            shoppingListFakeRepo.GetByUserAndIngredient(1, 5).Returns((ShoppingItem?)null);

            var result = await service.AddItemAsync("Tomato", 1, 5);

            Assert.NotNull(result);
            Assert.Equal(5, result.QuantityGrams);
            Assert.Equal("Tomato", result.IngredientName);
            Assert.False(result.IsChecked);

            await shoppingListFakeRepo.Received(1).Add(Arg.Any<ShoppingItem>());
        }

        [Fact]
        public async Task AddItemAsync_WhenExceptionOccurs_ReturnsNull()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();

            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            ingredientFakeRepo.GetOrCreateIngredientIdAsync(Arg.Any<string>())
                .Returns(Task.FromException<int>(new System.Exception()));

            var result = await service.AddItemAsync("Tomato", 1, 5);

            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveItemAsync_WhenSuccessful_ReturnsTrue()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1 };

            var result = await service.RemoveItemAsync(item);

            Assert.True(result);
            await shoppingListFakeRepo.Received(1).Delete(1);
        }

        [Fact]
        public async Task RemoveItemAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1 };

            shoppingListFakeRepo.Delete(1).Returns(Task.FromException(new System.Exception()));

            var result = await service.RemoveItemAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenSuccessful_ReturnsTrue()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1 };

            var result = await service.UpdateItemAsync(item);

            Assert.True(result);
            await shoppingListFakeRepo.Received(1).Update(item);
        }

        [Fact]
        public async Task UpdateItemAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1 };

            shoppingListFakeRepo.Update(item).Returns(Task.FromException(new System.Exception()));

            var result = await service.UpdateItemAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task MoveToPantryAsync_WhenSuccessful_ReturnsTrue()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1, UserId = 1, IngredientId = 5, QuantityGrams = 200 };

            var result = await service.MoveToPantryAsync(item);

            Assert.True(result);
            await inventoryFakeRepo.Received(1).Add(Arg.Is<Inventory>(i => i.QuantityGrams == 200));
            await shoppingListFakeRepo.Received(1).Delete(1);
        }

        [Fact]
        public async Task MoveToPantryAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);
            var item = new ShoppingItem { Id = 1 };

            inventoryFakeRepo.Add(Arg.Any<Inventory>()).Returns(Task.FromException(new System.Exception()));

            var result = await service.MoveToPantryAsync(item);

            Assert.False(result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenNoIngredientsNeeded_ReturnsZero()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            shoppingListFakeRepo.GetIngredientsNeededFromMealPlan(1).Returns(new List<ShoppingItem>());

            var result = await service.GenerateListAsync(1);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenIngredientsNeeded_ReturnsAddedCount()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            var needed = new List<ShoppingItem>
            {
                new ShoppingItem { IngredientId = 5, IngredientName = "Apple", QuantityGrams = 100 }
            };

            shoppingListFakeRepo.GetIngredientsNeededFromMealPlan(1).Returns(needed);
            inventoryFakeRepo.GetAllByUserId(1).Returns(new List<Inventory>());
            shoppingListFakeRepo.GetAllByUserId(1).Returns(new List<ShoppingItem>());

            ingredientFakeRepo.GetOrCreateIngredientIdAsync("Apple").Returns(5);
            shoppingListFakeRepo.GetByUserAndIngredient(1, 5).Returns((ShoppingItem?)null);

            var result = await service.GenerateListAsync(1);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GenerateListAsync_WhenExceptionOccurs_ReturnsMinusOne()
        {
            var shoppingListFakeRepo = Substitute.For<IShoppingListRepository>();
            var ingredientFakeRepo = Substitute.For<IIngredientRepository>();
            var inventoryFakeRepo = Substitute.For<IInventoryRepository>();
            var service = new ShoppingListService(shoppingListFakeRepo, ingredientFakeRepo, inventoryFakeRepo);

            shoppingListFakeRepo.GetIngredientsNeededFromMealPlan(1)
                .Returns(Task.FromException<List<ShoppingItem>>(new System.Exception()));

            var result = await service.GenerateListAsync(1);

            Assert.Equal(-1, result);
        }
    }
}
