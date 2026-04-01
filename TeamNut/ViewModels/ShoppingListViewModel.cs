using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TeamNut.ViewModels
{
    public partial class ShoppingItem : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isChecked;
    }

    public partial class ShoppingListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ShoppingItem> items = new ObservableCollection<ShoppingItem>();

        public ShoppingListViewModel()
        {
            // Adding mock data for the UI
            Items.Add(new ShoppingItem { Name = "Whole Wheat Bread", IsChecked = false });
            Items.Add(new ShoppingItem { Name = "Almond Milk", IsChecked = true });
            Items.Add(new ShoppingItem { Name = "Eggs", IsChecked = false });
            Items.Add(new ShoppingItem { Name = "Chicken Breast", IsChecked = false });
            Items.Add(new ShoppingItem { Name = "Spinach", IsChecked = false });
        }

        [RelayCommand]
        public void AddItem(string itemName)
        {
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                Items.Add(new ShoppingItem { Name = itemName.Trim(), IsChecked = false });
            }
        }

        [RelayCommand]
        public void DeleteItem(ShoppingItem item)
        {
            if (item != null && Items.Contains(item))
            {
                Items.Remove(item);
            }
        }
    }
}
