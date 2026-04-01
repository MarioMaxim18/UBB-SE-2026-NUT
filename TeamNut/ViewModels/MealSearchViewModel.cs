using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class MealSearchViewModel : ObservableObject
    {
        private readonly MealService _mealService;

        public ObservableCollection<Meal> Meals { get; private set; } = new ObservableCollection<Meal>();

        public string SearchTerm { get; set; } = string.Empty;

        public Meal? SelectedMeal { get; set; }

        public MealSearchViewModel()
        {
            _mealService = new MealService();
            _ = LoadMealsAsync();
        }

        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await _mealService.GetMealsAsync(filter);
            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));
        }

        public async Task<System.Collections.Generic.List<Meal>> SearchMealsAsync(MealFilter filter)
        {
            var list = await _mealService.GetFilteredMealsAsync(filter);
            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));
            return list;
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadMealsAsync(SearchTerm);
        }

        [RelayCommand]
        public void ToggleFavorite(Meal meal)
        {
            if (meal == null) return;
            meal.IsFavorite = !meal.IsFavorite;
        }
    }
}
