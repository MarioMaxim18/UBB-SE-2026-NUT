using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TeamNut.Models
{
    public partial class UserData : ObservableValidator
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int UserId { get; set; }

        [ObservableProperty]
        [Range(1, 500, ErrorMessage = "Weight must be a positive whole number, between 1 and 500")]
        public partial int Weight { get; set; }

        [ObservableProperty]
        [Range(1, 300, ErrorMessage = "Height must be a positive whole number, between 1 and 300")]
        public partial int Height { get; set; }

        [ObservableProperty]
        public partial int Age { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = "Please select a gender")]
        [RegularExpression(@"^(male|female)$", ErrorMessage = "Gender must be 'male' or 'female'")]
        public partial string Gender { get; set; } = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Please select a goal")]
        [RegularExpression(@"^(bulk|cut|maintenance|well-being)$", ErrorMessage = "Select a valid goal")]
        public partial string Goal { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double Bmi { get; set; }

        [ObservableProperty]
        public partial int CalorieNeeds { get; set; }

        [ObservableProperty]
        public partial int ProteinNeeds { get; set; }

        [ObservableProperty]
        public partial int CarbNeeds { get; set; }

        [ObservableProperty]
        public partial int FatNeeds { get; set; }

        public List<string> GetValidationErrors()
        {
            ValidateAllProperties();
            return GetErrors().Select(e => e.ErrorMessage!).Where(m => m != null).ToList();
        }

        public int CalculateAge(DateTimeOffset? birthDate)
        {
            if (birthDate == null) return 0;

            var today = DateTime.Today;
            var birth = birthDate.Value.DateTime;
            var age = today.Year - birth.Year;
            if (birth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public int CalculateBmi()
        {
            if (Height <= 0 || Weight <= 0) return 0;

            double heightInMeters = Height / 100.0;
            double bmi = Weight / (heightInMeters * heightInMeters);
            return (int)Math.Round(bmi);
        }

        public int CalculateCalorieNeeds()
        {
            if (Weight <= 0 || Height <= 0 || Age <= 0) return 0;

            double bmr;
            if (Gender.Equals("male", StringComparison.OrdinalIgnoreCase))
            {
                bmr = (10 * Weight) + (6.25 * Height) - (5 * Age) + 5;
            }
            else if (Gender.Equals("female", StringComparison.OrdinalIgnoreCase))
            {
                bmr = (10 * Weight) + (6.25 * Height) - (5 * Age) - 161;
            }
            else
            {
                return 0;
            }

            double tdee = bmr * 1.55;

            double adjustedCalories = Goal.ToLower() switch
            {
                "bulk" => tdee + 300,
                "cut" => tdee - 300,
                "maintenance" => tdee,
                "well-being" => tdee,
                _ => tdee
            };

            return (int)Math.Round(adjustedCalories);
        }

        public int CalculateProteinNeeds()
        {
            if (Weight <= 0) return 0;

            double proteinPerKg = Goal.ToLower() switch
            {
                "bulk" => 2.0,
                "cut" => 2.2,
                "maintenance" => 1.8,
                "well-being" => 1.6,
                _ => 1.8
            };

            return (int)Math.Round(Weight * proteinPerKg);
        }

        public int CalculateFatNeeds()
        {
            int calories = CalculateCalorieNeeds();
            if (calories <= 0) return 0;

            double fatPercentage = Goal.ToLower() switch
            {
                "bulk" => 0.25,
                "cut" => 0.25,
                "maintenance" => 0.28,
                "well-being" => 0.30,
                _ => 0.25
            };

            double fatCalories = calories * fatPercentage;
            return (int)Math.Round(fatCalories / 9);
        }

        public int CalculateCarbNeeds()
        {
            int calories = CalculateCalorieNeeds();
            int protein = CalculateProteinNeeds();
            int fat = CalculateFatNeeds();

            if (calories <= 0) return 0;

            int proteinCalories = protein * 4;
            int fatCalories = fat * 9;
            int carbCalories = calories - proteinCalories - fatCalories;

            return (int)Math.Round(carbCalories / 4.0);
        }
    }
}
