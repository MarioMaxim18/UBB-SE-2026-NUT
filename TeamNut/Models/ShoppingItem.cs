// <copyright file="ShoppingItem.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>Represents an item on the user's shopping list.</summary>
    public partial class ShoppingItem : ObservableObject
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int IngredientId { get; set; }

        [ObservableProperty]
        public partial string IngredientName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double QuantityGrams { get; set; }

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }
}
