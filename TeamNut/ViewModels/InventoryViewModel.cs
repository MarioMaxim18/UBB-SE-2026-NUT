using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;
using System;

namespace TeamNut.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly InventoryService _inventoryService;
        private readonly int _currentUserId;
        private const double DefaultQuantityToAdd = 100;
        private const double MinQuantityAllowed = 0;
        private const string EmptyInventoryMessage = "Your pantry is empty. Start adding items!";
        private const string SelectIngredientMessage = "Please choose an ingredient from suggestions.";
        private const string InvalidQuantityMessage = "Quantity must be greater than 0.";
        private const string LoadInventoryErrorMessage = "Error loading inventory: {0}";
        private const string LoadIngredientsErrorMessage = "Error loading ingredients: {0}";
        private const string DeleteItemErrorMessage = "Could not delete item: {0}";
        private const string AddItemErrorMessage = "Could not add item: {0}";
        private const string AddItemSuccessMessage = "Added {0}g of {1}.";
        private const StringComparison IngredientSearchComparison = StringComparison.OrdinalIgnoreCase;
        private bool _isBusy;
        private string _emptyListMessage = EmptyInventoryMessage;
        private string _statusMessage = string.Empty;
        private string _ingredientSearchText = string.Empty;
        private Ingredient? _selectedIngredient;
        private double _quantityToAdd = DefaultQuantityToAdd;
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
        public InventoryViewModel(int userId)
        {
            _inventoryService = new InventoryService();
            _currentUserId = userId;
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
                StatusMessage = string.Format(LoadInventoryErrorMessage, ex.Message);
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
                StatusMessage = string.Format(DeleteItemErrorMessage, ex.Message);
            }
        }

        [RelayCommand]
        private async Task AddNewIngredientAsync()
        {
            if (SelectedIngredient == null)
            {
                StatusMessage = SelectIngredientMessage;
                return;
            }

            if (QuantityToAdd <= MinQuantityAllowed)
            {
                StatusMessage = InvalidQuantityMessage;
                return;
            }

            try
            {
                int qty = (int)Math.Round(QuantityToAdd);
                await _inventoryService.AddToPantry(
                    _currentUserId,
                    SelectedIngredient.FoodId,
                    qty
                );

                await LoadInventoryAsync();

                StatusMessage = string.Format(
                    AddItemSuccessMessage,
                    qty,
                    SelectedIngredient.Name
                );

                IngredientSearchText = string.Empty;
                SelectedIngredient = null;
                UpdateFilteredIngredients();
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(AddItemErrorMessage, ex.Message);
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
                StatusMessage = string.Format(LoadIngredientsErrorMessage, ex.Message);
            }
        }

        private void UpdateFilteredIngredients()
        {
            FilteredIngredients.Clear();

            var query = IngredientSearchText?.Trim() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(query)
                ? AvailableIngredients
                : AvailableIngredients.Where(i =>
                    i.Name.Contains(query, IngredientSearchComparison));

            foreach (var ingredient in filtered)
            {
                FilteredIngredients.Add(ingredient);
            }
        }

        public bool IsListEmpty => !Items.Any();
    }
}