using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    
    public partial class MealFilter : ObservableObject
    {
        [ObservableProperty]
        private bool _isKeto;

        [ObservableProperty]
        private bool _isVegan;

        [ObservableProperty]
        private bool _isNutFree;

        [ObservableProperty]
        private bool _isLactoseFree;

        [ObservableProperty]
        private bool _isGlutenFree;

        [ObservableProperty]
        private string _searchTerm = string.Empty;
    }
}
