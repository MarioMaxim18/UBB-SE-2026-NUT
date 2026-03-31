using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamNut.Models
{
    public partial class CalorieLog : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private int _userId;

        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private double _caloriesConsumed;

        [ObservableProperty]
        private double _protein;

        [ObservableProperty]
        private double _carbs;

        [ObservableProperty]
        private double _fats;
    }
}