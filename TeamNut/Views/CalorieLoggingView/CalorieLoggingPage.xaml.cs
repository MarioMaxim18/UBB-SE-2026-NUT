using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;

namespace TeamNut.Views.CalorieLoggingView
{
    public sealed partial class CalorieLoggingPage : Page
    {
        private readonly CalorieLoggingViewModel _viewModel;

        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            _viewModel = new CalorieLoggingViewModel();
            this.DataContext = _viewModel;

            LoadData();
        }

        private async void LoadData()
        {
            await _viewModel.Load(System.DateTime.Now.AddDays(-1));
        }
    }
}