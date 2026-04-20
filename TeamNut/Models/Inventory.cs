using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class Inventory : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int UserId { get; set; }

        [ObservableProperty]
        public partial int IngredientId { get; set; }

        [ObservableProperty]
        public partial int QuantityGrams { get; set; }

        [ObservableProperty]
        public partial string IngredientName { get; set; } = string.Empty;
    }
}
