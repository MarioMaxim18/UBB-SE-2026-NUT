using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System.ComponentModel.DataAnnotations;

namespace TeamNut.Models
{
    public partial class Meal : ObservableObject
    {
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int Calories { get; set; }

        [ObservableProperty]
        public partial int Carbs { get; set; }

        [ObservableProperty]
        public partial int Fat { get; set; }

        [ObservableProperty]
        public partial int Protein { get; set; }

        [ObservableProperty]
        public partial bool IsVegan { get; set; }

        [ObservableProperty]
        public partial bool IsKeto { get; set; }

        [ObservableProperty]
        public partial bool IsGlutenFree { get; set; }

        [ObservableProperty]
        public partial bool IsLactoseFree { get; set; }

        [ObservableProperty]
        public partial bool IsNutFree { get; set; }

        [ObservableProperty]
        public partial bool IsFavorite { get; set; }

        
        public Visibility VeganVisibility => IsVegan ? Visibility.Visible : Visibility.Collapsed;
        public Visibility KetoVisibility => IsKeto ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GlutenFreeVisibility => IsGlutenFree ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LactoseFreeVisibility => IsLactoseFree ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NutFreeVisibility => IsNutFree ? Visibility.Visible : Visibility.Collapsed;

        public Visibility FavoriteVisibility => IsFavorite ? Visibility.Visible : Visibility.Collapsed;

        public string FavoriteIcon => IsFavorite ? "★" : "☆";

        [ObservableProperty]
        public partial string Description { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ImageUrl { get; set; } = string.Empty;

        
        partial void OnIsVeganChanged(bool value) => OnPropertyChanged(nameof(VeganVisibility));
        partial void OnIsKetoChanged(bool value) => OnPropertyChanged(nameof(KetoVisibility));
        partial void OnIsGlutenFreeChanged(bool value) => OnPropertyChanged(nameof(GlutenFreeVisibility));
        partial void OnIsLactoseFreeChanged(bool value) => OnPropertyChanged(nameof(LactoseFreeVisibility));
        partial void OnIsNutFreeChanged(bool value) => OnPropertyChanged(nameof(NutFreeVisibility));

        // to refresh the star immediately when clicked
        partial void OnIsFavoriteChanged(bool value)
        {
            OnPropertyChanged(nameof(FavoriteIcon));
            OnPropertyChanged(nameof(FavoriteVisibility));
        }
    }
}