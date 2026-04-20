using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Views.MealPlanView;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly MealPlanService mealPlanService;

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MealViewModel> GeneratedMeals { get; set; } = new ObservableCollection<MealViewModel>();

        private int currentMealPlanId;

        public int CurrentMealPlanId
        {
            get => currentMealPlanId;
            set => SetProperty(ref currentMealPlanId, value);
        }

        private int totalCalories;

        public int TotalCalories
        {
            get => totalCalories;
            set => SetProperty(ref totalCalories, value);
        }

        private int totalProtein;

        public int TotalProtein
        {
            get => totalProtein;
            set => SetProperty(ref totalProtein, value);
        }

        private int totalCarbs;

        public int TotalCarbs
        {
            get => totalCarbs;
            set => SetProperty(ref totalCarbs, value);
        }

        private int totalFat;

        public int TotalFat
        {
            get => totalFat;
            set => SetProperty(ref totalFat, value);
        }

        private bool hasMeals;

        public bool HasMeals
        {
            get => hasMeals;
            set => SetProperty(ref hasMeals, value);
        }

        [ObservableProperty]
        public partial string TotalNutritionSummary { get; set; }

        [ObservableProperty]
        public partial string GoalDescription { get; set; }

        [ObservableProperty]
        public partial bool ShowErrorDialog { get; set; }

        [ObservableProperty]
        public partial string ErrorDialogTitle { get; set; }

        [ObservableProperty]
        public partial string ErrorDialogMessage { get; set; }

        public MealPlanViewModel()
        {
            mealPlanService = new MealPlanService();
        }

        [RelayCommand]
        private async Task OnGenerateMealPlan()
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                ErrorDialogTitle = "User Not Logged In";
                ErrorDialogMessage = "You need to be logged in to view your meal plan.\n\nPlease create an account or log in to continue.";
                ShowErrorDialog = true;
                StatusMessage = "Please log in to view your meal plan.";
                return;
            }

            var todaysPlan = await mealPlanService.GetTodaysMealPlanAsync(userId.Value);

            if (todaysPlan != null)
            {
                ErrorDialogTitle = "Meal Plan Already Exists";
                ErrorDialogMessage = "You already have a meal plan for today.\n\nYour meal plan will automatically regenerate tomorrow based on your latest preferences.\n\nIf you changed your settings, the new preferences will apply to tomorrow's meal plan.";
                ShowErrorDialog = true;
                StatusMessage = "Meal plan already generated for today. New plan tomorrow!";
            }
            else
            {
                await LoadOrGenerateTodaysMealPlanAsync();
            }
        }

        public async Task LoadOrGenerateTodaysMealPlanAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading your meal plan...";
            GeneratedMeals.Clear();
            TotalNutritionSummary = string.Empty;
            GoalDescription = string.Empty;

            try
            {
                int? userId = UserSession.UserId;

                if (userId == null || userId <= 0)
                {
                    StatusMessage = "Please log in to view your meal plan.";
                    HasMeals = false;
                    return;
                }

                var todaysPlan = await mealPlanService.GetTodaysMealPlanAsync(userId.Value);

                if (todaysPlan != null)
                {
                    StatusMessage = "Loading your meal plan for today...";
                    await LoadMealPlanByIdAsync(todaysPlan.Id, userId.Value);
                }
                else
                {
                    StatusMessage = "Generating your personalized meal plan for today...";
                    await GenerateNewMealPlanAsync(userId.Value);
                }
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

        private async Task LoadMealPlanByIdAsync(int mealPlanId, int userId)
        {
            try
            {
                CurrentMealPlanId = mealPlanId;

                string userGoal = await mealPlanService.GetUserGoalAsync(userId);

                var meals = await mealPlanService.GetMealsForMealPlanAsync(mealPlanId);

                if (meals == null || meals.Count == 0)
                {
                    StatusMessage = "No meals found in your plan. Please try regenerating tomorrow.";
                    HasMeals = false;
                    return;
                }

                int index = 0;
                var mealTypes = new Dictionary<int, string>
                {
                    { 0, "BREAKFAST" },
                    { 1, "LUNCH" },
                    { 2, "DINNER" },
                };

                foreach (var meal in meals)
                {
                    var mealType = mealTypes.ContainsKey(index) ? mealTypes[index] : "MEAL";
                    var mealViewModel = MealViewModel.FromMeal(meal, mealType);
                    GeneratedMeals.Add(mealViewModel);
                    index++;
                }

                CalculateTotals();

                var (totalCal, totalPro, totalCar, totalFt) = mealPlanService.CalculateTotalNutrition(meals);

                string goalName = string.IsNullOrEmpty(userGoal) ? "Maintenance" : char.ToUpper(userGoal[0]) + userGoal.Substring(1);
                GoalDescription = $"{goalName} Goal";

                TotalNutritionSummary = $"Daily Total: {totalCal} kcal | {totalPro}g protein | {totalCar}g carbs | {totalFt}g fat";

                StatusMessage = $"Your meal plan for today ({goalName} goal)";
                HasMeals = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading meal plan: {ex.Message}";
                HasMeals = false;
            }
        }

        private async Task GenerateNewMealPlanAsync(int userId)
        {
            try
            {
                string userGoal = await mealPlanService.GetUserGoalAsync(userId);

                int mealPlanId = await mealPlanService.GeneratePersonalizedMealPlanAsync(userId);

                await LoadMealPlanByIdAsync(mealPlanId, userId);

                StatusMessage = $"New meal plan generated for today!";
            }
            catch (InvalidOperationException ex)
            {
                ErrorDialogTitle = "Error Generating Meal Plan";
                ErrorDialogMessage = ex.Message;
                ShowErrorDialog = true;
                StatusMessage = $"{ex.Message}";
                HasMeals = false;
            }
            catch (Exception ex)
            {
                ErrorDialogTitle = "Unexpected Error";
                ErrorDialogMessage = $"An unexpected error occurred:\n\n{ex.Message}";
                ShowErrorDialog = true;
                StatusMessage = $"Error: {ex.Message}";
                HasMeals = false;
            }
        }

        public async void LoadTodaysMealPlan()
        {
            await LoadOrGenerateTodaysMealPlanAsync();
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
            try
            {
                if (CurrentMealPlanId <= 0)
                {
                    ErrorDialogTitle = "No Meal Plan";
                    ErrorDialogMessage = "No meal plan is currently loaded. Please generate a meal plan first.";
                    ShowErrorDialog = true;
                    return;
                }

                await mealPlanService.SaveMealsToDailyLogAsync(CurrentMealPlanId);

                StatusMessage = $"All {GeneratedMeals.Count} meals saved to daily log!";
            }
            catch (Exception ex)
            {
                ErrorDialogTitle = "Save Failed";
                ErrorDialogMessage = $"Failed to save to daily log:\n\n{ex.Message}";
                ShowErrorDialog = true;
            }
        }

        public async Task SaveToDailyLogAsync()
        {
            if (CurrentMealPlanId <= 0)
            {
                throw new InvalidOperationException("No meal plan is currently loaded. Please generate a meal plan first.");
            }

            await mealPlanService.SaveMealsToDailyLogAsync(CurrentMealPlanId);
        }

        public async Task RegenerateMealPlanForTestingAsync()
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= 0)
            {
                throw new InvalidOperationException("You must be logged in to regenerate a meal plan.");
            }

            IsBusy = true;
            StatusMessage = "Regenerating meal plan (test)...";
            GeneratedMeals.Clear();

            try
            {
                await GenerateNewMealPlanAsync(userId.Value);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SaveMealToDailyLogAsync(int mealId)
        {
            var meal = GeneratedMeals.FirstOrDefault(m => m.Id == mealId);
            if (meal == null)
            {
                throw new InvalidOperationException("Meal not found in current meal plan.");
            }

            await mealPlanService.SaveMealToDailyLogAsync(mealId, meal.Calories);
        }
    }
}
