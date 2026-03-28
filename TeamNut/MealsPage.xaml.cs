using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TeamNut.ViewModels;
using TeamNut.Models;

namespace TeamNut
{
    public sealed partial class MealsPage : Page
    {
        private MealSearchViewModel viewModel;

        bool filterVegan = false;
        bool filterKeto = false;
        bool filterGluten = false;
        bool filterLactose = false;
        bool filterNuts = false;
        bool filterFavorites = false;

        public MealsPage()
        {
            this.InitializeComponent();
            viewModel = new MealSearchViewModel();
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var meal = button.DataContext as Meal;

            if (meal != null)
            {
                viewModel.ToggleFavorite(meal);
            }


            btnSearch_Click(this, new RoutedEventArgs());
        }
        
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var results = viewModel.SearchMeals(
                txtSearch.Text,
                filterVegan,
                filterKeto,
                filterGluten,
                filterLactose,
                filterNuts,
                filterFavorites 
            );

            listMeals.ItemsSource = results;
        }
        private void txtSearch_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        
        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item)
            {
                var selected = item.Content?.ToString();

                // reset all filters, then set the selected one
                filterVegan = filterKeto = filterGluten = filterLactose = filterNuts = filterFavorites = false;

                switch (selected)
                {
                    case "Vegan":
                        filterVegan = true;
                        break;
                    case "Keto":
                        filterKeto = true;
                        break;
                    case "Gluten Free":
                        filterGluten = true;
                        break;
                    case "Lactose Free":
                        filterLactose = true;
                        break;
                    case "No Nuts":
                        filterNuts = true;
                        break;
                    case "Favorites":
                        filterFavorites = true;
                        break;
                    case "All":
                    default:
                        // all false -> show everything
                        break;
                }

                // refresh results
                btnSearch_Click(null, null);
            }
        }

        private async void OpenFilters_Click(object sender, RoutedEventArgs e)
        {
            CheckBox vegan = new CheckBox { Content = "Vegan", IsChecked = filterVegan };
            CheckBox keto = new CheckBox { Content = "Keto", IsChecked = filterKeto };
            CheckBox gluten = new CheckBox { Content = "Gluten Free", IsChecked = filterGluten };
            CheckBox lactose = new CheckBox { Content = "Lactose Free", IsChecked = filterLactose };
            CheckBox nuts = new CheckBox { Content = "No Nuts", IsChecked = filterNuts };
            CheckBox favorites = new CheckBox { Content = "Favorites Only", IsChecked = filterFavorites };

            StackPanel panel = new StackPanel();
            panel.Children.Add(vegan);
            panel.Children.Add(keto);
            panel.Children.Add(gluten);
            panel.Children.Add(lactose);
            panel.Children.Add(nuts);
            panel.Children.Add(favorites); // 🔥 agora correto

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Filters",
                Content = panel,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                filterVegan = vegan.IsChecked == true;
                filterKeto = keto.IsChecked == true;
                filterGluten = gluten.IsChecked == true;
                filterLactose = lactose.IsChecked == true;
                filterNuts = nuts.IsChecked == true;
                filterFavorites = favorites.IsChecked == true; // 🔥 importante
            }
        }
    }
}