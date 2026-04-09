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
        private const string DefaultSearchTerm = "";
        private const string NoIngredientsFoundMessage = "No ingredients found.";
        private const string IngredientsLineSeparator = "\n";
        public ObservableCollection<Meal> Meals { get; private set; } = new();
        public string SearchTerm { get; set; } = DefaultSearchTerm;
        public Meal? SelectedMeal { get; set; }
        public MealSearchViewModel()
        {
            _mealService = new MealService();
            _ = LoadMealsAsync();
        }
        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await _mealService.GetMealsAsync(
                new MealFilter { SearchTerm = filter }
            );
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

        public async Task<string> GetMealIngredientsTextAsync(int mealId)
        {
            var lines = await _mealService.GetMealIngredientLinesAsync(mealId);

            return lines.Count > 0
                ? string.Join(IngredientsLineSeparator, lines)
                : NoIngredientsFoundMessage;
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadMealsAsync(SearchTerm);
        }

        [RelayCommand]
        public async Task ToggleFavoriteAsync(Meal meal)
        {
            if (meal == null)
                return;

            await _mealService.ToggleFavoriteAsync(meal);
        }
    }
}