namespace TeamNut.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    [Collection("UsesStaticUserSession")]
    public class InventoryViewModelTests
    {
        [Fact]
        public void IngredientSearchText_Setter_UpdatesFilteredIngredients()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new InventoryViewModel(inventoryService, filteringService);
            var ingredient = new Ingredient { Name = "Apple" };

            filteringService.FilterIngredients(Arg.Any<IEnumerable<Ingredient>>(), "Apple").Returns(new List<Ingredient> { ingredient });

            vm.IngredientSearchText = "Apple";

            Assert.Contains(ingredient, vm.FilteredIngredients);
        }

        [Fact]
        public async Task LoadInventoryAsync_WhenSuccessful_PopulatesItems()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var inventoryList = new List<Inventory> { new Inventory { Id = 1 } };

            inventoryService.GetUserInventory(Arg.Any<int>()).Returns(inventoryList);
            var vm = new InventoryViewModel(inventoryService, filteringService);

            await vm.LoadInventoryAsync();

            Assert.Single(vm.Items);
            Assert.Equal(1, vm.Items[0].Id);
        }

        [Fact]
        public async Task RemoveItemAsync_WhenSuccessful_RemovesItemFromList()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new InventoryViewModel(inventoryService, filteringService);
            var item = new Inventory { Id = 1 };
            vm.Items.Add(item);

            await vm.RemoveItemCommand.ExecuteAsync(item);

            await inventoryService.Received(1).RemoveItem(1);
            Assert.Empty(vm.Items);
        }

        [Fact]
        public async Task AddNewIngredientAsync_WhenIngredientIsNull_SetsStatusMessage()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new InventoryViewModel(inventoryService, filteringService);

            vm.SelectedIngredient = null;

            await vm.AddNewIngredientCommand.ExecuteAsync(null);

            Assert.Equal("Please choose an ingredient from suggestions.", vm.StatusMessage);
        }

        [Fact]
        public async Task AddNewIngredientAsync_WhenQuantityIsInvalid_SetsStatusMessage()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var vm = new InventoryViewModel(inventoryService, filteringService);

            vm.SelectedIngredient = new Ingredient { FoodId = 1 };
            vm.QuantityToAdd = 0;

            await vm.AddNewIngredientCommand.ExecuteAsync(null);

            Assert.Equal("Quantity must be greater than 0.", vm.StatusMessage);
        }

        [Fact]
        public async Task AddNewIngredientAsync_WhenSuccessful_AddsToPantryAndSetsMessage()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();

            filteringService.FilterIngredients(Arg.Any<IEnumerable<Ingredient>>(), Arg.Any<string>())
                            .Returns(new List<Ingredient>());
            inventoryService.GetUserInventory(Arg.Any<int>()).Returns(new List<Inventory>());

            UserSession.Login(99, "Test", "User");
            var vm = new InventoryViewModel(inventoryService, filteringService);

            vm.SelectedIngredient = new Ingredient { FoodId = 5, Name = "Apple" };
            vm.QuantityToAdd = 200;

            await vm.AddNewIngredientCommand.ExecuteAsync(null);

            await inventoryService.Received(1).AddToPantry(99, 5, 200);
            Assert.Equal("Added 200g of Apple.", vm.StatusMessage);
            Assert.Null(vm.SelectedIngredient);
            Assert.Equal(string.Empty, vm.IngredientSearchText);

            UserSession.Logout();
        }

        [Fact]
        public async Task LoadIngredientsAsync_WhenSuccessful_PopulatesAvailableIngredients()
        {
            var inventoryService = Substitute.For<IInventoryService>();
            var filteringService = Substitute.For<IFilteringService>();
            var ingredients = new List<Ingredient> { new Ingredient { FoodId = 1, Name = "Banana" } };

            inventoryService.GetAllIngredients().Returns(ingredients);
            var vm = new InventoryViewModel(inventoryService, filteringService);

            await vm.LoadIngredientsAsync();

            Assert.Single(vm.AvailableIngredients);
            Assert.Equal("Banana", vm.AvailableIngredients[0].Name);
        }
    }
}
