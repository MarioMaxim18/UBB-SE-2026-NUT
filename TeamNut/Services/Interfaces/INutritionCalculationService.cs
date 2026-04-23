namespace TeamNut.Services.Interfaces
{
    using System;
    using TeamNut.Models;

    public interface INutritionCalculationService
    {
        int CalculateAge(DateTimeOffset? birthDate);
        double CalculateBmi(int weight, int height);
        int CalculateCalorieNeeds(int weight, int height, int age, string gender, string goal);
        int CalculateProteinNeeds(int weight, string goal);
        int CalculateFatNeeds(int calorieNeeds, string goal);
        int CalculateCarbNeeds(int calorieNeeds, int proteinNeeds, int fatNeeds);
        void ApplyCalculations(UserData userData, DateTimeOffset? birthDate = null);
    }
}
