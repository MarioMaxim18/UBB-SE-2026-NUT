using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    /// <summary>Represents the active filters used when searching meals.</summary>
    public partial class MealFilter : ObservableObject
    {
        /// <summary>Gets or sets a value indicating whether the keto filter is active.</summary>
        [ObservableProperty]
        public partial bool IsKeto { get; set; }

        /// <summary>Gets or sets a value indicating whether the vegan filter is active.</summary>
        [ObservableProperty]
        public partial bool IsVegan { get; set; }

        /// <summary>Gets or sets a value indicating whether the nut-free filter is active.</summary>
        [ObservableProperty]
        public partial bool IsNutFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the lactose-free filter is active.</summary>
        [ObservableProperty]
        public partial bool IsLactoseFree { get; set; }

        /// <summary>Gets or sets a value indicating whether the gluten-free filter is active.</summary>
        [ObservableProperty]
        public partial bool IsGlutenFree { get; set; }

        /// <summary>Gets or sets a value indicating whether only favourite meals are shown.</summary>
        [ObservableProperty]
        public partial bool IsFavoriteOnly { get; set; }

        /// <summary>Gets or sets the text search term.</summary>
        [ObservableProperty]
        public partial string SearchTerm { get; set; } = string.Empty;
    }
}
