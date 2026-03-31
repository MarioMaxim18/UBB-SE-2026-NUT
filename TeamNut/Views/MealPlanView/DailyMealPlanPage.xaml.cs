using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using TeamNut.ModelViews;

namespace TeamNut.Views.MealPlanView
{
    public sealed partial class DailyMealPlanPage : Page
    {
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();

        public DailyMealPlanPage()
        {
            this.InitializeComponent();
            DateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Load today's meal plan when navigating to this page
            ViewModel.LoadTodaysMealPlan();
        }
    }
}
