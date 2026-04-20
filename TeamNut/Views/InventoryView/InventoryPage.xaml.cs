using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TeamNut.Models;
using TeamNut.ViewModels;

namespace TeamNut.Views.InventoryView
{
    public sealed partial class InventoryPage : Page
    {
        public InventoryViewModel ViewModel { get; } = new InventoryViewModel(Models.UserSession.UserId ?? 0);

        public InventoryPage()
        {
            this.InitializeComponent();
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
