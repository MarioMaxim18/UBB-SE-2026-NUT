using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TeamNut.Models
{
    public partial class UserData : ObservableValidator
    {
        private const int MinWeightKg = 1;
        private const int MaxWeightKg = 500;
        private const int MinHeightCm = 1;
        private const int MaxHeightCm = 300;
        private const int MaxNameLength = 50;
        private const string ErrorWeightRange = "Weight must be a positive whole number, between 1 and 500";
        private const string ErrorHeightRange = "Height must be a positive whole number, between 1 and 300";
        private const string ErrorGenderRequired = "Please select a gender";
        private const string ErrorGoalRequired = "Please select a goal";
        private const string ErrorGenderInvalid = "Gender must be 'male' or 'female'";
        private const string ErrorGoalInvalid = "Select a valid goal";
        private const string GenderMale = "male";
        private const string GenderFemale = "female";
        private const string GoalBulk = "bulk";
        private const string GoalCut = "cut";
        private const string GoalMaintenance = "maintenance";
        private const string GoalWellBeing = "well-being";
        private const string RegexGender = @"^(male|female)$";
        private const string RegexGoal = @"^(bulk|cut|maintenance|well-being)$";
        private const double BmrWeightFactor = 10.0;
        private const double BmrHeightFactor = 6.25;
        private const double BmrAgeFactor = 5.0;
        private const double BmrMaleOffset = 5.0;
        private const double BmrFemaleOffset = 161.0;
        private const double ActivityMultiplier = 1.55;
        private const int BulkCalorieDelta = 300;
        private const int CutCalorieDelta = -300;
        private const double ProteinBulk = 2.0;
        private const double ProteinCut = 2.2;
        private const double ProteinMaintenance = 1.8;
        private const double ProteinWellBeing = 1.6;
        private const double FatBulkCut = 0.25;
        private const double FatMaintenance = 0.28;
        private const double FatWellBeing = 0.30;
        private const int CaloriesPerGramProtein = 4;
        private const int CaloriesPerGramCarbs = 4;
        private const int CaloriesPerGramFat = 9;
        
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int UserId { get; set; }

        [ObservableProperty]
        [Range(MinWeightKg, MaxWeightKg, ErrorMessage = ErrorWeightRange)]
        public partial int Weight { get; set; }

        [ObservableProperty]
        [Range(MinHeightCm, MaxHeightCm, ErrorMessage = ErrorHeightRange)]
        public partial int Height { get; set; }

        [ObservableProperty]
        public partial int Age { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorGenderRequired)]
        [RegularExpression(RegexGender, ErrorMessage = ErrorGenderInvalid)]
        public partial string Gender { get; set; } = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorGoalRequired)]
        [RegularExpression(RegexGoal, ErrorMessage = ErrorGoalInvalid)]
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
            return GetErrors()
                .Select(e => e.ErrorMessage!)
                .Where(m => m != null)
                .ToList();
        }

        public int CalculateAge(DateTimeOffset? birthDate)
        {
            if (birthDate == null) return 0;

            var today = DateTime.Today;
            var birth = birthDate.Value.DateTime;

            int age = today.Year - birth.Year;
            if (birth.Date > today.AddYears(-age))
                age--;

            return age;
        }

        public double CalculateBmi()
        {
            if (Height <= 0 || Weight <= 0)
                return 0;

            double heightMeters = Height / 100.0;
            double bmi = Weight / (heightMeters * heightMeters);

            return (int)Math.Round(bmi);
        }

        public int CalculateCalorieNeeds()
        {
            if (Weight <= 0 || Height <= 0 || Age <= 0)
                return 0;

            double bmr =
                Gender.Equals(GenderMale, StringComparison.OrdinalIgnoreCase)
                    ? (BmrWeightFactor * Weight) +
                      (BmrHeightFactor * Height) -
                      (BmrAgeFactor * Age) +
                      BmrMaleOffset

                    : Gender.Equals(GenderFemale, StringComparison.OrdinalIgnoreCase)
                        ? (BmrWeightFactor * Weight) +
                          (BmrHeightFactor * Height) -
                          (BmrAgeFactor * Age) -
                          BmrFemaleOffset
                        : 0;

            if (bmr <= 0)
                return 0;

            double tdee = bmr * ActivityMultiplier;

            double adjustedCalories = Goal.ToLower() switch
            {
                GoalBulk => tdee + BulkCalorieDelta,
                GoalCut => tdee + CutCalorieDelta,
                GoalMaintenance => tdee,
                GoalWellBeing => tdee,
                _ => tdee
            };

            return (int)Math.Round(adjustedCalories);
        }

        public int CalculateProteinNeeds()
        {
            if (Weight <= 0) return 0;

            double proteinPerKg = Goal.ToLower() switch
            {
                GoalBulk => ProteinBulk,
                GoalCut => ProteinCut,
                GoalMaintenance => ProteinMaintenance,
                GoalWellBeing => ProteinWellBeing,
                _ => ProteinMaintenance
            };

            return (int)Math.Round(Weight * proteinPerKg);
        }

        public int CalculateFatNeeds()
        {
            int calories = CalculateCalorieNeeds();
            if (calories <= 0) return 0;

            double fatRatio = Goal.ToLower() switch
            {
                GoalBulk or GoalCut => FatBulkCut,
                GoalMaintenance => FatMaintenance,
                GoalWellBeing => FatWellBeing,
                _ => FatBulkCut
            };

            double fatCalories = calories * fatRatio;
            return (int)Math.Round(fatCalories / CaloriesPerGramFat);
        }

        public int CalculateCarbNeeds()
        {
            int calories = CalculateCalorieNeeds();
            int proteinCalories = CalculateProteinNeeds() * CaloriesPerGramProtein;
            int fatCalories = CalculateFatNeeds() * CaloriesPerGramFat;

            if (calories <= 0)
                return 0;

            int carbCalories = calories - proteinCalories - fatCalories;
            return (int)Math.Round(carbCalories / (double)CaloriesPerGramCarbs);
        }
    }
}
