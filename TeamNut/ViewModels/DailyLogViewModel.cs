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
        private static class Constants
        {
            public const double DefaultDailyCaloriesGoal = 2000;
            public const double DefaultDailyProteinGoal = 150;
            public const double DefaultDailyCarbsGoal = 250;
            public const double DefaultDailyFatsGoal = 70;

            public const int DaysPerWeek = 7;
            public const double PercentMultiplier = 100.0;

            public const string CaloriesUnit = "kcal";
            public const string GramsUnit = "g";

            public const string NumberFormatNoDecimals = "F0";
            public const string MetricFormatWithGoal = "{0:F0} / {1:F0} {2} ({3:F0}%)";
            public const string MetricFormatNoGoal = "{0:F0} {1}";
            public const string BurnedCaloriesFormat = "{0:F0} kcal";
            public const string LoggedMealFormat = "Logged {0}.";

            public const string NoMealSelectedMessage = "Select a meal first.";
            public const string NoMealsFoundMessage = "No meals found.";
            public const string NoDataMessage = "You need to have had atleast one consumed meal.";

            public const string Empty = "";
            public const StringComparison CaseInsensitiveComparison = StringComparison.OrdinalIgnoreCase;
        }

        private readonly DailyLogService _service;

        private bool _hasData;
        private string _statusMessage = Constants.Empty;
        private DailyLog _dailyTotals = new();
        private DailyLog _weeklyTotals = new();

        private double _dailyCaloriesGoal = Constants.DefaultDailyCaloriesGoal;
        private double _dailyProteinGoal = Constants.DefaultDailyProteinGoal;
        private double _dailyCarbsGoal = Constants.DefaultDailyCarbsGoal;
        private double _dailyFatsGoal = Constants.DefaultDailyFatsGoal;

        private string _dailyCaloriesText = Constants.Empty;
        private string _dailyProteinText = Constants.Empty;
        private string _dailyCarbsText = Constants.Empty;
        private string _dailyFatsText = Constants.Empty;

        private string _weeklyCaloriesText = Constants.Empty;
        private string _weeklyProteinText = Constants.Empty;
        private string _weeklyCarbsText = Constants.Empty;
        private string _weeklyFatsText = Constants.Empty;

        private string _dailyBurnedCaloriesText = Constants.Empty;
        private string _mealSearchText = Constants.Empty;

        private ObservableCollection<Meal> _availableMeals = new();
        private ObservableCollection<Meal> _filteredMeals = new();
        private Meal? _selectedMeal;
        private string _logMealStatusMessage = Constants.Empty;

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

        public double WeeklyCaloriesGoal => DailyCaloriesGoal * Constants.DaysPerWeek;
        public double WeeklyProteinGoal => DailyProteinGoal * Constants.DaysPerWeek;
        public double WeeklyCarbsGoal => DailyCarbsGoal * Constants.DaysPerWeek;
        public double WeeklyFatsGoal => DailyFatsGoal * Constants.DaysPerWeek;

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
            _availableMeals = new ObservableCollection<Meal>(meals);
            UpdateFilteredMeals();
        }

        public async Task LogSelectedMealAsync()
        {
            if (SelectedMeal == null)
            {
                LogMealStatusMessage = Constants.NoMealSelectedMessage;
                return;
            }

            await _service.LogMealAsync(SelectedMeal);
            LogMealStatusMessage = string.Format(Constants.LoggedMealFormat, SelectedMeal.Name);

            MealSearchText = Constants.Empty;
            SelectedMeal = null;

            await LoadAsync();
        }

        private void UpdateFilteredMeals()
        {
            _filteredMeals.Clear();
            var query = MealSearchText?.Trim() ?? Constants.Empty;

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _availableMeals
                : new ObservableCollection<Meal>(
                    _availableMeals.Where(m =>
                        m.Name.Contains(query, Constants.CaseInsensitiveComparison)));

            foreach (var meal in filtered)
            {
                _filteredMeals.Add(meal);
            }

            LogMealStatusMessage = !string.IsNullOrWhiteSpace(query) && _filteredMeals.Count == 0
                ? Constants.NoMealsFoundMessage
                : Constants.Empty;
        }

        public async Task LoadAsync()
        {
            if (!await _service.HasAnyLogsAsync())
            {
                HasData = false;
                StatusMessage = Constants.NoDataMessage;
                return;
            }

            HasData = true;
            StatusMessage = Constants.Empty;

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

            DailyCaloriesText = BuildMetricText(DailyTotals.Calories, DailyCaloriesGoal, Constants.CaloriesUnit);
            DailyProteinText = BuildMetricText(DailyTotals.Protein, DailyProteinGoal, Constants.GramsUnit);
            DailyCarbsText = BuildMetricText(DailyTotals.Carbs, DailyCarbsGoal, Constants.GramsUnit);
            DailyFatsText = BuildMetricText(DailyTotals.Fats, DailyFatsGoal, Constants.GramsUnit);

            var burnedCalories = await _service.GetTodayBurnedCaloriesAsync();
            DailyBurnedCaloriesText = string.Format(Constants.BurnedCaloriesFormat, burnedCalories);

            WeeklyCaloriesText = BuildMetricText(WeeklyTotals.Calories, WeeklyCaloriesGoal, Constants.CaloriesUnit);
            WeeklyProteinText = BuildMetricText(WeeklyTotals.Protein, WeeklyProteinGoal, Constants.GramsUnit);
            WeeklyCarbsText = BuildMetricText(WeeklyTotals.Carbs, WeeklyCarbsGoal, Constants.GramsUnit);
            WeeklyFatsText = BuildMetricText(WeeklyTotals.Fats, WeeklyFatsGoal, Constants.GramsUnit);
        }

        private static string BuildMetricText(double total, double goal, string unit)
        {
            if (goal <= 0)
            {
                return string.Format(Constants.MetricFormatNoGoal, total, unit);
            }

            var pct = (total / goal) * Constants.PercentMultiplier;
            return string.Format(Constants.MetricFormatWithGoal, total, goal, unit, pct);
        }
    }
}