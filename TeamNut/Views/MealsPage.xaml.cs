using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.Models;
using TeamNut.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TeamNut
{
    /// <summary>Page for browsing and searching meals.</summary>
    public sealed partial class MealsPage : Page
    {
        private MealSearchViewModel ViewModel { get; }
        private int currentPage = DefaultStartPage;
        private List<Meal> allMeals = new List<Meal>();
        private const int DefaultStartPage = 1;
        private const int DefaultPageSize = 5;
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
        private int pageSize = DefaultPageSize;

        /// <summary>Initializes a new instance of the <see cref="MealsPage"/> class.</summary>
        public MealsPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetService<MealSearchViewModel>();

            Loaded += (s, e) => BtnSearch_Click(this, new RoutedEventArgs());
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

            var results = await ViewModel.SearchMealsAsync(filter);

            allMeals = results.ToList();
            currentPage = DefaultStartPage;
            LoadMeals();
        }

        private void LoadMeals()
        {
            if (allMeals == null)
            {
                return;
            }

            var pagedMeals = allMeals
                .Skip((currentPage - DefaultStartPage) * pageSize)
                .Take(pageSize)
                .ToList();

            listMeals.ItemsSource = null;
            listMeals.ItemsSource = pagedMeals;

            int totalPages = Math.Max(
                DefaultStartPage,
                (int)Math.Ceiling((double)allMeals.Count / pageSize));

            txtPage.Text = $"{currentPage} / {totalPages}";

            btnPrev.IsEnabled = currentPage > DefaultStartPage;
            btnNext.IsEnabled = currentPage < totalPages;
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.DataContext is not Meal meal)
            {
                return;
            }

            meal.IsFavorite = !meal.IsFavorite;
            btn.Content = meal.IsFavorite ? FavoriteOnSymbol : FavoriteOffSymbol;

            await ViewModel.ToggleFavoriteAsync(meal);

            if (chkFavorites?.IsChecked == true && !meal.IsFavorite)
            {
                allMeals.Remove(meal);
                LoadMeals();
            }
        }

        private async void ListMeals_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Meal meal)
            {
                return;
            }

            var ingredientsText =
                await ViewModel.GetMealIngredientsTextAsync(meal.Id);

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
            if (currentPage > DefaultStartPage)
            {
                currentPage--;
                LoadMeals();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            int totalPages =
                (int)Math.Ceiling((double)allMeals.Count / pageSize);

            if (currentPage < totalPages)
            {
                currentPage++;
                LoadMeals();
            }
        }

        private void TxtSearch_KeyDown(
            object sender,
            Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                BtnSearch_Click(this, new RoutedEventArgs());
            }
        }
    }
}