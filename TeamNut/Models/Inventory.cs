using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class Inventory : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private int _userId;

        [ObservableProperty]
        private int _ingredientId;

        [ObservableProperty]
        private int _quantityGrams;

        
        [ObservableProperty]
        private string _ingredientName = string.Empty;
    }
}
