using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;

namespace TeamNut.ViewModels
{
    /// <summary>View model for searching and filtering meals.</summary>
    public partial class MealSearchViewModel : ObservableObject
    {
        private readonly IMealService mealService;
        private const string DefaultSearchTerm = "";
        private const string NoIngredientsFoundMessage = "No ingredients found.";
        private const string IngredientsLineSeparator = "\n";

        public ObservableCollection<Meal> Meals { get; private set; } = new ObservableCollection<Meal>();

        public string SearchTerm { get; set; } = DefaultSearchTerm;

        public Meal? SelectedMeal { get; set; }

        public MealSearchViewModel(IMealService mmealService)
        {
            mealService = mmealService;
            _ = LoadMealsAsync();
        }

        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await mealService.GetMealsAsync(
                new MealFilter { SearchTerm = filter ?? string.Empty });
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
            {
                return;
            }

            await mealService.ToggleFavoriteAsync(meal);
        }
    }
}
