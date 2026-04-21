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

        /// <summary>Gets the current collection of meals.</summary>
        public ObservableCollection<Meal> Meals { get; private set; } = new ObservableCollection<Meal>();

        /// <summary>Gets or sets the search term text.</summary>
        public string SearchTerm { get; set; } = DefaultSearchTerm;

        /// <summary>Gets or sets the currently selected meal.</summary>
        public Meal? SelectedMeal { get; set; }

        public MealSearchViewModel(IMealService mmealService)
        {
            mealService = mmealService;
            _ = LoadMealsAsync();
        }

        /// <summary>Loads meals, optionally filtered by a search string.</summary>
        /// <param name="filter">Optional text filter.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await mealService.GetMealsAsync(
                new MealFilter { SearchTerm = filter ?? string.Empty });
            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));
        }

        /// <summary>Searches for meals using the given filter criteria.</summary>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>A list of meals that match the filter.</returns>
        public async Task<List<Meal>> SearchMealsAsync(MealFilter filter)
        {
            var list = await mealService.GetFilteredMealsAsync(filter);

            Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(Meals));

            return list;
        }

        /// <summary>Gets a formatted ingredient list text for the given meal.</summary>
        /// <param name="mealId">The meal identifier.</param>
        /// <returns>A newline-separated ingredient list, or a "not found" message.</returns>
        public async Task<string> GetMealIngredientsTextAsync(int mealId)
        {
            var lines = await mealService.GetMealIngredientLinesAsync(mealId);

            return lines.Count > 0
                ? string.Join(IngredientsLineSeparator, lines)
                : NoIngredientsFoundMessage;
        }

        /// <summary>Executes a search using the current <see cref="SearchTerm"/>.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadMealsAsync(SearchTerm);
        }

        /// <summary>Toggles the favourite state of a meal.</summary>
        /// <param name="meal">The meal to toggle.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
