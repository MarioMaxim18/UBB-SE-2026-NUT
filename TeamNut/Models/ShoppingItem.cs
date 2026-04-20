using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
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
