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

        /// <summary>Gets or sets a value indicating whether a background operation is running.</summary>
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        /// <summary>Gets or sets the message displayed when the inventory list is empty.</summary>
        public string EmptyListMessage
        {
            get => emptyListMessage;
            set => SetProperty(ref emptyListMessage, value);
        }

        /// <summary>Gets or sets the status message shown to the user.</summary>
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        /// <summary>Gets or sets the ingredient search text for autocomplete filtering.</summary>
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

        /// <summary>Gets or sets the ingredient chosen by the user from suggestions.</summary>
        public Ingredient? SelectedIngredient
        {
            get => selectedIngredient;
            set => SetProperty(ref selectedIngredient, value);
        }

        /// <summary>Gets or sets the quantity in grams to add to inventory.</summary>
        public double QuantityToAdd
        {
            get => quantityToAdd;
            set => SetProperty(ref quantityToAdd, value);
        }

        /// <summary>Gets the current inventory items.</summary>
        public ObservableCollection<Inventory> Items { get; } = new ObservableCollection<Inventory>();

        /// <summary>Gets all available ingredients.</summary>
        public ObservableCollection<Ingredient> AvailableIngredients { get; } = new ObservableCollection<Ingredient>();

        /// <summary>Gets the filtered ingredients matching the search text.</summary>
        public ObservableCollection<Ingredient> FilteredIngredients { get; } = new ObservableCollection<Ingredient>();

        /// <summary>Initializes a new instance of the <see cref="InventoryViewModel"/> class.</summary>
        /// <param name="userId">The identifier of the current user.</param>
        public InventoryViewModel(int userId)
        {
            inventoryService = new InventoryService();
            currentUserId = userId;
            _ = LoadInventoryAsync();
            _ = LoadIngredientsAsync();
        }

        /// <summary>Loads inventory items for the current user.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>Loads all available ingredients from the database.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        /// <summary>Gets a value indicating whether the inventory list is empty.</summary>
        public bool IsListEmpty => !Items.Any();
    }
}
