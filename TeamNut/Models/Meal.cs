using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeamNut.Models
{
    public partial class Meal : ObservableObject
    {
        [ObservableProperty]
        [Key]
        public partial int Id { get; set; }
        [ObservableProperty]
        public partial string Name { get; set; }

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

        [ObservableProperty]
        public partial string Description { get; set; }

        [ObservableProperty]
        public partial string ImageUrl { get; set; }
    }
}