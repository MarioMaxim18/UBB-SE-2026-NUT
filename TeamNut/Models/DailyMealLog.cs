using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class DailyMealLog : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        private int mealId;

        [ObservableProperty]
        private string mealName;

        [ObservableProperty]
        private DateTime logDate;

        [ObservableProperty]
        private int calories;

        [ObservableProperty]
        private int protein;

        [ObservableProperty]
        private int carbs;

        [ObservableProperty]
        private int fat;

        [ObservableProperty]
        private string mealType;
    }
}
