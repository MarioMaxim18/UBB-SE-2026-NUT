namespace TeamNut.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;

    /// <summary>
    /// DailyLogViewModel.
    /// </summary>
    public partial class DailyLogViewModel : ObservableObject
    {
        private static class Constants
        {
            public const double DefaultDailyCaloriesGoal = 2000;

            public const double DefaultDailyProteinGoal = 150;

            public const double DefaultDailyCarbsGoal = 250;

            public const double DefaultDailyFatsGoal = 70;

            public const int DaysPerWeek = 7;

            public const string CaloriesUnit = "kcal";

            public const string GramsUnit = "g";

            public const string LoggedMealFormat = "Logged {0}.";

            public const string NoMealSelectedMessage = "Select a meal first.";

            public const string NoMealsFoundMessage = "No meals found.";

            public const string NoDataMessage = "You need to have had atleast one consumed meal.";

            public const string Empty = "";
        }

        private readonly IDailyLogService service;

        private readonly IFormattingService formattingService;

        private readonly IFilteringService filteringService;

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

        public DailyLogViewModel(
            IDailyLogService dailyLogService,
            IFormattingService fformattingService,
            IFilteringService ffilteringService)
        {
            this.service = dailyLogService;
            this.formattingService = fformattingService;
            this.filteringService = ffilteringService;
            _ = this.LoadMealsForAutocompleteAsync();
        }

        public bool HasData
        {
            get => this.hasData;
            set => SetProperty(ref this.hasData, value);
        }

        public string StatusMessage
        {
            get => this.statusMessage;
            set => SetProperty(ref this.statusMessage, value);
        }

        public DailyLog DailyTotals
        {
            get => this.dailyTotals;
            set => SetProperty(ref this.dailyTotals, value);
        }

        public DailyLog WeeklyTotals
        {
            get => this.weeklyTotals;
            set => SetProperty(ref this.weeklyTotals, value);
        }

        public double DailyCaloriesGoal
        {
            get => this.dailyCaloriesGoal;
            set => SetProperty(ref this.dailyCaloriesGoal, value);
        }

        public double DailyProteinGoal
        {
            get => this.dailyProteinGoal;
            set => SetProperty(ref this.dailyProteinGoal, value);
        }

        public double DailyCarbsGoal
        {
            get => this.dailyCarbsGoal;
            set => SetProperty(ref this.dailyCarbsGoal, value);
        }

        public double DailyFatsGoal
        {
            get => this.dailyFatsGoal;
            set => SetProperty(ref this.dailyFatsGoal, value);
        }

        public double WeeklyCaloriesGoal => DailyCaloriesGoal * Constants.DaysPerWeek;

        public double WeeklyProteinGoal => DailyProteinGoal * Constants.DaysPerWeek;

        public double WeeklyCarbsGoal => DailyCarbsGoal * Constants.DaysPerWeek;

        public double WeeklyFatsGoal => DailyFatsGoal * Constants.DaysPerWeek;

        public string MealSearchText
        {
            get => this.mealSearchText;
            set
            {
                if (SetProperty(ref this.mealSearchText, value))
                {
                    this.UpdateFilteredMeals();
                }
            }
        }

        public Meal? SelectedMeal
        {
            get => this.selectedMeal;
            set => SetProperty(ref this.selectedMeal, value);
        }

        public string LogMealStatusMessage
        {
            get => this.logMealStatusMessage;
            set => SetProperty(ref this.logMealStatusMessage, value);
        }

        public string DailyCaloriesText
        {
            get => this.dailyCaloriesText;
            set => SetProperty(ref this.dailyCaloriesText, value);
        }

        public string DailyProteinText
        {
            get => this.dailyProteinText;
            set => SetProperty(ref this.dailyProteinText, value);
        }

        public string DailyCarbsText
        {
            get => this.dailyCarbsText;
            set => SetProperty(ref this.dailyCarbsText, value);
        }

        public string DailyFatsText
        {
            get => this.dailyFatsText;
            set => SetProperty(ref this.dailyFatsText, value);
        }

        public string DailyBurnedCaloriesText
        {
            get => this.dailyBurnedCaloriesText;
            set => SetProperty(ref this.dailyBurnedCaloriesText, value);
        }

        public string WeeklyCaloriesText
        {
            get => this.weeklyCaloriesText;
            set => SetProperty(ref this.weeklyCaloriesText, value);
        }

        public string WeeklyProteinText
        {
            get => this.weeklyProteinText;
            set => SetProperty(ref this.weeklyProteinText, value);
        }

        public string WeeklyCarbsText
        {
            get => this.weeklyCarbsText;
            set => SetProperty(ref this.weeklyCarbsText, value);
        }

        public string WeeklyFatsText
        {
            get => this.weeklyFatsText;
            set => SetProperty(ref this.weeklyFatsText, value);
        }

        public ObservableCollection<Meal> FilteredMeals => filteredMeals;

        public async Task LoadMealsForAutocompleteAsync()
        {
            var meals = await this.service.GetMealsForAutocompleteAsync();
            this.availableMeals = new ObservableCollection<Meal>(meals);
            this.UpdateFilteredMeals();
        }

        public async Task LogSelectedMealAsync()
        {
            if (SelectedMeal == null)
            {
                LogMealStatusMessage = Constants.NoMealSelectedMessage;
                return;
            }

            await this.service.LogMealAsync(SelectedMeal);
            LogMealStatusMessage = string.Format(Constants.LoggedMealFormat, SelectedMeal.Name);

            MealSearchText = Constants.Empty;
            SelectedMeal = null;

            await this.LoadAsync();
        }

        private void UpdateFilteredMeals()
        {
            this.filteredMeals.Clear();
            var query = MealSearchText?.Trim() ?? Constants.Empty;

            var filtered = this.filteringService.FilterMeals(this.availableMeals, query);

            foreach (var meal in filtered)
            {
                this.filteredMeals.Add(meal);
            }

            LogMealStatusMessage = !string.IsNullOrWhiteSpace(query) && this.filteredMeals.Count == 0
                ? Constants.NoMealsFoundMessage
                : Constants.Empty;
        }

        public async Task LoadAsync()
        {
            if (!await this.service.HasAnyLogsAsync())
            {
                HasData = false;
                StatusMessage = Constants.NoDataMessage;
                return;
            }

            HasData = true;
            StatusMessage = Constants.Empty;

            var userData = await this.service.GetCurrentUserNutritionTargetsAsync();
            if (userData != null)
            {
                DailyCaloriesGoal = userData.CalorieNeeds > 0 ? userData.CalorieNeeds : DailyCaloriesGoal;
                DailyProteinGoal = userData.ProteinNeeds > 0 ? userData.ProteinNeeds : DailyProteinGoal;
                DailyCarbsGoal = userData.CarbNeeds > 0 ? userData.CarbNeeds : DailyCarbsGoal;
                DailyFatsGoal = userData.FatNeeds > 0 ? userData.FatNeeds : DailyFatsGoal;
            }

            DailyTotals = await this.service.GetTodayTotalsAsync();
            WeeklyTotals = await this.service.GetCurrentWeekTotalsAsync();

            DailyCaloriesText = this.formattingService.FormatMetricWithGoal(DailyTotals.Calories, DailyCaloriesGoal, Constants.CaloriesUnit);
            DailyProteinText = this.formattingService.FormatMetricWithGoal(DailyTotals.Protein, DailyProteinGoal, Constants.GramsUnit);
            DailyCarbsText = this.formattingService.FormatMetricWithGoal(DailyTotals.Carbs, DailyCarbsGoal, Constants.GramsUnit);
            DailyFatsText = this.formattingService.FormatMetricWithGoal(DailyTotals.Fats, DailyFatsGoal, Constants.GramsUnit);

            var burnedCalories = await this.service.GetTodayBurnedCaloriesAsync();
            DailyBurnedCaloriesText = this.formattingService.FormatBurnedCalories(burnedCalories);

            WeeklyCaloriesText = this.formattingService.FormatMetricWithGoal(WeeklyTotals.Calories, this.WeeklyCaloriesGoal, Constants.CaloriesUnit);
            WeeklyProteinText = this.formattingService.FormatMetricWithGoal(WeeklyTotals.Protein, this.WeeklyProteinGoal, Constants.GramsUnit);
            WeeklyCarbsText = this.formattingService.FormatMetricWithGoal(WeeklyTotals.Carbs, this.WeeklyCarbsGoal, Constants.GramsUnit);
            WeeklyFatsText = this.formattingService.FormatMetricWithGoal(WeeklyTotals.Fats, this.WeeklyFatsGoal, Constants.GramsUnit);
        }
    }
}
