using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        private readonly MealPlanRepository _mealPlanRepository;

        [ObservableProperty]
        public partial string StatusMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        private ObservableCollection<Meal> generatedMeals = new();

        public MealPlanViewModel()
        {
            _mealPlanRepository = new MealPlanRepository();
        }

        [RelayCommand]
        private async Task GenerateMealPlan()
        {
            StatusMessage = string.Empty;
            IsBusy = true;
            GeneratedMeals.Clear();

            try
            {
                StatusMessage = "Generating your personalized daily meal plan...";

                int userId = 1;

                int mealPlanId = await _mealPlanRepository.GeneratePersonalizedDailyMealPlan(userId);

                var meals = await _mealPlanRepository.GetMealsForMealPlan(mealPlanId);

                foreach (var meal in meals)
                {
                    GeneratedMeals.Add(meal);
                }

                StatusMessage = $"Meal plan generated! {meals.Count} meals added.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}