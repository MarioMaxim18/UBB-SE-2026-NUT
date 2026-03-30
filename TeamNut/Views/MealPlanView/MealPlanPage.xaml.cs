using Microsoft.UI.Xaml.Controls;
using TeamNut.ModelViews;

namespace TeamNut.Views.MealPlanView
{
    public sealed partial class MealPlanPage : Page
    {
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();

        public MealPlanPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}