using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class MealFilter : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsKeto { get; set; }

        [ObservableProperty]
        public partial bool IsVegan { get; set; }

        [ObservableProperty]
        public partial bool IsNutFree { get; set; }

        [ObservableProperty]
        public partial bool IsLactoseFree { get; set; }

        [ObservableProperty]
        public partial bool IsGlutenFree { get; set; }

        [ObservableProperty]
        public partial bool IsFavoriteOnly { get; set; }

        [ObservableProperty]
        public partial string SearchTerm { get; set; } = string.Empty;
    }
}
