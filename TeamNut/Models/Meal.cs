using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace TeamNut.Models
{
    /// <summary>Represents a meal and its nutrition/filter metadata.</summary>
    public partial class Meal : ObservableObject
    {
        /// <summary>Gets or sets the meal identifier.</summary>
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }

        /// <summary>Gets or sets the meal name.</summary>
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the calorie count.</summary>
        [ObservableProperty]
        public partial int Calories { get; set; }

        /// <summary>Gets or sets the carbohydrate grams.</summary>
        [ObservableProperty]
        public partial int Carbs { get; set; }

        /// <summary>Gets or sets the fat grams.</summary>
        [ObservableProperty]
        public partial int Fat { get; set; }

        /// <summary>Gets or sets the protein grams.</summary>
        [ObservableProperty]
        public partial int Protein { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is vegan.</summary>
        [ObservableProperty]
        public partial bool IsVegan { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is keto-friendly.</summary>
        [ObservableProperty]
        public partial bool IsKeto { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is gluten-free.</summary>
        [ObservableProperty]
        public partial bool IsGlutenFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is lactose-free.</summary>
        [ObservableProperty]
        public partial bool IsLactoseFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is nut-free.</summary>
        [ObservableProperty]
        public partial bool IsNutFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the meal is marked as a favourite.</summary>
        [ObservableProperty]
        public partial bool IsFavorite { get; set; }

        /// <summary>Gets the visibility for the vegan badge.</summary>
        public Visibility VeganVisibility => IsVegan ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the visibility for the keto badge.</summary>
        public Visibility KetoVisibility => IsKeto ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the visibility for the gluten-free badge.</summary>
        public Visibility GlutenFreeVisibility => IsGlutenFree ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the visibility for the lactose-free badge.</summary>
        public Visibility LactoseFreeVisibility => IsLactoseFree ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the visibility for the nut-free badge.</summary>
        public Visibility NutFreeVisibility => IsNutFree ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the visibility for the favourite indicator.</summary>
        public Visibility FavoriteVisibility => IsFavorite ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Gets the favourite icon character.</summary>
        public string FavoriteIcon => IsFavorite ? "★" : "☆";

        /// <summary>Gets or sets the meal description.</summary>
        [ObservableProperty]
        public partial string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets the URL of the meal image.</summary>
        [ObservableProperty]
        public partial string ImageUrl { get; set; } = string.Empty;

        partial void OnIsVeganChanged(bool value) => OnPropertyChanged(nameof(VeganVisibility));

        partial void OnIsKetoChanged(bool value) => OnPropertyChanged(nameof(KetoVisibility));

        partial void OnIsGlutenFreeChanged(bool value) => OnPropertyChanged(nameof(GlutenFreeVisibility));

        partial void OnIsLactoseFreeChanged(bool value) => OnPropertyChanged(nameof(LactoseFreeVisibility));

        partial void OnIsNutFreeChanged(bool value) => OnPropertyChanged(nameof(NutFreeVisibility));

        partial void OnIsFavoriteChanged(bool value)
        {
            OnPropertyChanged(nameof(FavoriteIcon));
            OnPropertyChanged(nameof(FavoriteVisibility));
        }
    }
}
