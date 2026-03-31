using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Services;

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
        private ObservableCollection<Meal> generatedMeals = new ObservableCollection<Meal>();

        public MealPlanViewModel()
        {
            _mealPlanRepository = new MealPlanRepository();
        }

        [RelayCommand]
        private async void OnGenerateMealPlan()
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

                StatusMessage = $"Personalized meal plan generated successfully! {meals.Count} meals added based on your nutritional needs.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while generating the plan: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}