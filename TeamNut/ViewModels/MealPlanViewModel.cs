using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Services;
using TeamNut.Views.MealPlanView;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly MealPlanRepository _mealPlanRepository;

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        private ObservableCollection<MealViewModel> generatedMeals = new();

        private int _totalCalories;
        public int TotalCalories
        {
            get => _totalCalories;
            set => SetProperty(ref _totalCalories, value);
        }

        private int _totalProtein;
        public int TotalProtein
        {
            get => _totalProtein;
            set => SetProperty(ref _totalProtein, value);
        }

        private int _totalCarbs;
        public int TotalCarbs
        {
            get => _totalCarbs;
            set => SetProperty(ref _totalCarbs, value);
        }

        private int _totalFat;
        public int TotalFat
        {
            get => _totalFat;
            set => SetProperty(ref _totalFat, value);
        }

        private bool _hasMeals;
        public bool HasMeals
        {
            get => _hasMeals;
            set => SetProperty(ref _hasMeals, value);
        }

        public MealPlanViewModel()
        {
            _mealPlanRepository = new MealPlanRepository();
        }

        [RelayCommand]
        private async Task OnGenerateMealPlan()
        {
            StatusMessage = string.Empty;
            IsBusy = true;
            GeneratedMeals.Clear();

            try
            {
                StatusMessage = "Generating your personalized daily meal plan...";

                int userId = UserSession.UserId ?? 1;

                int mealPlanId = await _mealPlanRepository.GeneratePersonalizedDailyMealPlan(userId);

                StatusMessage = $"Meal plan created (ID: {mealPlanId}). Loading meals...";

                var meals = await _mealPlanRepository.GetMealsForMealPlan(mealPlanId);

                if (meals == null || meals.Count == 0)
                {
                    StatusMessage = "Meal plan created but no meals were returned.";
                    HasMeals = false;
                    return;
                }

                var mealTypes = new Dictionary<int, string>
                {
                    { 0, "BREAKFAST" },
                    { 1, "LUNCH" },
                    { 2, "DINNER" }
                };

                int index = 0;
                foreach (var meal in meals)
                {
                    var mealType = mealTypes.ContainsKey(index) ? mealTypes[index] : "MEAL";

                    var mealViewModel = MealViewModel.FromMeal(meal, mealType);

                    mealViewModel.Ingredients =
                        await _mealPlanRepository.GetIngredientsForMeal(meal.Id);

                    GeneratedMeals.Add(mealViewModel);
                    index++;
                }

                CalculateTotals();
                HasMeals = GeneratedMeals.Count > 0;

                StatusMessage =
                    $"Success! {meals.Count} meals added. " +
                    $"Total: {TotalCalories} cal, {TotalProtein}g protein, " +
                    $"{TotalCarbs}g carbs, {TotalFat}g fat";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                HasMeals = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void LoadTodaysMealPlan()
        {
            IsBusy = true;
            StatusMessage = "Loading your meal plan for today...";
            GeneratedMeals.Clear();

            try
            {
                int userId = UserSession.UserId ?? 1;

                var todayPlan = await _mealPlanRepository.GetTodaysMealPlan(userId);

                if (todayPlan != null)
                {
                    var meals = await _mealPlanRepository.GetMealsForMealPlan(todayPlan.Id);

                    var mealTypes = new Dictionary<int, string>
                    {
                        { 0, "BREAKFAST" },
                        { 1, "LUNCH" },
                        { 2, "DINNER" }
                    };

                    int index = 0;
                    foreach (var meal in meals)
                    {
                        var mealType = mealTypes.ContainsKey(index) ? mealTypes[index] : "MEAL";

                        var mealViewModel = MealViewModel.FromMeal(meal, mealType);

                        mealViewModel.Ingredients =
                            await _mealPlanRepository.GetIngredientsForMeal(meal.Id);

                        GeneratedMeals.Add(mealViewModel);
                        index++;
                    }

                    CalculateTotals();
                    HasMeals = GeneratedMeals.Count > 0;
                    StatusMessage = string.Empty;
                }
                else
                {
                    StatusMessage = "No meal plan found for today.";
                    HasMeals = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading meal plan: {ex.Message}";
                HasMeals = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CalculateTotals()
        {
            TotalCalories = GeneratedMeals.Sum(m => m.Calories);
            TotalProtein = GeneratedMeals.Sum(m => m.Protein);
            TotalCarbs = GeneratedMeals.Sum(m => m.Carbs);
            TotalFat = GeneratedMeals.Sum(m => m.Fat);
        }
    }
}