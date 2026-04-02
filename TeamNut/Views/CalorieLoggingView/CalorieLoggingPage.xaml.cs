using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamNut.ViewModels;

namespace TeamNut.Views.CalorieLoggingView
{
    public sealed partial class CalorieLoggingPage : Page
    {
        private readonly DailyLogViewModel _viewModel;

        public CalorieLoggingPage()
        {
            this.InitializeComponent();

            _viewModel = new DailyLogViewModel();
            this.DataContext = _viewModel;

            LoadData();
        }

        private async void LoadData()
        {
            await _viewModel.LoadAsync();
        }
    }
}
