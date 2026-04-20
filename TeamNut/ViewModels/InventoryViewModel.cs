using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly InventoryService inventoryService;
        private readonly int currentUserId;

        private bool isBusy;
        private string emptyListMessage = "Your pantry is empty. Start adding items!";
        private string statusMessage = string.Empty;
        private string ingredientSearchText = string.Empty;
        private Ingredient? selectedIngredient;
        private double quantityToAdd = 100;

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        public string EmptyListMessage
        {
            get => emptyListMessage;
            set => SetProperty(ref emptyListMessage, value);
        }

        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        public string IngredientSearchText
        {
            get => ingredientSearchText;
            set
            {
                if (SetProperty(ref ingredientSearchText, value))
                {
                    UpdateFilteredIngredients();
                }
            }
        }

        public Ingredient? SelectedIngredient
        {
            get => selectedIngredient;
            set => SetProperty(ref selectedIngredient, value);
        }

        public double QuantityToAdd
        {
            get => quantityToAdd;
            set => SetProperty(ref quantityToAdd, value);
        }

        public ObservableCollection<Inventory> Items { get; } = new ObservableCollection<Inventory>();

        public ObservableCollection<Ingredient> AvailableIngredients { get; } = new ObservableCollection<Ingredient>();

        public ObservableCollection<Ingredient> FilteredIngredients { get; } = new ObservableCollection<Ingredient>();

        public InventoryViewModel(int userId)
        {
            inventoryService = new InventoryService();
            currentUserId = userId;

            _ = LoadInventoryAsync();
            _ = LoadIngredientsAsync();
        }

        [RelayCommand]
        public async Task LoadInventoryAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var inventoryItems = await inventoryService.GetUserInventory(currentUserId);

                Items.Clear();
                foreach (var item in inventoryItems)
                {
                    Items.Add(item);
                }

                OnPropertyChanged(nameof(IsListEmpty));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading inventory: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RemoveItemAsync(Inventory item)
        {
            if (item == null)
            {
                return;
            }

            try
            {
                await inventoryService.RemoveItem(item.Id);
                Items.Remove(item);
                OnPropertyChanged(nameof(IsListEmpty));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not delete item: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task AddNewIngredientAsync()
        {
            if (SelectedIngredient == null)
            {
                StatusMessage = "Please choose an ingredient from suggestions.";
                return;
            }

            if (QuantityToAdd <= 0)
            {
                StatusMessage = "Quantity must be greater than 0.";
                return;
            }

            try
            {
                int qty = (int)Math.Round(QuantityToAdd);
                await inventoryService.AddToPantry(currentUserId, SelectedIngredient.FoodId, qty);
                await LoadInventoryAsync();
                StatusMessage = $"Added {qty}g of {SelectedIngredient.Name}.";
                IngredientSearchText = string.Empty;
                SelectedIngredient = null;
                UpdateFilteredIngredients();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not add item: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task LoadIngredientsAsync()
        {
            try
            {
                var ingredients = await inventoryService.GetAllIngredients();
                AvailableIngredients.Clear();
                foreach (var ingredient in ingredients)
                {
                    AvailableIngredients.Add(ingredient);
                }

                UpdateFilteredIngredients();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading ingredients: {ex.Message}";
            }
        }

        private void UpdateFilteredIngredients()
        {
            FilteredIngredients.Clear();

            var query = IngredientSearchText?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(query)
                ? AvailableIngredients
                : new ObservableCollection<Ingredient>(AvailableIngredients.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));

            foreach (var ingredient in filtered)
            {
                FilteredIngredients.Add(ingredient);
            }
        }

        public bool IsListEmpty => !Items.Any();
    }
}
