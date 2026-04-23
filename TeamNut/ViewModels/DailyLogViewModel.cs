using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public class DailyLogViewModel : ObservableObject
    {
        private readonly DailyLogService _service;

        private bool _hasData;
        private string _statusMessage = string.Empty;
        private DailyLog _dailyTotals = new();
        private DailyLog _weeklyTotals = new();

        // Goals
        private double _dailyCaloriesGoal = 2000;
        private double _dailyProteinGoal = 150;
        private double _dailyCarbsGoal = 250;
        private double _dailyFatsGoal = 70;

        // Display Text
        private string _dailyCaloriesText = string.Empty;
        private string _dailyProteinText = string.Empty;
        private string _dailyCarbsText = string.Empty;
        private string _dailyFatsText = string.Empty;
        private string _weeklyCaloriesText = string.Empty;
        private string _weeklyProteinText = string.Empty;
        private string _weeklyCarbsText = string.Empty;
        private string _weeklyFatsText = string.Empty;
        private string _dailyBurnedCaloriesText = string.Empty;

        // Meal Search
        private string _mealSearchText = string.Empty;
        private ObservableCollection<Meal> _availableMeals = new();
        private ObservableCollection<Meal> _filteredMeals = new();
        private Meal? _selectedMeal;
        private string _logMealStatusMessage = string.Empty;

        public DailyLogViewModel()
        {
            _service = new DailyLogService();
            _ = LoadMealsForAutocompleteAsync();
        }

        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public DailyLog DailyTotals
        {
            get => _dailyTotals;
            set => SetProperty(ref _dailyTotals, value);
        }

        public DailyLog WeeklyTotals
        {
            get => _weeklyTotals;
            set => SetProperty(ref _weeklyTotals, value);
        }

        public double DailyCaloriesGoal
        {
            get => _dailyCaloriesGoal;
            set => SetProperty(ref _dailyCaloriesGoal, value);
        }

        public double DailyProteinGoal
        {
            get => _dailyProteinGoal;
            set => SetProperty(ref _dailyProteinGoal, value);
        }

        public double DailyCarbsGoal
        {
            get => _dailyCarbsGoal;
            set => SetProperty(ref _dailyCarbsGoal, value);
        }

        public double DailyFatsGoal
        {
            get => _dailyFatsGoal;
            set => SetProperty(ref _dailyFatsGoal, value);
        }

        public double WeeklyCaloriesGoal => DailyCaloriesGoal * 7;
        public double WeeklyProteinGoal => DailyProteinGoal * 7;
        public double WeeklyCarbsGoal => DailyCarbsGoal * 7;
        public double WeeklyFatsGoal => DailyFatsGoal * 7;

        public string DailyCaloriesText
        {
            get => _dailyCaloriesText;
            set => SetProperty(ref _dailyCaloriesText, value);
        }

        public string DailyProteinText
        {
            get => _dailyProteinText;
            set => SetProperty(ref _dailyProteinText, value);
        }

        public string DailyCarbsText
        {
            get => _dailyCarbsText;
            set => SetProperty(ref _dailyCarbsText, value);
        }

        public string DailyFatsText
        {
            get => _dailyFatsText;
            set => SetProperty(ref _dailyFatsText, value);
        }

        public string WeeklyCaloriesText
        {
            get => _weeklyCaloriesText;
            set => SetProperty(ref _weeklyCaloriesText, value);
        }

        public string WeeklyProteinText
        {
            get => _weeklyProteinText;
            set => SetProperty(ref _weeklyProteinText, value);
        }

        public string WeeklyCarbsText
        {
            get => _weeklyCarbsText;
            set => SetProperty(ref _weeklyCarbsText, value);
        }

        public string WeeklyFatsText
        {
            get => _weeklyFatsText;
            set => SetProperty(ref _weeklyFatsText, value);
        }

        public string DailyBurnedCaloriesText
        {
            get => _dailyBurnedCaloriesText;
            set => SetProperty(ref _dailyBurnedCaloriesText, value);
        }

        public string MealSearchText
        {
            get => _mealSearchText;
            set
            {
                if (SetProperty(ref _mealSearchText, value))
                {
                    UpdateFilteredMeals();
                }
            }
        }

        public ObservableCollection<Meal> AvailableMeals
        {
            get => _availableMeals;
            set => SetProperty(ref _availableMeals, value);
        }

        public ObservableCollection<Meal> FilteredMeals
        {
            get => _filteredMeals;
            set => SetProperty(ref _filteredMeals, value);
        }

        public Meal? SelectedMeal
        {
            get => _selectedMeal;
            set => SetProperty(ref _selectedMeal, value);
        }

        public string LogMealStatusMessage
        {
            get => _logMealStatusMessage;
            set => SetProperty(ref _logMealStatusMessage, value);
        }

        public async Task LoadMealsForAutocompleteAsync()
        {
            var meals = await _service.GetMealsForAutocompleteAsync();
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

            await _service.LogMealAsync(SelectedMeal);

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
            if (!await _service.HasAnyLogsAsync())
            {
                HasData = false;
                StatusMessage = "You need to have had at least one consumed meal.";
                return;
            }

            HasData = true;
            StatusMessage = string.Empty;

            var userData = await _service.GetCurrentUserNutritionTargetsAsync();
            if (userData != null)
            {
                DailyCaloriesGoal = userData.CalorieNeeds > 0 ? userData.CalorieNeeds : DailyCaloriesGoal;
                DailyProteinGoal = userData.ProteinNeeds > 0 ? userData.ProteinNeeds : DailyProteinGoal;
                DailyCarbsGoal = userData.CarbNeeds > 0 ? userData.CarbNeeds : DailyCarbsGoal;
                DailyFatsGoal = userData.FatNeeds > 0 ? userData.FatNeeds : DailyFatsGoal;
            }

            DailyTotals = await _service.GetTodayTotalsAsync();
            WeeklyTotals = await _service.GetCurrentWeekTotalsAsync();

            OnPropertyChanged(nameof(WeeklyCaloriesGoal));
            OnPropertyChanged(nameof(WeeklyProteinGoal));
            OnPropertyChanged(nameof(WeeklyCarbsGoal));
            OnPropertyChanged(nameof(WeeklyFatsGoal));

            DailyCaloriesText = BuildMetricText(DailyTotals.Calories, DailyCaloriesGoal, "kcal");
            DailyProteinText = BuildMetricText(DailyTotals.Protein, DailyProteinGoal, "g");
            DailyCarbsText = BuildMetricText(DailyTotals.Carbs, DailyCarbsGoal, "g");
            DailyFatsText = BuildMetricText(DailyTotals.Fats, DailyFatsGoal, "g");

            var burnedCalories = await _service.GetTodayBurnedCaloriesAsync();
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