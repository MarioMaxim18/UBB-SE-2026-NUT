namespace TeamNut.Services
{
    using System;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;

    public class NutritionCalculationService : INutritionCalculationService
    {
        private const string GenderMale = "male";
        private const string GenderFemale = "female";

        private const string GoalBulk = "bulk";
        private const string GoalCut = "cut";
        private const string GoalMaintenance = "maintenance";
        private const string GoalWellBeing = "well-being";

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
        private const int CaloriesPerGramCarbohydrates = 4;
        private const int CaloriesPerGramFat = 9;

        public int CalculateAge(DateTimeOffset? birthDate)
        {
            if (birthDate == null)
            {
                return 0;
            }

            var today = DateTime.Today;
            var birth = birthDate.Value.DateTime;

            int age = today.Year - birth.Year;
            if (birth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public double CalculateBmi(int weight, int height)
        {
            if (height <= 0 || weight <= 0)
            {
                return 0;
            }

            double heightMeters = height / 100.0;
            double bmi = weight / (heightMeters * heightMeters);

            return (int)Math.Round(bmi);
        }

        public int CalculateCalorieNeeds(int weight, int height, int age, string gender, string goal)
        {
            if (weight <= 0 || height <= 0 || age <= 0)
            {
                return 0;
            }

            double bmr =
                gender.Equals(GenderMale, StringComparison.OrdinalIgnoreCase)
                    ? (BmrWeightFactor * weight) + (BmrHeightFactor * height) - (BmrAgeFactor * age) + BmrMaleOffset
                    : gender.Equals(GenderFemale, StringComparison.OrdinalIgnoreCase)
                        ? (BmrWeightFactor * weight) + (BmrHeightFactor * height) - (BmrAgeFactor * age) - BmrFemaleOffset
                        : 0;

            if (bmr <= 0)
            {
                return 0;
            }

            double tdee = bmr * ActivityMultiplier;
            goal = goal ?? string.Empty;

            double adjustedCalories = goal.ToLower() switch
            {
                GoalBulk => tdee + BulkCalorieDelta,
                GoalCut => tdee + CutCalorieDelta,
                GoalMaintenance => tdee,
                GoalWellBeing => tdee,
                _ => tdee
            };

            return (int)Math.Round(adjustedCalories);
        }

        public int CalculateProteinNeeds(int weight, string goal)
        {
            if (weight <= 0)
            {
                return 0;
            }

            goal = goal ?? string.Empty;
            double proteinPerKg = goal.ToLower() switch
            {
                GoalBulk => ProteinBulk,
                GoalCut => ProteinCut,
                GoalMaintenance => ProteinMaintenance,
                GoalWellBeing => ProteinWellBeing,
                _ => ProteinMaintenance
            };

            return (int)Math.Round(weight * proteinPerKg);
        }

        public int CalculateFatNeeds(int calorieNeeds, string goal)
        {
            if (calorieNeeds <= 0)
            {
                return 0;
            }

            goal = goal ?? string.Empty;
            double fatRatio = goal.ToLower() switch
            {
                GoalBulk or GoalCut => FatBulkCut,
                GoalMaintenance => FatMaintenance,
                GoalWellBeing => FatWellBeing,
                _ => FatBulkCut
            };

            double fatCalories = calorieNeeds * fatRatio;
            return (int)Math.Round(fatCalories / CaloriesPerGramFat);
        }

        public int CalculateCarbohydrateNeeds(int calorieNeeds, int proteinNeeds, int fatNeeds)
        {
            int proteinCalories = proteinNeeds * CaloriesPerGramProtein;
            int fatCalories = fatNeeds * CaloriesPerGramFat;

            if (calorieNeeds <= 0)
            {
                return 0;
            }

            int carbohydrateCalories = Math.Max(0, calorieNeeds - proteinCalories - fatCalories);
            return (int)Math.Round(carbohydrateCalories / (double)CaloriesPerGramCarbohydrates);
        }

        public void ApplyCalculations(UserData userData, DateTimeOffset? birthDate = null)
        {
            if (userData == null)
            {
                return;
            }

            if (birthDate.HasValue)
            {
                userData.Age = CalculateAge(birthDate);
            }

            userData.Bmi = CalculateBmi(userData.Weight, userData.Height);
            userData.CalorieNeeds = CalculateCalorieNeeds(
                userData.Weight,
                userData.Height,
                userData.Age,
                userData.Gender,
                userData.Goal);
            userData.ProteinNeeds = CalculateProteinNeeds(userData.Weight, userData.Goal);
            userData.FatNeeds = CalculateFatNeeds(userData.CalorieNeeds, userData.Goal);
            userData.CarbohydrateNeeds = CalculateCarbohydrateNeeds(
                userData.CalorieNeeds,
                userData.ProteinNeeds,
                userData.FatNeeds);
        }
    }
}
