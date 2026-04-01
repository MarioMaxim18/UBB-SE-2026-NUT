using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class ShoppingItem : ObservableObject
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isChecked;
    }
}
