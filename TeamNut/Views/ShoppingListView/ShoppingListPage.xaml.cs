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
            
            // Helpful to assign Name to root so bindings inside DataTemplates can reach ViewModel
            this.Name = "RootPage";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewItem();
        }

        private void NewItemTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                AddNewItem();
            }
        }

        private void AddNewItem()
        {
            var text = NewItemTextBox.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                ViewModel.AddItem(text);
                NewItemTextBox.Text = string.Empty;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Alternative to Command binding, we can just grab the bound item directly
            if (sender is Button btn && btn.DataContext is ShoppingItem item)
            {
                ViewModel.DeleteItem(item);
            }
        }
    }
}
