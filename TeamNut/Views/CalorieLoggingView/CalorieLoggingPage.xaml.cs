using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;

namespace TeamNut.Views.CalorieLoggingView
{
    /// <summary>Page for logging daily calorie intake.</summary>
    public sealed partial class CalorieLoggingPage : Page
    {
        private readonly DailyLogViewModel viewModel;

        /// <summary>Initializes a new instance of the <see cref="CalorieLoggingPage"/> class.</summary>
        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            viewModel = new DailyLogViewModel();
            this.DataContext = viewModel;

            LoadData();
        }

        private async void LoadData()
        {
            await viewModel.LoadAsync();
        }

        private void MealSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Meal meal)
            {
                viewModel.SelectedMeal = meal;
            }
        }

        private async void LogMeal_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.LogSelectedMealAsync();
        }
    }
}
