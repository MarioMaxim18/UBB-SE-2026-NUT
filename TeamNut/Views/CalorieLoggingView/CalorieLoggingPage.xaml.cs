using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.Models;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut.Views.CalorieLoggingView
{
    /// <summary>
    /// CalorieLoggingPage.
    /// </summary>
    public sealed partial class CalorieLoggingPage : Page
    {
        private DailyLogViewModel ViewModel { get; }

        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<DailyLogViewModel>();
            this.DataContext = ViewModel;

            LoadData();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadAsync();
        }

        internal async System.Threading.Tasks.Task RefreshAsync()
        {
            await ViewModel.LoadAsync();
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
