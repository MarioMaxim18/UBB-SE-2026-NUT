using Microsoft.UI.Xaml.Controls;
using TeamNut.ModelViews; // English: Ensure this points to your ModelViews folder

namespace TeamNut.Views.MealPlanView
{
    public sealed partial class MealPlanPage : Page
    {
        // CRITICAL: This MUST be public for x:Bind to work!
        public MealPlanViewModel ViewModel { get; } = new MealPlanViewModel();

        public MealPlanPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}