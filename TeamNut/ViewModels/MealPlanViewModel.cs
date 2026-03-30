using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ModelViews
{
    public partial class MealPlanViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string statusMessage { get; set; } = "Ready to create your plan!";

        [ObservableProperty]
        public partial bool isBusy { get; set; }

        public MealPlanViewModel()
        {
            // Constructor left empty for now as we don't have specific services yet
        }

        [RelayCommand]
        private async void OnGenerateMealPlan()
        {
            StatusMessage = string.Empty;
            IsBusy = true;

            // Simple feedback for the user
            StatusMessage = "Analyzing pantry items... please wait.";

            try
            {
                // Simulate processing time for generation logic
                await Task.Delay(2000);
                StatusMessage = "Meal plan generated successfully based on your pantry!";
            }
            catch (Exception ex)
            {
                StatusMessage = "An error occurred while generating the plan.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}