using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.Models;
using TeamNut.ViewModels;

namespace TeamNut.Views.InventoryView
{
    /// <summary>Page for viewing and managing the user's food inventory.</summary>
    public sealed partial class InventoryPage : Page
    {
        /// <summary>Gets the view model.</summary>
        public InventoryViewModel ViewModel { get; }

        /// <summary>Initializes a new instance of the <see cref="InventoryPage"/> class.</summary>
        public InventoryPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<InventoryViewModel>();
            this.DataContext = ViewModel;
        }

        /// <summary>Loads inventory data when the page is navigated to.</summary>
        /// <param name="e">Navigation event arguments.</param>
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
