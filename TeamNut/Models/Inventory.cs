using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    /// <summary>Represents a user's inventory entry for a food ingredient.</summary>
    public partial class Inventory : ObservableObject
    {
        /// <summary>Gets or sets the inventory entry identifier.</summary>
        [ObservableProperty]
        public partial int Id { get; set; }

        /// <summary>Gets or sets the user identifier.</summary>
        [ObservableProperty]
        public partial int UserId { get; set; }

        /// <summary>Gets or sets the ingredient identifier.</summary>
        [ObservableProperty]
        public partial int IngredientId { get; set; }

        /// <summary>Gets or sets the quantity in grams.</summary>
        [ObservableProperty]
        public partial int QuantityGrams { get; set; }

        /// <summary>Gets or sets the ingredient name.</summary>
        [ObservableProperty]
        public partial string IngredientName { get; set; } = string.Empty;
    }
}
