using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class MealSearchViewModel : ObservableObject
    {
        private readonly MealService mealService;

        public ObservableCollection<Meal> Meals { get; private set; } = new ObservableCollection<Meal>();

        public string SearchTerm { get; set; } = string.Empty;

        public Meal? SelectedMeal { get; set; }

        public MealSearchViewModel()
        {
            mealService = new MealService();
            _ = LoadMealsAsync();
        }

        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await mealService.GetMealsAsync(new MealFilter { SearchTerm = filter });
            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));
        }

        public async Task<List<Meal>> SearchMealsAsync(MealFilter filter)
        {
            var list = await mealService.GetFilteredMealsAsync(filter);
            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));
            return list;
        }

        public async Task<string> GetMealIngredientsTextAsync(int mealId)
        {
            var lines = await mealService.GetMealIngredientLinesAsync(mealId);
            return lines.Count > 0 ? string.Join("\n", lines) : "No ingredients found.";
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
            {
                return;
            }
            await mealService.ToggleFavoriteAsync(meal);
        }
    }
}
