using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.CalorieLoggingView
{
    public sealed partial class CalorieLoggingPage : Page
    {
        private DailyLogViewModel _viewModel { get; }

        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            _viewModel = App.Services.GetService<DailyLogViewModel>();
            this.DataContext = _viewModel;

            LoadData();
        }

        private async void LoadData()
        {
            await _viewModel.LoadAsync();
        }

        private void MealSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Meal meal)
            {
                _viewModel.SelectedMeal = meal;
            }
        }

        private async void LogMeal_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.LogSelectedMealAsync();
        }
    }
}
