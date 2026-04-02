using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class ShoppingItem : ObservableObject
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int IngredientId { get; set; }

        [ObservableProperty]
        private string ingredientName;

        [ObservableProperty]
        private double quantityGrams;

        [ObservableProperty]
        private bool isChecked;
    }
}
