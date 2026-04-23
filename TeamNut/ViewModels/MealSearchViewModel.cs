// <copyright file="MealSearchViewModel.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// MealSearchViewModel.
    /// </summary>
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
            this.mealService = mmealService;
            this.paginationService = ppaginationService;
            _ = this.LoadMealsAsync();
        }

        public void SetAllMeals(List<Meal> meals)
        {
            this.allMeals = meals;
            this.currentPage = 1;
            this.UpdatePagedMeals();
        }

        public void GoToNextPage()
        {
            int totalPages = this.paginationService.GetTotalPages(this.allMeals.Count, this.pageSize);
            if (this.currentPage < totalPages)
            {
                this.currentPage++;
                this.UpdatePagedMeals();
            }
        }

        public void GoToPreviousPage()
        {
            if (this.currentPage > 1)
            {
                this.currentPage--;
                this.UpdatePagedMeals();
            }
        }

        private void UpdatePagedMeals()
        {
            var pagedMeals = this.paginationService.GetPage(this.allMeals, this.currentPage, this.pageSize);
            int totalPages = this.paginationService.GetTotalPages(this.allMeals.Count, this.pageSize);

            this.Meals = new ObservableCollection<Meal>(pagedMeals);
            OnPropertyChanged(nameof(this.Meals));

            this.PageText = $"{this.currentPage} / {totalPages}";
            this.CanGoToPreviousPage = this.currentPage > 1;
            this.CanGoToNextPage = this.currentPage < totalPages;
        }

        public async Task LoadMealsAsync(string? filter = null)
        {
            var list = await this.mealService.GetMealsAsync(
                new MealFilter { SearchTerm = filter ?? string.Empty });
            this.Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(this.Meals));
        }

        public async Task<List<Meal>> SearchMealsAsync(MealFilter filter)
        {
            var list = await this.mealService.GetFilteredMealsAsync(filter);

            this.Meals = new ObservableCollection<Meal>(list);
            OnPropertyChanged(nameof(this.Meals));

            return list;
        }

        public async Task<string> GetMealIngredientsTextAsync(int mealId)
        {
            var lines = await this.mealService.GetMealIngredientLinesAsync(mealId);

            return lines.Count > 0
                ? string.Join(IngredientsLineSeparator, lines)
                : NoIngredientsFoundMessage;
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await this.LoadMealsAsync(this.SearchTerm);
        }

        [RelayCommand]
        public async Task ToggleFavoriteAsync(Meal meal)
        {
            if (meal == null)
            {
                return;
            }

            await this.mealService.ToggleFavoriteAsync(meal);
        }
    }
}
