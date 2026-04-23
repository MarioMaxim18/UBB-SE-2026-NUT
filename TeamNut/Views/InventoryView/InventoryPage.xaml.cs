using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.Models;
using TeamNut.ViewModels;

namespace TeamNut.Views.InventoryView
{
    /// <summary>
    /// InventoryPage.
    /// </summary>
    public sealed partial class InventoryPage : Page
    {
        public InventoryViewModel ViewModel { get; }

        public InventoryPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<InventoryViewModel>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = ViewModel.LoadInventoryAsync();
        }

        private void IngredientSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Ingredient ingredient)
            {
                ViewModel.SelectedIngredient = ingredient;
            }
        }
    }
}
