// <copyright file="ShoppingListViewModel.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// ShoppingListViewModel.
    /// </summary>
    public partial class ShoppingListViewModel : ObservableObject
    {
        private readonly IShoppingListService shoppingListService;

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
        public partial ObservableCollection<ShoppingItem> Items { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsStatusVisible { get; set; }

        [ObservableProperty]
        public partial bool IsError { get; set; }

        [ObservableProperty]
        public partial double PendingQuantity { get; set; }

        public ShoppingListViewModel(IShoppingListService sshoppingListService)
        {
            this.Items = new ObservableCollection<ShoppingItem>();
            this.StatusMessage = string.Empty;
            this.PendingQuantity = DefaultPendingQuantity;
            this.shoppingListService = sshoppingListService;
            _ = this.LoadItemsAsync();
        }

        public async Task LoadItemsAsync()
        {
            if (UserSession.UserId == null)
            {
                return;
            }

            var loadedItems =
                await this.shoppingListService.GetShoppingItemsAsync(
                    UserSession.UserId.Value);

            this.Items.Clear();

            foreach (var item in loadedItems)
            {
                item.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(ShoppingItem.IsChecked) && s is ShoppingItem si)
                    {
                        await this.shoppingListService.UpdateItemAsync(si);
                    }
                };

                this.Items.Add(item);
            }
        }

        [RelayCommand]
        public async Task AddItem(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName) || UserSession.UserId == null)
            {
                return;
            }

            var addedItem = await this.shoppingListService.AddItemAsync(
                itemName.Trim(),
                UserSession.UserId.Value,
                this.PendingQuantity);

            if (addedItem == null)
            {
                this.ShowStatus(ErrorAddItem, true);
                return;
            }

            var existing = this.Items.FirstOrDefault(i => i.Id == addedItem.Id);

            if (existing == null)
            {
                addedItem.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(ShoppingItem.IsChecked) && s is ShoppingItem si)
                    {
                        bool updated = await this.shoppingListService.UpdateItemAsync(si);
                        if (!updated)
                        {
                            this.ShowStatus(ErrorUpdateChecked, true);
                        }
                    }
                };

                this.Items.Add(addedItem);
            }
            else
            {
                existing.QuantityGrams = addedItem.QuantityGrams;
            }

            this.ShowStatus(
                string.Format(StatusAddSuccessFormat, itemName),
                false);

            this.PendingQuantity = DefaultPendingQuantity;
        }

        [RelayCommand]
        public async Task MoveToPantry(ShoppingItem item)
        {
            if (item == null || !this.Items.Contains(item))
            {
                return;
            }

            bool success = await this.shoppingListService.MoveToPantryAsync(item);

            if (success)
            {
                this.Items.Remove(item);
                this.ShowStatus(
                    string.Format(StatusMoveToPantryFormat, item.IngredientName),
                    false);
            }
            else
            {
                this.ShowStatus(ErrorMoveToPantry, true);
            }
        }

        [RelayCommand]
        public async Task RemoveItem(ShoppingItem item)
        {
            if (item == null || !this.Items.Contains(item))
            {
                return;
            }

            bool success = await this.shoppingListService.RemoveItemAsync(item);

            if (success)
            {
                this.Items.Remove(item);
                this.ShowStatus(StatusItemRemoved, false);
            }
            else
            {
                this.ShowStatus(ErrorDeleteItem, true);
            }
        }

        [RelayCommand]
        public async Task GenerateList()
        {
            if (UserSession.UserId == null)
            {
                return;
            }

            int itemsAdded =
                await this.shoppingListService.GenerateListAsync(
                    UserSession.UserId.Value);

            if (itemsAdded > 0)
            {
                await this.LoadItemsAsync();
                this.ShowStatus(
                    string.Format(StatusGenerateSuccessFormat, itemsAdded),
                    false);
            }
            else if (itemsAdded == 0)
            {
                this.ShowStatus(StatusAlreadyComplete, false);
            }
            else
            {
                this.ShowStatus(ErrorGenerateList, true);
            }
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string query)
        {
            return await this.shoppingListService.SearchIngredientsAsync(query);
        }

        private void ShowStatus(string message, bool error)
        {
            this.StatusMessage = message;
            this.IsError = error;
            this.IsStatusVisible = true;

            Task.Delay(StatusDisplayDurationMs).ContinueWith(_ =>
            {
                this.IsStatusVisible = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
