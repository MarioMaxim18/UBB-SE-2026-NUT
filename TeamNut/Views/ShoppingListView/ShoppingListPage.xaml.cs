namespace TeamNut.Views.ShoppingListView
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using TeamNut.Models;
    using TeamNut.ViewModels;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// ShoppingListPage.
    /// </summary>
    public sealed partial class ShoppingListPage : Page
    {
        private const string RootPageName = "RootPage";

        private const int MinSearchLength = 3;

        private const string NoMatchingIngredientsText = "no matching ingredients found";

        private const string ButtonYes = "Yes";

        private const string ButtonCancel = "Cancel";

        private const string TitleConfirmPantryTransfer = "Confirm Pantry Transfer";

        private const string TitleConfirmDeletion = "Confirm Deletion";

        private const string MsgConfirmPantryTransfer = "Are you sure you want to remove this item and add it to your pantry?";

        private const string MsgConfirmDeletion = "Are you sure you want to remove this item from the shopping list?";

        public ShoppingListViewModel ViewModel { get; }

        public ShoppingListPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<ShoppingListViewModel>();
            this.Name = RootPageName;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var text = this.IngredientSearchBox.Text;

            if (!string.IsNullOrWhiteSpace(text) && text != NoMatchingIngredientsText)
            {
                await this.ViewModel.AddItem(text).ConfigureAwait(true);
                this.IngredientSearchBox.Text = string.Empty;
                this.IngredientSearchBox.ItemsSource = null;
            }
        }

        private async void IngredientSearchBox_TextChanged(
            AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            if (sender.Text.Length >= MinSearchLength)
            {
                var results = await this.ViewModel.SearchIngredientsAsync(sender.Text);

                sender.ItemsSource = results.Count == 0
                    ? new System.Collections.Generic.List<string>
                    {
                        NoMatchingIngredientsText,
                    }
                    : System.Linq.Enumerable.ToList(
                        System.Linq.Enumerable.Select(results, r => r.Value));
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void IngredientSearchBox_SuggestionChosen(
            AutoSuggestBox sender,
            AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedName = args.SelectedItem.ToString();

            if (selectedName == NoMatchingIngredientsText)
            {
                sender.Text = string.Empty;
                return;
            }

            sender.Text = selectedName;
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: ShoppingItem item })
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = TitleConfirmPantryTransfer,
                Content = MsgConfirmPantryTransfer,
                PrimaryButtonText = ButtonYes,
                CloseButtonText = ButtonCancel,
                XamlRoot = this.XamlRoot,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                this.ViewModel.MoveToPantryCommand.Execute(item);
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: ShoppingItem item })
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = TitleConfirmDeletion,
                Content = MsgConfirmDeletion,
                PrimaryButtonText = ButtonYes,
                CloseButtonText = ButtonCancel,
                XamlRoot = this.XamlRoot,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                this.ViewModel.RemoveItemCommand.Execute(item);
            }
        }
    }
}
