using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TeamNut.ViewModels;
using TeamNut.Models;

namespace TeamNut.Views.ShoppingListView
{
    public sealed partial class ShoppingListPage : Page
    {
        public ShoppingListViewModel ViewModel { get; } = new ShoppingListViewModel();

        public ShoppingListPage()
        {
            this.InitializeComponent();
            
            this.Name = "RootPage";
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var text = IngredientSearchBox.Text;
            if (!string.IsNullOrWhiteSpace(text) && text != "no matching ingredients found")
            {
                await ViewModel.AddItem(text).ConfigureAwait(true);
                IngredientSearchBox.Text = string.Empty;
                IngredientSearchBox.ItemsSource = null;
            }
        }

        private async void IngredientSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 2)
                {
                    var results = await ViewModel.SearchIngredientsAsync(sender.Text);
                    if (results.Count == 0)
                    {
                        sender.ItemsSource = new System.Collections.Generic.List<string> { "no matching ingredients found" };
                    }
                    else
                    {
                        sender.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(results, r => r.Value));
                    }
                }
                else
                {
                     sender.ItemsSource = null;
                }
            }
        }

        private void IngredientSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedName = args.SelectedItem?.ToString();
            if (selectedName == null) return;
            if (selectedName == "no matching ingredients found")
            {
                sender.Text = "";
                return;
            }
            sender.Text = selectedName;
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ShoppingItem item)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm Pantry Transfer",
                    Content = "Are you sure you want to remove this item and add it to your pantry?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    ViewModel.MoveToPantryCommand.Execute(item);
                }
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.DataContext is ShoppingItem item)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm Deletion",
                    Content = "Are you sure you want to remove this item from the shopping list?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    ViewModel.RemoveItemCommand.Execute(item);
                }
            }
        }
    }
}
