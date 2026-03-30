using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;
using TeamNut.Models;

namespace TeamNut
{
    public sealed partial class MealsPage : Page
    {
        private MealSearchViewModel viewModel;

        public MealsPage()
        {
            this.InitializeComponent();
            viewModel = new MealSearchViewModel();
            btnSearch_Click(this, new RoutedEventArgs());
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var meal = button?.DataContext as Meal;

            if (meal != null)
            {
                viewModel.ToggleFavorite(meal);
            }

            btnSearch_Click(this, new RoutedEventArgs());
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var filter = new MealFilter
            {
                SearchTerm = txtSearch.Text ?? ""
            };

            var results = viewModel.SearchMeals(filter);

            listMeals.ItemsSource = results;
        }

        private void txtSearch_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnSearch_Click(this, new RoutedEventArgs());
            }
        }
    }
}