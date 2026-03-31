using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamNut.Models;
using TeamNut.Services;

namespace TeamNut.ViewModels
{
    public partial class CalorieLoggingViewModel : ObservableObject
    {
        private readonly CalorieLogService _service;

        public CalorieLoggingViewModel()
        {
            _service = new CalorieLogService();
        }

        [ObservableProperty]
        private bool _hasData;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private CalorieLog _dailyLog = new();

        [ObservableProperty]
        private CalorieLog _weeklyLog = new();

        public async Task Load(DateTime mealPlanDate)
        {
            if (!_service.HasDayPassed(mealPlanDate))
            {
                HasData = false;
                StatusMessage = "Not enough information to display";
                return;
            }

            HasData = true;

            DailyLog = await _service.GetDailyLog();
            WeeklyLog = await _service.GetWeeklyTotals();
        }
    }
}