using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    /// <summary>View model for the daily calorie log page.</summary>
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

        private readonly DailyLogService service;

        private bool hasData;
        private string statusMessage = Constants.Empty;
        private DailyLog dailyTotals = new DailyLog();
        private DailyLog weeklyTotals = new DailyLog();

        private double dailyCaloriesGoal = Constants.DefaultDailyCaloriesGoal;
        private double dailyProteinGoal = Constants.DefaultDailyProteinGoal;
        private double dailyCarbsGoal = Constants.DefaultDailyCarbsGoal;
        private double dailyFatsGoal = Constants.DefaultDailyFatsGoal;

        private string dailyCaloriesText = Constants.Empty;
        private string dailyProteinText = Constants.Empty;
        private string dailyCarbsText = Constants.Empty;
        private string dailyFatsText = Constants.Empty;

        private string weeklyCaloriesText = Constants.Empty;
        private string weeklyProteinText = Constants.Empty;
        private string weeklyCarbsText = Constants.Empty;
        private string weeklyFatsText = Constants.Empty;

        private string dailyBurnedCaloriesText = Constants.Empty;
        private string mealSearchText = Constants.Empty;

        private ObservableCollection<Meal> availableMeals = new ObservableCollection<Meal>();
        private ObservableCollection<Meal> filteredMeals = new ObservableCollection<Meal>();
        private Meal? selectedMeal;
        private string logMealStatusMessage = Constants.Empty;

        /// <summary>Initializes a new instance of the <see cref="DailyLogViewModel"/> class.</summary>
        public DailyLogViewModel()
        {
            service = new DailyLogService();
            _ = LoadMealsForAutocompleteAsync();
        }

        /// <summary>Gets or sets a value indicating whether the view has data to display.</summary>
        public bool HasData
        {
            get => hasData;
            set => SetProperty(ref hasData, value);
        }

        /// <summary>Gets or sets the status message shown to the user.</summary>
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        /// <summary>Gets or sets today's nutrition totals.</summary>
        public DailyLog DailyTotals
        {
            get => dailyTotals;
            set => SetProperty(ref dailyTotals, value);
        }

        /// <summary>Gets or sets this week's nutrition totals.</summary>
        public DailyLog WeeklyTotals
        {
            get => weeklyTotals;
            set => SetProperty(ref weeklyTotals, value);
        }

        /// <summary>Gets or sets the daily calorie goal.</summary>
        public double DailyCaloriesGoal
        {
            get => dailyCaloriesGoal;
            set => SetProperty(ref dailyCaloriesGoal, value);
        }

        /// <summary>Gets or sets the daily protein goal in grams.</summary>
        public double DailyProteinGoal
        {
            get => dailyProteinGoal;
            set => SetProperty(ref dailyProteinGoal, value);
        }

        /// <summary>Gets or sets the daily carbohydrate goal in grams.</summary>
        public double DailyCarbsGoal
        {
            get => dailyCarbsGoal;
            set => SetProperty(ref dailyCarbsGoal, value);
        }

        /// <summary>Gets or sets the daily fat goal in grams.</summary>
        public double DailyFatsGoal
        {
            get => dailyFatsGoal;
            set => SetProperty(ref dailyFatsGoal, value);
        }

        /// <summary>Gets the weekly calorie goal.</summary>
        public double WeeklyCaloriesGoal => DailyCaloriesGoal * Constants.DaysPerWeek;

        /// <summary>Gets the weekly protein goal in grams.</summary>
        public double WeeklyProteinGoal => DailyProteinGoal * Constants.DaysPerWeek;

        /// <summary>Gets the weekly carbohydrate goal in grams.</summary>
        public double WeeklyCarbsGoal => DailyCarbsGoal * Constants.DaysPerWeek;

        /// <summary>Gets the weekly fat goal in grams.</summary>
        public double WeeklyFatsGoal => DailyFatsGoal * Constants.DaysPerWeek;

        /// <summary>Gets or sets the meal search text used for autocomplete filtering.</summary>
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

        /// <summary>Gets or sets the currently selected meal for logging.</summary>
        public Meal? SelectedMeal
        {
            get => selectedMeal;
            set => SetProperty(ref selectedMeal, value);
        }

        /// <summary>Gets or sets the status message for the log meal operation.</summary>
        public string LogMealStatusMessage
        {
            get => logMealStatusMessage;
            set => SetProperty(ref logMealStatusMessage, value);
        }

        /// <summary>Loads all meals into the autocomplete list.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadMealsForAutocompleteAsync()
        {
            var meals = await service.GetMealsForAutocompleteAsync();
            availableMeals = new ObservableCollection<Meal>(meals);
            UpdateFilteredMeals();
        }

        /// <summary>Logs the currently selected meal to the daily log.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LogSelectedMealAsync()
        {
            if (SelectedMeal == null)
            {
                LogMealStatusMessage = Constants.NoMealSelectedMessage;
                return;
            }

            await service.LogMealAsync(SelectedMeal);
            LogMealStatusMessage = string.Format(Constants.LoggedMealFormat, SelectedMeal.Name);

            MealSearchText = Constants.Empty;
            SelectedMeal = null;

            await LoadAsync();
        }

        private void UpdateFilteredMeals()
        {
            filteredMeals.Clear();
            var query = MealSearchText?.Trim() ?? Constants.Empty;

            var filtered = string.IsNullOrWhiteSpace(query)
                ? availableMeals
                : new ObservableCollection<Meal>(
                    availableMeals.Where(m =>
                        m.Name.Contains(query, Constants.CaseInsensitiveComparison)));

            foreach (var meal in filtered)
            {
                filteredMeals.Add(meal);
            }

            LogMealStatusMessage = !string.IsNullOrWhiteSpace(query) && filteredMeals.Count == 0
                ? Constants.NoMealsFoundMessage
                : Constants.Empty;
        }

        /// <summary>Loads all daily and weekly nutrition data for the current user.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadAsync()
        {
            if (!await service.HasAnyLogsAsync())
            {
                HasData = false;
                StatusMessage = Constants.NoDataMessage;
                return;
            }

            HasData = true;
            StatusMessage = Constants.Empty;

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

            dailyCaloriesText = BuildMetricText(DailyTotals.Calories, DailyCaloriesGoal, Constants.CaloriesUnit);
            dailyProteinText = BuildMetricText(DailyTotals.Protein, DailyProteinGoal, Constants.GramsUnit);
            dailyCarbsText = BuildMetricText(DailyTotals.Carbs, DailyCarbsGoal, Constants.GramsUnit);
            dailyFatsText = BuildMetricText(DailyTotals.Fats, DailyFatsGoal, Constants.GramsUnit);

            var burnedCalories = await service.GetTodayBurnedCaloriesAsync();
            dailyBurnedCaloriesText = string.Format(Constants.BurnedCaloriesFormat, burnedCalories);

            weeklyCaloriesText = BuildMetricText(WeeklyTotals.Calories, WeeklyCaloriesGoal, Constants.CaloriesUnit);
            weeklyProteinText = BuildMetricText(WeeklyTotals.Protein, WeeklyProteinGoal, Constants.GramsUnit);
            weeklyCarbsText = BuildMetricText(WeeklyTotals.Carbs, WeeklyCarbsGoal, Constants.GramsUnit);
            weeklyFatsText = BuildMetricText(WeeklyTotals.Fats, WeeklyFatsGoal, Constants.GramsUnit);
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
