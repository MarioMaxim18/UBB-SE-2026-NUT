using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly MealPlanService _mealPlanService;

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        private ObservableCollection<Meal> generatedMeals = new ObservableCollection<Meal>();

        [ObservableProperty]
        public partial string TotalNutritionSummary { get; set; }

        [ObservableProperty]
        private string goalDescription;

        [ObservableProperty]
        private bool showErrorDialog;

        [ObservableProperty]
        private string errorDialogTitle;

        [ObservableProperty]
        private string errorDialogMessage;

        public MealPlanViewModel()
        {
            _mealPlanService = new MealPlanService();
        }

        [RelayCommand]
        private async void OnGenerateMealPlan()
        {
            StatusMessage = string.Empty;
            TotalNutritionSummary = string.Empty;
            IsBusy = true;
            GeneratedMeals.Clear();

            try
            {
                // Get the current user's ID from the session
                int? userId = UserSession.UserId;

                // Check if user is logged in
                if (userId == null || userId <= 0)
                {
                    ErrorDialogTitle = "User Not Logged In";
                    ErrorDialogMessage = "You need to be logged in to generate a personalized meal plan.\n\nPlease create an account or log in to continue.";
                    ShowErrorDialog = true;
                    StatusMessage = "❌ Please log in to generate your meal plan.";
                    return;
                }

                StatusMessage = "Generating your personalized daily meal plan...";

                // Get the user's goal from the database
                string userGoal = await _mealPlanService.GetUserGoalAsync(userId.Value);

                // Use the service to generate the meal plan
                int mealPlanId = await _mealPlanService.GeneratePersonalizedMealPlanAsync(userId.Value);

                // Use the service to get the meals
                var meals = await _mealPlanService.GetMealsForMealPlanAsync(mealPlanId);

                // Add meals to the observable collection
                foreach (var meal in meals)
                {
                    GeneratedMeals.Add(meal);
                }

                // Calculate and display total nutrition
                var (totalCalories, totalProtein, totalCarbs, totalFat) = _mealPlanService.CalculateTotalNutrition(meals);

                // Display goal information with emoji
                string goalEmoji = _mealPlanService.GetGoalEmoji(userGoal);
                string goalName = char.ToUpper(userGoal[0]) + userGoal.Substring(1);
                GoalDescription = $"{goalEmoji} {goalName} Goal";

                TotalNutritionSummary = $"Daily Total: {totalCalories} kcal | {totalProtein}g protein | {totalCarbs}g carbs | {totalFat}g fat";

                StatusMessage = $"✅ Meal plan generated for your {goalName} goal! {meals.Count} meals tailored to your needs.";
            }
            catch (InvalidOperationException ex)
            {
                ErrorDialogTitle = "Error Generating Meal Plan";
                ErrorDialogMessage = ex.Message;
                ShowErrorDialog = true;
                StatusMessage = $"❌ {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorDialogTitle = "Unexpected Error";
                ErrorDialogMessage = $"An unexpected error occurred while generating your meal plan:\n\n{ex.Message}";
                ShowErrorDialog = true;
                StatusMessage = $"❌ An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
