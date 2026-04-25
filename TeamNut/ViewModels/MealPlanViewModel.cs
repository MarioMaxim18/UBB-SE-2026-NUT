namespace TeamNut.ModelViews
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using TeamNut.Models;
    using TeamNut.Services;
    using TeamNut.Services.Interfaces;
    using TeamNut.Views.MealPlanView;

    /// <summary>
    /// MealPlanViewModel.
    /// </summary>
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly IMealPlanService mealPlanService;

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

        private const string NutritionSummaryFormat = "Daily Total: {0} kcal | {1}g protein | {2}g carbohydrates | {3}g fat";

        private const string MealSavedSuccessFormat = "All {0} meals saved to daily log!";

        private static readonly Dictionary<int, string> MealTypes = new Dictionary<int, string>
        {
            { 0, "BREAKFAST" },
            { 1, "LUNCH" },
            { 2, "DINNER" }
        };

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MealViewModel> GeneratedMeals { get; set; }

        private int currentMealPlanId;

        public int CurrentMealPlanId
        {
            get => this.currentMealPlanId;
            set => SetProperty(ref this.currentMealPlanId, value);
        }

        private int totalCalories;

        public int TotalCalories
        {
            get => this.totalCalories;
            set => SetProperty(ref this.totalCalories, value);
        }

        private int totalProtein;

        public int TotalProtein
        {
            get => this.totalProtein;
            set => SetProperty(ref this.totalProtein, value);
        }

        private int totalCarbohydrates;

        public int TotalCarbohydrates
        {
            get => this.totalCarbohydrates;
            set => SetProperty(ref this.totalCarbohydrates, value);
        }

        private int totalFat;

        public int TotalFat
        {
            get => this.totalFat;
            set => SetProperty(ref this.totalFat, value);
        }

        private bool hasMeals;

        public bool HasMeals
        {
            get => this.hasMeals;
            set => SetProperty(ref this.hasMeals, value);
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

        public MealPlanViewModel(IMealPlanService mealPlanService)
        {
            this.GeneratedMeals = new ObservableCollection<MealViewModel>();
            this.StatusMessage = string.Empty;
            this.TotalNutritionSummary = string.Empty;
            this.GoalDescription = string.Empty;
            this.ErrorDialogTitle = string.Empty;
            this.ErrorDialogMessage = string.Empty;
            this.mealPlanService = mealPlanService;
        }

        [RelayCommand]
        private async Task OnGenerateMealPlan()
        {
            int? userId = UserSession.UserId;

            if (userId == null || userId <= InvalidId)
            {
                this.ErrorDialogTitle = ErrorUserNotLoggedTitle;
                this.ErrorDialogMessage = ErrorUserNotLoggedMessage;
                this.ShowErrorDialog = true;
                this.StatusMessage = StatusLoginRequired;
                return;
            }

            var todaysPlan = await this.mealPlanService.GetTodaysMealPlanAsync(userId.Value);

            if (todaysPlan != null)
            {
                this.ErrorDialogTitle = ErrorMealPlanExistsTitle;
                this.ErrorDialogMessage = ErrorMealPlanExistsMessage;
                this.ShowErrorDialog = true;
                this.StatusMessage = StatusMealPlanExists;
            }
            else
            {
                await this.LoadOrGenerateTodaysMealPlanAsync();
            }
        }

        public async Task ForceRegenerateMealPlanAsync()
        {
            this.IsBusy = true;
            this.StatusMessage = StatusRegeneratingTest;
            this.GeneratedMeals.Clear();
            this.TotalNutritionSummary = string.Empty;
            this.GoalDescription = string.Empty;

            try
            {
                int? userId = UserSession.UserId;

                if (userId == null || userId <= InvalidId)
                {
                    this.StatusMessage = StatusLoginRequired;
                    HasMeals = false;
                    return;
                }

                await this.GenerateNewMealPlanAsync(userId.Value);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public async Task LoadOrGenerateTodaysMealPlanAsync()
        {
            this.IsBusy = true;
            this.StatusMessage = StatusLoadingMealPlan;
            this.GeneratedMeals.Clear();
            this.TotalNutritionSummary = string.Empty;
            this.GoalDescription = string.Empty;

            try
            {
                int? userId = UserSession.UserId;

                if (userId == null || userId <= InvalidId)
                {
                    this.StatusMessage = StatusLoginRequired;
                    HasMeals = false;
                    return;
                }

                var todaysPlan = await this.mealPlanService.GetTodaysMealPlanAsync(userId.Value);

                if (todaysPlan != null)
                {
                    this.StatusMessage = StatusLoadingTodayMealPlan;
                    await this.LoadMealPlanByIdAsync(todaysPlan.Id, userId.Value);
                }
                else
                {
                    this.StatusMessage = StatusGeneratingMealPlan;
                    await this.GenerateNewMealPlanAsync(userId.Value);
                }
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task LoadMealPlanByIdAsync(int mealPlanId, int userId)
        {
            CurrentMealPlanId = mealPlanId;

            var meals = await this.mealPlanService.GetMealsForMealPlanAsync(mealPlanId);
            if (meals == null || meals.Count == 0)
            {
                this.StatusMessage = ErrorNoMealsFound;
                HasMeals = false;
                return;
            }

            string userGoal = await this.mealPlanService.GetUserGoalAsync(userId);
            string goalName = char.ToUpper(userGoal[0]) + userGoal[1..];

            int index = 0;
            foreach (var meal in meals)
            {
                string mealType = MealTypes.TryGetValue(index, out var type)
                    ? type
                    : "MEAL";

                this.GeneratedMeals.Add(MealViewModel.FromMeal(meal, mealType));
                index++;
            }

            this.CalculateTotals();

            this.TotalNutritionSummary = string.Format(
                NutritionSummaryFormat,
                TotalCalories,
                TotalProtein,
                TotalCarbohydrates,
                TotalFat);

            this.GoalDescription = goalName + GoalSuffix;
            this.StatusMessage = string.Format(StatusMealPlanTitleFormat, goalName);
            HasMeals = true;
        }

        private async Task GenerateNewMealPlanAsync(int userId)
        {
            try
            {
                int mealPlanId = await this.mealPlanService.GeneratePersonalizedMealPlanAsync(userId);
                await this.LoadMealPlanByIdAsync(mealPlanId, userId);
                this.StatusMessage = StatusMealPlanGenerated;
            }
            catch (Exception ex)
            {
                this.ErrorDialogTitle = ErrorGeneratingMealPlanTitle;
                this.ErrorDialogMessage = ex.Message;
                this.ShowErrorDialog = true;
                HasMeals = false;
            }
        }

        private void CalculateTotals()
        {
            TotalCalories = this.GeneratedMeals.Sum(m => m.Calories);
            TotalProtein = this.GeneratedMeals.Sum(m => m.Protein);
            TotalCarbohydrates = this.GeneratedMeals.Sum(m => m.Carbohydrates);
            TotalFat = this.GeneratedMeals.Sum(m => m.Fat);
        }

        [RelayCommand]
        private async Task SaveToDailyLog()
        {
            if (CurrentMealPlanId <= InvalidId)
            {
                this.ErrorDialogTitle = ErrorNoMealPlanTitle;
                this.ErrorDialogMessage = ErrorNoMealPlanMessage;
                this.ShowErrorDialog = true;
                return;
            }

            await this.mealPlanService.SaveMealsToDailyLogAsync(CurrentMealPlanId);
            this.StatusMessage = string.Format(MealSavedSuccessFormat, this.GeneratedMeals.Count);
        }

        internal async Task SaveToDailyLogAsync()
        {
            await this.SaveToDailyLog();
        }
    }
}
