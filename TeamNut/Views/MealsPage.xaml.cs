namespace TeamNut
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using TeamNut.Models;
    using TeamNut.ViewModels;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// MealsPage.
    /// </summary>
    public sealed partial class MealsPage : Page
    {
        private MealSearchViewModel ViewModel { get; }

        private const string FavoriteOnSymbol = "★";

        private const string FavoriteOffSymbol = "☆";

        private const string ButtonClose = "Close";

        private const string LabelCalories = "Calories";

        private const string LabelProtein = "Protein";

        private const string LabelCarbs = "Carbs";

        private const string LabelFat = "Fat";

        private const string UnitGrams = "g";

        private const int DetailsPanelSpacing = 10;

        private const int MealImageHeight = 150;

        private const string LineBreak = "\n";

        private const string DoubleLineBreak = "\n\n";

        public MealsPage()
        {
            InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<MealSearchViewModel>();
            DataContext = this.ViewModel;

            Loaded += (s, e) => this.BtnSearch_Click(this, new RoutedEventArgs());
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var filter = new MealFilter
            {
                SearchTerm = txtSearch.Text ?? string.Empty,
                IsVegan = chkVegan?.IsChecked == true,
                IsKeto = chkKeto?.IsChecked == true,
                IsGlutenFree = chkGlutenFree?.IsChecked == true,
                IsLactoseFree = chkLactoseFree?.IsChecked == true,
                IsNutFree = chkNutFree?.IsChecked == true,
                IsFavoriteOnly = chkFavorites?.IsChecked == true
            };

            var results = await this.ViewModel.SearchMealsAsync(filter);
            this.ViewModel.SetAllMeals(results);
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.DataContext is not Meal meal)
            {
                return;
            }

            meal.IsFavorite = !meal.IsFavorite;
            btn.Content = meal.IsFavorite ? FavoriteOnSymbol : FavoriteOffSymbol;

            await this.ViewModel.ToggleFavoriteAsync(meal);

            if (chkFavorites?.IsChecked == true && !meal.IsFavorite)
            {
                this.BtnSearch_Click(this, new RoutedEventArgs());
            }
        }

        private async void ListMeals_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Meal meal)
            {
                return;
            }

            var ingredientsText =
                await this.ViewModel.GetMealIngredientsTextAsync(meal.Id);

            var panel = new StackPanel { Spacing = DetailsPanelSpacing };

            if (!string.IsNullOrEmpty(meal.ImageUrl))
            {
                panel.Children.Add(new Image
                {
                    Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                        new Uri(meal.ImageUrl)),
                    Height = MealImageHeight
                });
            }

            panel.Children.Add(new TextBlock
            {
                Text =
                    $"{LabelCalories}: {meal.Calories}{LineBreak}" +
                    $"{LabelProtein}: {meal.Protein}{UnitGrams}{LineBreak}" +
                    $"{LabelCarbs}: {meal.Carbs}{UnitGrams}{LineBreak}" +
                    $"{LabelFat}: {meal.Fat}{UnitGrams}" +
                    $"{DoubleLineBreak}Ingredients:{LineBreak}" +
                    ingredientsText
            });

            var dialog = new ContentDialog
            {
                Title = meal.Name,
                Content = panel,
                CloseButtonText = ButtonClose,
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void Favorite_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Meal meal)
            {
                btn.Content = meal.IsFavorite
                    ? FavoriteOnSymbol
                    : FavoriteOffSymbol;
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.GoToPreviousPage();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.GoToNextPage();
        }

        private void TxtSearch_KeyDown(
            object sender,
            Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.BtnSearch_Click(this, new RoutedEventArgs());
            }
        }
    }
}
