using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    /// <summary>Represents an item on the user's shopping list.</summary>
    public partial class ShoppingItem : ObservableObject
    {
        /// <summary>Gets or sets the shopping item identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the ingredient identifier.</summary>
        public int IngredientId { get; set; }

        /// <summary>Gets or sets the ingredient name.</summary>
        [ObservableProperty]
        public partial string IngredientName { get; set; } = string.Empty;

        /// <summary>Gets or sets the quantity in grams.</summary>
        [ObservableProperty]
        public partial double QuantityGrams { get; set; }

        /// <summary>Gets or sets a value indicating whether the item has been checked off.</summary>
        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }
}
