using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using System;

namespace TeamNut.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        private readonly int _currentUserId;

        private bool _isBusy;
        private string _emptyListMessage = "Your pantry is empty. Start adding items!";
        private string _statusMessage = string.Empty;
        private string _ingredientSearchText = string.Empty;
        private Ingredient? _selectedIngredient;
        private double _quantityToAdd = 100;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string EmptyListMessage
        {
            get => _emptyListMessage;
            set => SetProperty(ref _emptyListMessage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string IngredientSearchText
        {
            get => _ingredientSearchText;
            set
            {
                if (SetProperty(ref _ingredientSearchText, value))
                {
                    UpdateFilteredIngredients();
                }
            }
        }

        public Ingredient? SelectedIngredient
        {
            get => _selectedIngredient;
            set => SetProperty(ref _selectedIngredient, value);
        }

        public double QuantityToAdd
        {
            get => _quantityToAdd;
            set => SetProperty(ref _quantityToAdd, value);
        }

        
        public ObservableCollection<Inventory> Items { get; } = new();
        public ObservableCollection<Ingredient> AvailableIngredients { get; } = new();
        public ObservableCollection<Ingredient> FilteredIngredients { get; } = new();

        public InventoryViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _currentUserId = Models.UserSession.UserId ?? 0;

            _ = LoadInventoryAsync();
            _ = LoadIngredientsAsync();
        }

       
        [RelayCommand]
        public async Task LoadInventoryAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var inventoryItems = await _inventoryService.GetUserInventory(_currentUserId);

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
            if (item == null) return;

            try
            {
                await _inventoryService.RemoveItem(item.Id);
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
                await _inventoryService.AddToPantry(_currentUserId, SelectedIngredient.FoodId, qty);
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
                var ingredients = await _inventoryService.GetAllIngredients();
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
