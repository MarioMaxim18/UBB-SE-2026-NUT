using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.CalorieLoggingView
{
    public sealed partial class CalorieLoggingPage : Page
    {
        private DailyLogViewModel ViewModel { get; }

        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetService<DailyLogViewModel>();
            this.DataContext = ViewModel;

            LoadData();
        }

        private async void LoadData()
        {
            await ViewModel.LoadAsync();
        }

        private void MealSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Meal meal)
            {
                ViewModel.SelectedMeal = meal;
            }
        }

        private async void LogMeal_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LogSelectedMealAsync();
        }
    }
}
