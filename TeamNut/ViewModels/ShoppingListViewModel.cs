using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{

    public partial class ShoppingListViewModel : ObservableObject
    {
        private readonly ShoppingListService _shoppingListService;

        [ObservableProperty]
        private ObservableCollection<ShoppingItem> items = new ObservableCollection<ShoppingItem>();

        [ObservableProperty]
        private string statusMessage;

        [ObservableProperty]
        private bool isStatusVisible;

        [ObservableProperty]
        private bool isError;

        [ObservableProperty]
        private double pendingQuantity = 100; // Default to 100g

        public ShoppingListViewModel()
        {
            _shoppingListService = new ShoppingListService();
            _ = LoadItemsAsync();
        }

        public async Task LoadItemsAsync()
        {
            if (UserSession.UserId == null) return;

            var loadedItems = await _shoppingListService.GetShoppingItemsAsync(UserSession.UserId.Value);
            
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
            if (!string.IsNullOrWhiteSpace(itemName) && UserSession.UserId != null)
            {
                var addedItem = await _shoppingListService.AddItemAsync(itemName.Trim(), UserSession.UserId.Value, PendingQuantity);
                if (addedItem != null)
                {
                    var existing = System.Linq.Enumerable.FirstOrDefault(Items, i => i.Id == addedItem.Id);
                    
                    if (existing == null)
                    {
                        addedItem.PropertyChanged += async (s, e) =>
                        {
                            if (e.PropertyName == nameof(ShoppingItem.IsChecked))
                            {
                                bool updated = await _shoppingListService.UpdateItemAsync((ShoppingItem)s);
                                if (!updated) ShowStatus("Failed to save checkmark state.", true);
                            }
                        };
                        Items.Add(addedItem);
                    }
                    else
                    {
                        existing.QuantityGrams = addedItem.QuantityGrams;
                    }

                    ShowStatus($"Updated '{itemName}' successfully!", false);
                    PendingQuantity = 100; // Reset to default
                }
                else
                {
                    ShowStatus("Database error: Could not add item.", true);
                }
            }
        }

        [RelayCommand]
        public async Task MoveToPantry(ShoppingItem item)
        {
            if (item != null && Items.Contains(item))
            {
                bool success = await _shoppingListService.MoveToPantryAsync(item);
                if (success)
                {
                    Items.Remove(item);
                    ShowStatus($"Moved '{item.IngredientName}' to Pantry.", false);
                }
                else
                {
                    ShowStatus("Failed to move item to Pantry.", true);
                }
            }
        }

        [RelayCommand]
        public async Task RemoveItem(ShoppingItem item)
        {
            if (item != null && Items.Contains(item))
            {
                bool success = await _shoppingListService.RemoveItemAsync(item);
                if (success)
                {
                    Items.Remove(item);
                    ShowStatus("Item removed from list.", false);
                }
                else
                {
                    ShowStatus("Failed to delete item from database.", true);
                }
            }
        }

        [RelayCommand]
        public async Task GenerateList()
        {
            if (UserSession.UserId != null)
            {
                int itemsAdded = await _shoppingListService.GenerateListAsync(UserSession.UserId.Value);
                if (itemsAdded > 0)
                {
                    await LoadItemsAsync();
                    ShowStatus($"Successfully generated {itemsAdded} new items from your Meal Plan!", false);
                }
                else if (itemsAdded == 0)
                {
                    ShowStatus("you already have everything you need", false);
                }
                else
                {
                    ShowStatus("Error analyzing Meal Plan for ingredients.", true);
                }
            }
        }

        public async Task<System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>> SearchIngredientsAsync(string query)
        {
            return await _shoppingListService.SearchIngredientsAsync(query);
        }

        private void ShowStatus(string message, bool error)
        {
            StatusMessage = message;
            IsError = error;
            IsStatusVisible = true;
            
            Task.Delay(3000).ContinueWith(_ =>
            {
                IsStatusVisible = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
