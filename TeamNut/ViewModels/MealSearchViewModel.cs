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
    public partial class MealSearchViewModel : ObservableObject
    {
        private readonly IMealService mealService;
        private readonly IPaginationService paginationService;
        private const string DefaultSearchTerm = "";
        private const string NoIngredientsFoundMessage = "No ingredients found.";
        private const string IngredientsLineSeparator = "\n";
        private const int DefaultPageSize = 5;

        public ObservableCollection<Meal> Meals { get; private set; } = new ObservableCollection<Meal>();

        public string SearchTerm { get; set; } = DefaultSearchTerm;

        public Meal? SelectedMeal { get; set; }

        private List<Meal> allMeals = new List<Meal>();
        private int currentPage = 1;
        private int pageSize = DefaultPageSize;

        [ObservableProperty]
        public partial string PageText { get; set; } = "1 / 1";

        [ObservableProperty]
        public partial bool CanGoToPreviousPage { get; set; }

        [ObservableProperty]
        public partial bool CanGoToNextPage { get; set; }

        public MealSearchViewModel(
            IMealService mmealService,
            IPaginationService ppaginationService)
        {
            mealService = mmealService;
            paginationService = ppaginationService;
            _ = LoadMealsAsync();
        }

        public void SetAllMeals(List<Meal> meals)
        {
            allMeals = meals;
            currentPage = 1;
            UpdatePagedMeals();
        }

        public void GoToNextPage()
        {
            int totalPages = paginationService.GetTotalPages(allMeals.Count, pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdatePagedMeals();
            }
        }

        public void GoToPreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePagedMeals();
            }
        }

        private void UpdatePagedMeals()
        {
            var pagedMeals = paginationService.GetPage(allMeals, currentPage, pageSize);
            int totalPages = paginationService.GetTotalPages(allMeals.Count, pageSize);

            Meals = new ObservableCollection<Meal>(pagedMeals);
            OnPropertyChanged(nameof(Meals));

            PageText = $"{currentPage} / {totalPages}";
            CanGoToPreviousPage = currentPage > 1;
            CanGoToNextPage = currentPage < totalPages;
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
