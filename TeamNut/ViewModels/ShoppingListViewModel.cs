using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using System.Linq;

namespace TeamNut.ViewModels
{
    public partial class ShoppingListViewModel : ObservableObject
    {
        private readonly ShoppingListService _shoppingListService;
        private const double DefaultPendingQuantity = 100;
        private const int StatusDisplayDurationMs = 3000;
        private const string StatusAddSuccessFormat = "Updated '{0}' successfully!";
        private const string StatusMoveToPantryFormat = "Moved '{0}' to Pantry.";
        private const string StatusItemRemoved = "Item removed from list.";
        private const string StatusAlreadyComplete = "You already have everything you need";
        private const string StatusGenerateSuccessFormat = "Successfully generated {0} new items from your Meal Plan!";
        private const string ErrorAddItem = "Database error: Could not add item.";
        private const string ErrorUpdateChecked = "Failed to save checkmark state.";
        private const string ErrorMoveToPantry = "Failed to move item to Pantry.";
        private const string ErrorDeleteItem = "Failed to delete item from database.";
        private const string ErrorGenerateList = "Error analyzing Meal Plan for ingredients.";
        [ObservableProperty]
        private ObservableCollection<ShoppingItem> items = new();
        [ObservableProperty]
        private string statusMessage;
        [ObservableProperty]
        private bool isStatusVisible;
        [ObservableProperty]
        private bool isError;
        [ObservableProperty]
        private double pendingQuantity = DefaultPendingQuantity;
        public ShoppingListViewModel()
        {
            _shoppingListService = new ShoppingListService();
            _ = LoadItemsAsync();
        }

        public async Task LoadItemsAsync()
        {
            if (UserSession.UserId == null)
                return;

            var loadedItems =
                await _shoppingListService.GetShoppingItemsAsync(
                    UserSession.UserId.Value);

            Items.Clear();

            foreach (var item in loadedItems)
            {
                item.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(ShoppingItem.IsChecked))
                    {
                        await _shoppingListService.UpdateItemAsync((ShoppingItem)s);
                    }
                };

                Items.Add(item);
            }
        }

        [RelayCommand]
        public async Task AddItem(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || UserSession.UserId == null)
                return;

            var addedItem = await _shoppingListService.AddItemAsync(
                itemName.Trim(),
                UserSession.UserId.Value,
                PendingQuantity);

            if (addedItem == null)
            {
                ShowStatus(ErrorAddItem, true);
                return;
            }

            var existing = Items.FirstOrDefault(i => i.Id == addedItem.Id);

            if (existing == null)
            {
                addedItem.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(ShoppingItem.IsChecked))
                    {
                        bool updated = await _shoppingListService.UpdateItemAsync((ShoppingItem)s);
                        if (!updated)
                            ShowStatus(ErrorUpdateChecked, true);
                    }
                };

                Items.Add(addedItem);
            }
            else
            {
                existing.QuantityGrams = addedItem.QuantityGrams;
            }

            ShowStatus(
                string.Format(StatusAddSuccessFormat, itemName),
                false);

            PendingQuantity = DefaultPendingQuantity;
        }

        [RelayCommand]
        public async Task MoveToPantry(ShoppingItem item)
        {
            if (item == null || !Items.Contains(item))
                return;

            bool success = await _shoppingListService.MoveToPantryAsync(item);

            if (success)
            {
                Items.Remove(item);
                ShowStatus(
                    string.Format(StatusMoveToPantryFormat, item.IngredientName),
                    false);
            }
            else
            {
                ShowStatus(ErrorMoveToPantry, true);
            }
        }

        [RelayCommand]
        public async Task RemoveItem(ShoppingItem item)
        {
            if (item == null || !Items.Contains(item))
                return;

            bool success = await _shoppingListService.RemoveItemAsync(item);

            if (success)
            {
                Items.Remove(item);
                ShowStatus(StatusItemRemoved, false);
            }
            else
            {
                ShowStatus(ErrorDeleteItem, true);
            }
        }

        [RelayCommand]
        public async Task GenerateList()
        {
            if (UserSession.UserId == null)
                return;

            int itemsAdded =
                await _shoppingListService.GenerateListAsync(
                    UserSession.UserId.Value);

            if (itemsAdded > 0)
            {
                await LoadItemsAsync();
                ShowStatus(
                    string.Format(StatusGenerateSuccessFormat, itemsAdded),
                    false);
            }
            else if (itemsAdded == 0)
            {
                ShowStatus(StatusAlreadyComplete, false);
            }
            else
            {
                ShowStatus(ErrorGenerateList, true);
            }
        }

        public async Task<
            System.Collections.Generic.List<
                System.Collections.Generic.KeyValuePair<int, string>>>
            SearchIngredientsAsync(string query)
        {
            return await _shoppingListService.SearchIngredientsAsync(query);
        }

        private void ShowStatus(string message, bool error)
        {
            StatusMessage = message;
            IsError = error;
            IsStatusVisible = true;

            Task.Delay(StatusDisplayDurationMs).ContinueWith(_ =>
            {
                IsStatusVisible = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}