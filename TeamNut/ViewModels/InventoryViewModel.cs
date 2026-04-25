namespace TeamNut.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// InventoryViewModel.
    /// </summary>
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly IInventoryService inventoryService;

        private readonly IFilteringService filteringService;

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

        private bool isBusy;

        private string emptyListMessage = EmptyInventoryMessage;

        private string statusMessage = string.Empty;

        private string ingredientSearchText = string.Empty;

        private Ingredient? selectedIngredient;

        private double quantityToAdd = DefaultQuantityToAdd;

        public bool IsBusy
        {
            get => this.isBusy;
            set => SetProperty(ref this.isBusy, value);
        }

        public string EmptyListMessage
        {
            get => this.emptyListMessage;
            set => SetProperty(ref this.emptyListMessage, value);
        }

        public string StatusMessage
        {
            get => this.statusMessage;
            set => SetProperty(ref this.statusMessage, value);
        }

        public string IngredientSearchText
        {
            get => this.ingredientSearchText;
            set
            {
                if (SetProperty(ref this.ingredientSearchText, value))
                {
                    this.UpdateFilteredIngredients();
                }
            }
        }

        public Ingredient? SelectedIngredient
        {
            get => this.selectedIngredient;
            set => SetProperty(ref this.selectedIngredient, value);
        }

        public double QuantityToAdd
        {
            get => this.quantityToAdd;
            set => SetProperty(ref this.quantityToAdd, value);
        }

        public ObservableCollection<Inventory> Items { get; } = new ObservableCollection<Inventory>();

        public ObservableCollection<Ingredient> AvailableIngredients { get; } = new ObservableCollection<Ingredient>();

        public ObservableCollection<Ingredient> FilteredIngredients { get; } = new ObservableCollection<Ingredient>();

        public InventoryViewModel(
            IInventoryService inventoryService,
            IFilteringService filteringService)
        {
            this.inventoryService = inventoryService;
            this.filteringService = filteringService;
            this.currentUserId = Models.UserSession.UserId ?? 0;

            _ = this.LoadInventoryAsync();
            _ = this.LoadIngredientsAsync();
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
                var inventoryItems = await this.inventoryService.GetUserInventory(this.currentUserId);

                this.Items.Clear();
                foreach (var item in inventoryItems)
                {
                    this.Items.Add(item);
                }

                OnPropertyChanged(nameof(this.IsListEmpty));
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
                await this.inventoryService.RemoveItem(item.Id);
                this.Items.Remove(item);
                OnPropertyChanged(nameof(this.IsListEmpty));
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
                int roundedQuantity = (int)Math.Round(QuantityToAdd);
                await this.inventoryService.AddToPantry(
                    this.currentUserId,
                    SelectedIngredient.FoodId,
                    roundedQuantity);

                await this.LoadInventoryAsync();

                StatusMessage = string.Format(
                    AddItemSuccessMessage,
                    roundedQuantity,
                    SelectedIngredient.Name);

                IngredientSearchText = string.Empty;
                SelectedIngredient = null;
                this.UpdateFilteredIngredients();
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
                var ingredients = await this.inventoryService.GetAllIngredients();
                this.AvailableIngredients.Clear();

                foreach (var ingredient in ingredients)
                {
                    this.AvailableIngredients.Add(ingredient);
                }

                this.UpdateFilteredIngredients();
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(LoadIngredientsErrorMessage, ex.Message);
            }
        }

        private void UpdateFilteredIngredients()
        {
            this.FilteredIngredients.Clear();

            var query = IngredientSearchText?.Trim() ?? string.Empty;

            var filtered = this.filteringService.FilterIngredients(this.AvailableIngredients, query);

            foreach (var ingredient in filtered)
            {
                this.FilteredIngredients.Add(ingredient);
            }
        }

        public bool IsListEmpty => !Items.Any();
    }
}
