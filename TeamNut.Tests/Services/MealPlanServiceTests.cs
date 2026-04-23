// <copyright file="MealPlanServiceTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class MealPlanServiceTests
    {
        private readonly MealPlanService service;

        public MealPlanServiceTests()
        {
            service = new MealPlanService(null!, null!, null!);
        }

        [Fact]
        public void CalculateTotalNutrition_WithValidMeals_ReturnsCorrectTotals()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 500, Protein = 30, Carbs = 50, Fat = 20 },
                new Meal { Calories = 600, Protein = 40, Carbs = 60, Fat = 25 },
                new Meal { Calories = 700, Protein = 35, Carbs = 70, Fat = 30 }
            };

            var result = service.CalculateTotalNutrition(meals);

            result.totalCalories.Should().Be(1800);
            result.totalProtein.Should().Be(105);
            result.totalCarbs.Should().Be(180);
            result.totalFat.Should().Be(75);
        }

        [Fact]
        public void CalculateTotalNutrition_WithEmptyList_ReturnsZeros()
        {
            var meals = new List<Meal>();

            var result = service.CalculateTotalNutrition(meals);

            result.totalCalories.Should().Be(0);
            result.totalProtein.Should().Be(0);
            result.totalCarbs.Should().Be(0);
            result.totalFat.Should().Be(0);
        }

        [Fact]
        public void CalculateTotalNutrition_WithNullList_ReturnsZeros()
        {
            var result = service.CalculateTotalNutrition(null!);

            result.totalCalories.Should().Be(0);
            result.totalProtein.Should().Be(0);
            result.totalCarbs.Should().Be(0);
            result.totalFat.Should().Be(0);
        }

        [Fact]
        public void CalculateTotalNutrition_WithSingleMeal_ReturnsMealValues()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 500, Protein = 30, Carbs = 50, Fat = 20 }
            };

            var result = service.CalculateTotalNutrition(meals);

            result.totalCalories.Should().Be(500);
            result.totalProtein.Should().Be(30);
            result.totalCarbs.Should().Be(50);
            result.totalFat.Should().Be(20);
        }

        [Theory]
        [InlineData(2000, 150, 200, 70, 2000, 150, 200, 70, 0.10, true)]
        [InlineData(2000, 150, 200, 70, 2100, 155, 210, 73, 0.10, true)]
        [InlineData(2000, 150, 200, 70, 2500, 150, 200, 70, 0.10, false)]
        [InlineData(2000, 150, 200, 70, 1800, 135, 180, 63, 0.10, true)]
        public void ValidateMealPlan_WithVariousTotals_ReturnsExpected(
            int targetCal,
            int targetProtein,
            int targetCarbs,
            int targetFat,
            int actualCal,
            int actualProtein,
            int actualCarbs,
            int actualFat,
            double tolerance,
            bool expected)
        {
            var meals = new List<Meal>
            {
                new Meal
                {
                    Calories = actualCal,
                    Protein = actualProtein,
                    Carbs = actualCarbs,
                    Fat = actualFat
                }
            };

            var result = service.ValidateMealPlan(
                meals,
                targetCal,
                targetProtein,
                targetCarbs,
                targetFat,
                tolerance);

            result.Should().Be(expected);
        }

        [Fact]
        public void ValidateMealPlan_WithExactMatch_ReturnsTrue()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 2000, Protein = 150, Carbs = 200, Fat = 70 }
            };

            var result = service.ValidateMealPlan(meals, 2000, 150, 200, 70);

            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateMealPlan_WithinDefaultTolerance_ReturnsTrue()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 2050, Protein = 155, Carbs = 205, Fat = 72 }
            };

            var result = service.ValidateMealPlan(meals, 2000, 150, 200, 70);

            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateMealPlan_OutsideDefaultTolerance_ReturnsFalse()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 2500, Protein = 150, Carbs = 200, Fat = 70 }
            };

            var result = service.ValidateMealPlan(meals, 2000, 150, 200, 70);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("bulk", "💪")]
        [InlineData("cut", "🔥")]
        [InlineData("maintenance", "⚖️")]
        [InlineData("well-being", "🧘")]
        [InlineData("invalid", "🎯")]
        [InlineData(null, "🎯")]
        public void GetGoalEmoji_WithVariousGoals_ReturnsCorrectEmoji(string? goal, string expected)
        {
            var result = service.GetGoalEmoji(goal!);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Bulk", "💪")]
        [InlineData("CUT", "🔥")]
        [InlineData("MAINTENANCE", "⚖️")]
        public void GetGoalEmoji_IsCaseInsensitive_ReturnsCorrectEmoji(string goal, string expected)
        {
            var result = service.GetGoalEmoji(goal);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("bulk", 2000)]
        [InlineData("cut", 2000)]
        [InlineData("maintenance", 2000)]
        [InlineData("well-being", 2000)]
        public void GetCalorieAdjustmentDescription_WithValidGoals_ContainsBaseTdee(
            string goal,
            int baseTdee)
        {
            var result = service.GetCalorieAdjustmentDescription(goal, baseTdee);

            result.Should().Contain(baseTdee.ToString());
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithBulkGoal_Contains300Calories()
        {
            var result = service.GetCalorieAdjustmentDescription("bulk", 2000);

            result.Should().Contain("+300");
            result.Should().Contain("2300");
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithCutGoal_ContainsMinus300Calories()
        {
            var result = service.GetCalorieAdjustmentDescription("cut", 2000);

            result.Should().Contain("-300");
            result.Should().Contain("1700");
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithMaintenanceGoal_Contains100Calories()
        {
            var result = service.GetCalorieAdjustmentDescription("maintenance", 2000);

            result.Should().Contain("+100");
            result.Should().Contain("2100");
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithWellBeingGoal_Contains100Calories()
        {
            var result = service.GetCalorieAdjustmentDescription("well-being", 2000);

            result.Should().Contain("+100");
            result.Should().Contain("2100");
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithInvalidGoal_ReturnsNoAdjustment()
        {
            var result = service.GetCalorieAdjustmentDescription("invalid", 2000);

            result.Should().Contain("No adjustment");
            result.Should().Contain("2000");
        }

        [Fact]
        public void GetCalorieAdjustmentDescription_WithNullGoal_ReturnsNoAdjustment()
        {
            var result = service.GetCalorieAdjustmentDescription(null!, 2000);

            result.Should().Contain("No adjustment");
        }

        [Fact]
        public void CalculateTotalNutrition_WithMultipleMeals_SumsCorrectly()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 100, Protein = 10, Carbs = 20, Fat = 5 },
                new Meal { Calories = 200, Protein = 15, Carbs = 30, Fat = 8 },
                new Meal { Calories = 300, Protein = 20, Carbs = 40, Fat = 12 },
                new Meal { Calories = 400, Protein = 25, Carbs = 50, Fat = 15 }
            };

            var result = service.CalculateTotalNutrition(meals);

            result.totalCalories.Should().Be(1000);
            result.totalProtein.Should().Be(70);
            result.totalCarbs.Should().Be(140);
            result.totalFat.Should().Be(40);
        }

        [Theory]
        [InlineData(0.05)]
        [InlineData(0.15)]
        [InlineData(0.20)]
        public void ValidateMealPlan_WithCustomTolerance_RespectsToleranceValue(double tolerance)
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 2100, Protein = 150, Carbs = 200, Fat = 70 }
            };

            var withinTolerance = 2000 * tolerance >= 100;

            var result = service.ValidateMealPlan(meals, 2000, 150, 200, 70, tolerance);

            result.Should().Be(withinTolerance);
        }

        [Fact]
        public void ValidateMealPlan_WithMultipleMealsExactMatch_ReturnsTrue()
        {
            var meals = new List<Meal>
            {
                new Meal { Calories = 1000, Protein = 75, Carbs = 100, Fat = 35 },
                new Meal { Calories = 1000, Protein = 75, Carbs = 100, Fat = 35 }
            };

            var result = service.ValidateMealPlan(meals, 2000, 150, 200, 70);

            result.Should().BeTrue();
        }
    }
}
