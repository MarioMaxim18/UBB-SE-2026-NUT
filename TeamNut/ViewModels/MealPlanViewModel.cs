using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Views.MealPlanView;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly MealPlanService _mealPlanService;
        private const int InvalidId = 0;
        private const string StatusLoadingMealPlan = "Loading your meal plan...";
        private const string StatusLoadingTodayMealPlan = "Loading your meal plan for today...";
        private const string StatusGeneratingMealPlan = "Generating your personalized meal plan for today...";
        private const string StatusMealPlanExists = "Meal plan already generated for today. New plan tomorrow!";
        private const string StatusLoginRequired = "Please log in to view your meal plan.";
        private const string StatusMealPlanGenerated = "New meal plan generated for today!";
        private const string StatusRegeneratingTest = "Regenerating meal plan (test)...";
        private const string ErrorUserNotLoggedTitle = "User Not Logged In";
        private const string ErrorUserNotLoggedMessage = "You need to be logged in to view your meal plan.\n\nPlease create an account or log in to continue.";
        private const string ErrorMealPlanExistsTitle = "Meal Plan Already Exists";
        private const string ErrorMealPlanExistsMessage = "You already have a meal plan for today.\n\nYour meal plan will automatically regenerate tomorrow based on your latest preferences.\n\nIf you changed your settings, the new preferences will apply to tomorrow's meal plan.";
        private const string ErrorNoMealsFound = "No meals found in your plan. Please try regenerating tomorrow.";
        private const string ErrorGeneratingMealPlanTitle = "Error Generating Meal Plan";
        private const string ErrorUnexpectedTitle = "Unexpected Error";
        private const string ErrorUnexpectedMessageFormat = "An unexpected error occurred:\n\n{0}";
        private const string ErrorSavingLogTitle = "Save Failed";
        private const string ErrorSavingLogMessageFormat = "Failed to save to daily log:\n\n{0}";
        private const string ErrorNoMealPlanTitle = "No Meal Plan";
        private const string ErrorNoMealPlanMessage = "No meal plan is currently loaded. Please generate a meal plan first.";
        private const string ErrorMealNotFound = "Meal not found in current meal plan.";
        private const string GoalSuffix = " Goal";
        private const string StatusMealPlanTitleFormat = "Your meal plan for today ({0} goal)";
        private const string NutritionSummaryFormat = "Daily Total: {0} kcal | {1}g protein | {2}g carbs | {3}g fat";
        private const string MealSavedSuccessFormat = "All {0} meals saved to daily log!";
        private static readonly Dictionary<int, string> MealTypes = new()
        {
            { 0, "BREAKFAST" },
            { 1, "LUNCH" },
            { 2, "DINNER" }
        };

        [ObservableProperty] public partial string StatusMessage { get; set; }
        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] private ObservableCollection<MealViewModel> generatedMeals = new();

        private int _currentMealPlanId;
        public int CurrentMealPlanId
        {
            get => _currentMealPlanId;
            set => SetProperty(ref _currentMealPlanId, value);
        }

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

        [ObservableProperty] public partial string TotalNutritionSummary { get; set; }
        [ObservableProperty] private string goalDescription = string.Empty;
        [ObservableProperty] private bool showErrorDialog;
        [ObservableProperty] private string errorDialogTitle = string.Empty;
        [ObservableProperty] private string errorDialogMessage = string.Empty;

        public MealPlanViewModel()
        {
            _mealPlanService = new MealPlanService();
        }

        [RelayCommand]
        private async Task OnGenerateMealPlan()
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= InvalidId)
            {
                ErrorDialogTitle = ErrorUserNotLoggedTitle;
                ErrorDialogMessage = ErrorUserNotLoggedMessage;
                ShowErrorDialog = true;
                StatusMessage = StatusLoginRequired;
                return;
            }

            var todaysPlan = await _mealPlanService.GetTodaysMealPlanAsync(userId.Value);

            if (todaysPlan != null)
            {
                ErrorDialogTitle = ErrorMealPlanExistsTitle;
                ErrorDialogMessage = ErrorMealPlanExistsMessage;
                ShowErrorDialog = true;
                StatusMessage = StatusMealPlanExists;
            }
            else
            {
                await LoadOrGenerateTodaysMealPlanAsync();
            }
        }

        public async Task LoadOrGenerateTodaysMealPlanAsync()
        {
            IsBusy = true;
            StatusMessage = StatusLoadingMealPlan;
            GeneratedMeals.Clear();
            TotalNutritionSummary = string.Empty;
            GoalDescription = string.Empty;

            try
            {
                int? userId = UserSession.UserId;

                if (userId == null || userId <= InvalidId)
                {
                    StatusMessage = StatusLoginRequired;
                    HasMeals = false;
                    return;
                }

                var todaysPlan = await _mealPlanService.GetTodaysMealPlanAsync(userId.Value);

                if (todaysPlan != null)
                {
                    StatusMessage = StatusLoadingTodayMealPlan;
                    await LoadMealPlanByIdAsync(todaysPlan.Id, userId.Value);
                }
                else
                {
                    StatusMessage = StatusGeneratingMealPlan;
                    await GenerateNewMealPlanAsync(userId.Value);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMealPlanByIdAsync(int mealPlanId, int userId)
        {
            CurrentMealPlanId = mealPlanId;

            var meals = await _mealPlanService.GetMealsForMealPlanAsync(mealPlanId);
            if (meals == null || meals.Count == 0)
            {
                StatusMessage = ErrorNoMealsFound;
                HasMeals = false;
                return;
            }

            string userGoal = await _mealPlanService.GetUserGoalAsync(userId);
            string goalName = char.ToUpper(userGoal[0]) + userGoal[1..];

            int index = 0;
            foreach (var meal in meals)
            {
                string mealType = MealTypes.TryGetValue(index, out var type)
                    ? type
                    : "MEAL";

                GeneratedMeals.Add(MealViewModel.FromMeal(meal, mealType));
                index++;
            }

            CalculateTotals();

            TotalNutritionSummary = string.Format(
                NutritionSummaryFormat,
                TotalCalories,
                TotalProtein,
                TotalCarbs,
                TotalFat);

            GoalDescription = goalName + GoalSuffix;
            StatusMessage = string.Format(StatusMealPlanTitleFormat, goalName);
            HasMeals = true;
        }

        private async Task GenerateNewMealPlanAsync(int userId)
        {
            try
            {
                int mealPlanId = await _mealPlanService.GeneratePersonalizedMealPlanAsync(userId);
                await LoadMealPlanByIdAsync(mealPlanId, userId);
                StatusMessage = StatusMealPlanGenerated;
            }
            catch (Exception ex)
            {
                ErrorDialogTitle = ErrorGeneratingMealPlanTitle;
                ErrorDialogMessage = ex.Message;
                ShowErrorDialog = true;
                HasMeals = false;
            }
        }

        private void CalculateTotals()
        {
            TotalCalories = GeneratedMeals.Sum(m => m.Calories);
            TotalProtein = GeneratedMeals.Sum(m => m.Protein);
            TotalCarbs = GeneratedMeals.Sum(m => m.Carbs);
            TotalFat = GeneratedMeals.Sum(m => m.Fat);
        }

        [RelayCommand]
        private async Task SaveToDailyLog()
        {
            if (CurrentMealPlanId <= InvalidId)
            {
                ErrorDialogTitle = ErrorNoMealPlanTitle;
                ErrorDialogMessage = ErrorNoMealPlanMessage;
                ShowErrorDialog = true;
                return;
            }

            await _mealPlanService.SaveMealsToDailyLogAsync(CurrentMealPlanId);
            StatusMessage = string.Format(MealSavedSuccessFormat, GeneratedMeals.Count);
        }

        internal async Task SaveToDailyLogAsync()
        {
            await SaveToDailyLog();
        }
    }
}