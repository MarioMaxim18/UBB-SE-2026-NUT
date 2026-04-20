using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public class DailyLogViewModel : ObservableObject
    {
        private readonly DailyLogService service;
        private bool hasData;
        private string statusMessage = string.Empty;
        private DailyLog dailyTotals = new DailyLog();
        private DailyLog weeklyTotals = new DailyLog();
        private double dailyCaloriesGoal = 2000;
        private double dailyProteinGoal = 150;
        private double dailyCarbsGoal = 250;
        private double dailyFatsGoal = 70;
        private string dailyCaloriesText = string.Empty;
        private string dailyProteinText = string.Empty;
        private string dailyCarbsText = string.Empty;
        private string dailyFatsText = string.Empty;
        private string weeklyCaloriesText = string.Empty;
        private string weeklyProteinText = string.Empty;
        private string weeklyCarbsText = string.Empty;
        private string weeklyFatsText = string.Empty;
        private string dailyBurnedCaloriesText = string.Empty;
        private string mealSearchText = string.Empty;
        private ObservableCollection<Meal> availableMeals = new ObservableCollection<Meal>();
        private ObservableCollection<Meal> filteredMeals = new ObservableCollection<Meal>();
        private Meal? selectedMeal;
        private string logMealStatusMessage = string.Empty;

        public DailyLogViewModel()
        {
            service = new DailyLogService();
            _ = LoadMealsForAutocompleteAsync();
        }

        public bool HasData
        {
            get => hasData;
            set => SetProperty(ref hasData, value);
        }

        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        public DailyLog DailyTotals
        {
            get => dailyTotals;
            set => SetProperty(ref dailyTotals, value);
        }

        public DailyLog WeeklyTotals
        {
            get => weeklyTotals;
            set => SetProperty(ref weeklyTotals, value);
        }

        public double DailyCaloriesGoal
        {
            get => dailyCaloriesGoal;
            set => SetProperty(ref dailyCaloriesGoal, value);
        }

        public double DailyProteinGoal
        {
            get => dailyProteinGoal;
            set => SetProperty(ref dailyProteinGoal, value);
        }

        public double DailyCarbsGoal
        {
            get => dailyCarbsGoal;
            set => SetProperty(ref dailyCarbsGoal, value);
        }

        public double DailyFatsGoal
        {
            get => dailyFatsGoal;
            set => SetProperty(ref dailyFatsGoal, value);
        }

        public double WeeklyCaloriesGoal => DailyCaloriesGoal * 7;

        public double WeeklyProteinGoal => DailyProteinGoal * 7;

        public double WeeklyCarbsGoal => DailyCarbsGoal * 7;

        public double WeeklyFatsGoal => DailyFatsGoal * 7;

        public string DailyCaloriesText
        {
            get => dailyCaloriesText;
            set => SetProperty(ref dailyCaloriesText, value);
        }

        public string DailyProteinText
        {
            get => dailyProteinText;
            set => SetProperty(ref dailyProteinText, value);
        }

        public string DailyCarbsText
        {
            get => dailyCarbsText;
            set => SetProperty(ref dailyCarbsText, value);
        }

        public string DailyFatsText
        {
            get => dailyFatsText;
            set => SetProperty(ref dailyFatsText, value);
        }

        public string WeeklyCaloriesText
        {
            get => weeklyCaloriesText;
            set => SetProperty(ref weeklyCaloriesText, value);
        }

        public string WeeklyProteinText
        {
            get => weeklyProteinText;
            set => SetProperty(ref weeklyProteinText, value);
        }

        public string WeeklyCarbsText
        {
            get => weeklyCarbsText;
            set => SetProperty(ref weeklyCarbsText, value);
        }

        public string WeeklyFatsText
        {
            get => weeklyFatsText;
            set => SetProperty(ref weeklyFatsText, value);
        }

        public string DailyBurnedCaloriesText
        {
            get => dailyBurnedCaloriesText;
            set => SetProperty(ref dailyBurnedCaloriesText, value);
        }

        public string MealSearchText
        {
            get => mealSearchText;
            set
            {
                if (SetProperty(ref mealSearchText, value))
                {
                    UpdateFilteredMeals();
                }
            }
        }

        public ObservableCollection<Meal> AvailableMeals
        {
            get => availableMeals;
            set => SetProperty(ref availableMeals, value);
        }

        public ObservableCollection<Meal> FilteredMeals
        {
            get => filteredMeals;
            set => SetProperty(ref filteredMeals, value);
        }

        public Meal? SelectedMeal
        {
            get => selectedMeal;
            set => SetProperty(ref selectedMeal, value);
        }

        public string LogMealStatusMessage
        {
            get => logMealStatusMessage;
            set => SetProperty(ref logMealStatusMessage, value);
        }

        public async Task LoadMealsForAutocompleteAsync()
        {
            var meals = await service.GetMealsForAutocompleteAsync();
            AvailableMeals = new ObservableCollection<Meal>(meals);
            UpdateFilteredMeals();
        }

        public async Task LogSelectedMealAsync()
        {
            if (SelectedMeal == null)
            {
                LogMealStatusMessage = "Select a meal first.";
                return;
            }

            await service.LogMealAsync(SelectedMeal);
            LogMealStatusMessage = $"Logged {SelectedMeal.Name}.";
            MealSearchText = string.Empty;
            SelectedMeal = null;
            await LoadAsync();
        }

        private void UpdateFilteredMeals()
        {
            FilteredMeals.Clear();

            var query = MealSearchText?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(query)
                ? AvailableMeals
                : new ObservableCollection<Meal>(AvailableMeals.Where(m => m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));

            foreach (var meal in filtered)
            {
                FilteredMeals.Add(meal);
            }

            if (!string.IsNullOrWhiteSpace(query) && FilteredMeals.Count == 0)
            {
                LogMealStatusMessage = "No meals found.";
            }
            else
            {
                LogMealStatusMessage = string.Empty;
            }
        }

        public async Task LoadAsync()
        {
            if (!await service.HasAnyLogsAsync())
            {
                HasData = false;
                StatusMessage = "You need to have had atleast one consumed meal.";
                return;
            }

            HasData = true;
            StatusMessage = string.Empty;

            var userData = await service.GetCurrentUserNutritionTargetsAsync();
            if (userData != null)
            {
                DailyCaloriesGoal = userData.CalorieNeeds > 0 ? userData.CalorieNeeds : DailyCaloriesGoal;
                DailyProteinGoal = userData.ProteinNeeds > 0 ? userData.ProteinNeeds : DailyProteinGoal;
                DailyCarbsGoal = userData.CarbNeeds > 0 ? userData.CarbNeeds : DailyCarbsGoal;
                DailyFatsGoal = userData.FatNeeds > 0 ? userData.FatNeeds : DailyFatsGoal;
            }

            DailyTotals = await service.GetTodayTotalsAsync();
            WeeklyTotals = await service.GetCurrentWeekTotalsAsync();

            OnPropertyChanged(nameof(WeeklyCaloriesGoal));
            OnPropertyChanged(nameof(WeeklyProteinGoal));
            OnPropertyChanged(nameof(WeeklyCarbsGoal));
            OnPropertyChanged(nameof(WeeklyFatsGoal));

            DailyCaloriesText = BuildMetricText(DailyTotals.Calories, DailyCaloriesGoal, "kcal");
            DailyProteinText = BuildMetricText(DailyTotals.Protein, DailyProteinGoal, "g");
            DailyCarbsText = BuildMetricText(DailyTotals.Carbs, DailyCarbsGoal, "g");
            DailyFatsText = BuildMetricText(DailyTotals.Fats, DailyFatsGoal, "g");
            var burnedCalories = await service.GetTodayBurnedCaloriesAsync();
            DailyBurnedCaloriesText = $"{burnedCalories:F0} kcal";

            WeeklyCaloriesText = BuildMetricText(WeeklyTotals.Calories, WeeklyCaloriesGoal, "kcal");
            WeeklyProteinText = BuildMetricText(WeeklyTotals.Protein, WeeklyProteinGoal, "g");
            WeeklyCarbsText = BuildMetricText(WeeklyTotals.Carbs, WeeklyCarbsGoal, "g");
            WeeklyFatsText = BuildMetricText(WeeklyTotals.Fats, WeeklyFatsGoal, "g");
        }

        private static string BuildMetricText(double total, double goal, string unit)
        {
            if (goal <= 0)
            {
                return $"{total:F0} {unit}";
            }

            var pct = (total / goal) * 100.0;
            return $"{total:F0} / {goal:F0} {unit} ({pct:F0}%)";
        }
    }
}
