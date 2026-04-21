using System;
using TeamNut.Models;

namespace TeamNut.Services.Interfaces
{
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
