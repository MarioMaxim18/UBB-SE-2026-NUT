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
            public const double DefaultDailyCarbohydratesGoal = 250;
            public const double DefaultDailyFatsGoal = 70;
            public const int DaysPerWeek = 7;

            public const string CaloriesUnit = "kcal";
            public const string GramsUnit = "g";

            public const string LoggedMealFormat = "Logged {0}.";
            public const string NoMealSelectedMessage = "Select a meal first.";
            public const string NoMealsFoundMessage = "No meals found.";
            public const string NoDataMessage = "You need to have had at least one consumed meal.";
            public const string Empty = "";
        }

        private readonly IDailyLogService service;
        private readonly IFormattingService formattingService;
        private readonly IFilteringService filteringService;

        private bool hasData;
        private string statusMessage = Constants.Empty;
        private DailyLog dailyTotals = new ();
        private DailyLog weeklyTotals = new ();

        private double dailyCaloriesGoal = Constants.DefaultDailyCaloriesGoal;
        private double dailyProteinGoal = Constants.DefaultDailyProteinGoal;
        private double dailyCarbohydratesGoal = Constants.DefaultDailyCarbohydratesGoal;
        private double dailyFatsGoal = Constants.DefaultDailyFatsGoal;

        private string dailyCaloriesText = Constants.Empty;
        private string dailyProteinText = Constants.Empty;
        private string dailyCarbohydratesText = Constants.Empty;
        private string dailyFatsText = Constants.Empty;

        private string weeklyCaloriesText = Constants.Empty;
        private string weeklyProteinText = Constants.Empty;
        private string weeklyCarbohydratesText = Constants.Empty;
        private string weeklyFatsText = Constants.Empty;

        private string dailyBurnedCaloriesText = Constants.Empty;
        private string mealSearchText = Constants.Empty;
        private string logMealStatusMessage = Constants.Empty;

        private ObservableCollection<Meal> availableMeals = new ();
        private ObservableCollection<Meal> filteredMeals = new ();
        private Meal? selectedMeal;

        public DailyLogViewModel(
            IDailyLogService dailyLogService,
            IFormattingService formattingService,
            IFilteringService filteringService)
        {
            this.service = dailyLogService;
            this.formattingService = formattingService;
            this.filteringService = filteringService;

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
            set
            {
                if (SetProperty(ref dailyCaloriesGoal, value))
                {
                    OnPropertyChanged(nameof(WeeklyCaloriesGoal));
                }
            }
        }

        public double DailyProteinGoal
        {
            get => dailyProteinGoal;
            set
            {
                if (SetProperty(ref dailyProteinGoal, value))
                {
                    OnPropertyChanged(nameof(WeeklyProteinGoal));
                }
            }
        }

        public double DailyCarbohydratesGoal
        {
            get => dailyCarbohydratesGoal;
            set
            {
                if (SetProperty(ref dailyCarbohydratesGoal, value))
                {
                    OnPropertyChanged(nameof(WeeklyCarbohydratesGoal));
                }
            }
        }

        public double DailyFatsGoal
        {
            get => dailyFatsGoal;
            set
            {
                if (SetProperty(ref dailyFatsGoal, value))
                {
                    OnPropertyChanged(nameof(WeeklyFatsGoal));
                }
            }
        }

        public double WeeklyCaloriesGoal => DailyCaloriesGoal * Constants.DaysPerWeek;
        public double WeeklyProteinGoal => DailyProteinGoal * Constants.DaysPerWeek;
        public double WeeklyCarbohydratesGoal => DailyCarbohydratesGoal * Constants.DaysPerWeek;
        public double WeeklyFatsGoal => DailyFatsGoal * Constants.DaysPerWeek;

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

        public string DailyCarbohydratesText
        {
            get => dailyCarbohydratesText;
            set => SetProperty(ref dailyCarbohydratesText, value);
        }

        public string DailyFatsText
        {
            get => dailyFatsText;
            set => SetProperty(ref dailyFatsText, value);
        }

        public string DailyBurnedCaloriesText
        {
            get => dailyBurnedCaloriesText;
            set => SetProperty(ref dailyBurnedCaloriesText, value);
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

        public string WeeklyCarbohydratesText
        {
            get => weeklyCarbohydratesText;
            set => SetProperty(ref weeklyCarbohydratesText, value);
        }

        public string WeeklyFatsText
        {
            get => weeklyFatsText;
            set => SetProperty(ref weeklyFatsText, value);
        }

        public ObservableCollection<Meal> FilteredMeals => filteredMeals;

        public async Task LoadMealsForAutocompleteAsync()
        {
            var meals = await service.GetMealsForAutocompleteAsync();
            availableMeals = new ObservableCollection<Meal>(meals);
            UpdateFilteredMeals();
        }

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
            var filtered = filteringService.FilterMeals(availableMeals, query);

            foreach (var meal in filtered)
            {
                filteredMeals.Add(meal);
            }

            LogMealStatusMessage = !string.IsNullOrWhiteSpace(query) && filteredMeals.Count == 0
                ? Constants.NoMealsFoundMessage
                : Constants.Empty;
        }

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
                DailyCarbohydratesGoal = userData.CarbohydrateNeeds > 0 ? userData.CarbohydrateNeeds : DailyCarbohydratesGoal;
                DailyFatsGoal = userData.FatNeeds > 0 ? userData.FatNeeds : DailyFatsGoal;
            }

            DailyTotals = await service.GetTodayTotalsAsync();
            WeeklyTotals = await service.GetCurrentWeekTotalsAsync();

            DailyCaloriesText = formattingService.FormatMetricWithGoal(DailyTotals.Calories, DailyCaloriesGoal, Constants.CaloriesUnit);
            DailyProteinText = formattingService.FormatMetricWithGoal(DailyTotals.Protein, DailyProteinGoal, Constants.GramsUnit);
            DailyCarbohydratesText = formattingService.FormatMetricWithGoal(DailyTotals.Carbohydrates, DailyCarbohydratesGoal, Constants.GramsUnit);
            DailyFatsText = formattingService.FormatMetricWithGoal(DailyTotals.Fats, DailyFatsGoal, Constants.GramsUnit);

            var burnedCalories = await service.GetTodayBurnedCaloriesAsync();
            DailyBurnedCaloriesText = formattingService.FormatBurnedCalories(burnedCalories);

            WeeklyCaloriesText = formattingService.FormatMetricWithGoal(WeeklyTotals.Calories, WeeklyCaloriesGoal, Constants.CaloriesUnit);
            WeeklyProteinText = formattingService.FormatMetricWithGoal(WeeklyTotals.Protein, WeeklyProteinGoal, Constants.GramsUnit);
            WeeklyCarbohydratesText = formattingService.FormatMetricWithGoal(WeeklyTotals.Carbohydrates, WeeklyCarbohydratesGoal, Constants.GramsUnit);
            WeeklyFatsText = formattingService.FormatMetricWithGoal(WeeklyTotals.Fats, WeeklyFatsGoal, Constants.GramsUnit);
        }
    }
}
