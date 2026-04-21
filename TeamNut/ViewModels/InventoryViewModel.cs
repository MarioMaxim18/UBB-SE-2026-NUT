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
    /// <summary>View model for managing the user's food ingredient inventory.</summary>
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly InventoryService inventoryService;
        private readonly int currentUserId;
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
        private bool isBusy;
        private string emptyListMessage = EmptyInventoryMessage;
        private string statusMessage = string.Empty;
        private string ingredientSearchText = string.Empty;
        private Ingredient? selectedIngredient;
        private double quantityToAdd = DefaultQuantityToAdd;

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
                await inventoryService.AddToPantry(
                    currentUserId,
                    SelectedIngredient.FoodId,
                    qty);

                await LoadInventoryAsync();

                StatusMessage = string.Format(
                    AddItemSuccessMessage,
                    qty,
                    SelectedIngredient.Name);

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
