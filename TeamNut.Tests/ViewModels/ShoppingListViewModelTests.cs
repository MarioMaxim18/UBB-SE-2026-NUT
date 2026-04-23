namespace TeamNut.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    public class ShoppingListViewModelTests
    {
        public ShoppingListViewModelTests()
        {
            UserSession.Logout();
        }

        [Fact]
        public async Task AddItemCommand_WhenServiceReturnsNull_SetsErrorStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);

            UserSession.Login(1, "TestUser", "User");
            shoppingListService.AddItemAsync("Apple", 1, 100).Returns((ShoppingItem?)null);

            await vm.AddItemCommand.ExecuteAsync("Apple");

            Assert.True(vm.IsError);
            Assert.Equal("Database error: Could not add item.", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task AddItemCommand_WhenItemIsNew_AddsToCollectionAndSetsStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var newItem = new ShoppingItem { Id = 1, IngredientName = "Apple" };

            UserSession.Login(1, "TestUser", "User");
            shoppingListService.AddItemAsync("Apple", 1, 100).Returns(newItem);

            await vm.AddItemCommand.ExecuteAsync("Apple");

            Assert.Single(vm.Items);
            Assert.Equal(newItem, vm.Items[0]);
            Assert.False(vm.IsError);
            Assert.Equal("Updated 'Apple' successfully!", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task AddItemCommand_WhenItemExists_UpdatesQuantityAndSetsStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var existingItem = new ShoppingItem { Id = 1, IngredientName = "Apple", QuantityGrams = 50 };
            var returnedItem = new ShoppingItem { Id = 1, IngredientName = "Apple", QuantityGrams = 150 };

            vm.Items.Add(existingItem);

            UserSession.Login(1, "TestUser", "User");
            shoppingListService.AddItemAsync("Apple", 1, 100).Returns(returnedItem);

            await vm.AddItemCommand.ExecuteAsync("Apple");

            Assert.Single(vm.Items);
            Assert.Equal(150, vm.Items[0].QuantityGrams);
            Assert.False(vm.IsError);
            Assert.Equal("Updated 'Apple' successfully!", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task MoveToPantryCommand_WhenSuccessful_RemovesItemAndSetsStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var item = new ShoppingItem { Id = 1, IngredientName = "Apple" };
            vm.Items.Add(item);

            shoppingListService.MoveToPantryAsync(item).Returns(true);

            await vm.MoveToPantryCommand.ExecuteAsync(item);

            Assert.Empty(vm.Items);
            Assert.False(vm.IsError);
            Assert.Equal("Moved 'Apple' to Pantry.", vm.StatusMessage);
        }

        [Fact]
        public async Task MoveToPantryCommand_WhenFails_SetsErrorStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var item = new ShoppingItem { Id = 1, IngredientName = "Apple" };
            vm.Items.Add(item);

            shoppingListService.MoveToPantryAsync(item).Returns(false);

            await vm.MoveToPantryCommand.ExecuteAsync(item);

            Assert.Single(vm.Items);
            Assert.True(vm.IsError);
            Assert.Equal("Failed to move item to Pantry.", vm.StatusMessage);
        }

        [Fact]
        public async Task RemoveItemCommand_WhenSuccessful_RemovesItemAndSetsStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var item = new ShoppingItem { Id = 1, IngredientName = "Apple" };
            vm.Items.Add(item);

            shoppingListService.RemoveItemAsync(item).Returns(true);

            await vm.RemoveItemCommand.ExecuteAsync(item);

            Assert.Empty(vm.Items);
            Assert.False(vm.IsError);
            Assert.Equal("Item removed from list.", vm.StatusMessage);
        }

        [Fact]
        public async Task RemoveItemCommand_WhenFails_SetsErrorStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);
            var item = new ShoppingItem { Id = 1, IngredientName = "Apple" };
            vm.Items.Add(item);

            shoppingListService.RemoveItemAsync(item).Returns(false);

            await vm.RemoveItemCommand.ExecuteAsync(item);

            Assert.Single(vm.Items);
            Assert.True(vm.IsError);
            Assert.Equal("Failed to delete item from database.", vm.StatusMessage);
        }

        [Fact]
        public async Task GenerateListCommand_WhenItemsAdded_SetsSuccessStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);

            UserSession.Login(1, "TestUser", "User");
            shoppingListService.GenerateListAsync(1).Returns(5);
            shoppingListService.GetShoppingItemsAsync(1).Returns(new List<ShoppingItem>());

            await vm.GenerateListCommand.ExecuteAsync(null);

            Assert.False(vm.IsError);
            Assert.Equal("Successfully generated 5 new items from your Meal Plan!", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task GenerateListCommand_WhenZeroItemsAdded_SetsAlreadyCompleteStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);

            UserSession.Login(1, "TestUser", "User");
            shoppingListService.GenerateListAsync(1).Returns(0);

            await vm.GenerateListCommand.ExecuteAsync(null);

            Assert.False(vm.IsError);
            Assert.Equal("You already have everything you need", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task GenerateListCommand_WhenErrorOccurs_SetsErrorStatus()
        {
            var shoppingListService = Substitute.For<IShoppingListService>();
            var vm = new ShoppingListViewModel(shoppingListService);

            UserSession.Login(1, "TestUser", "User");

            shoppingListService.GenerateListAsync(Arg.Any<int>()).Returns(Task.FromResult(-1));

            await vm.GenerateListCommand.ExecuteAsync(null);

            Assert.True(vm.IsError);
            Assert.Equal("Error analyzing Meal Plan for ingredients.", vm.StatusMessage);

            UserSession.Logout();
        }
    }
}