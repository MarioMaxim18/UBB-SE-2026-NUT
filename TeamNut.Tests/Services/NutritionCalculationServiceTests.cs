using System;
using FluentAssertions;
using FluentAssertions.Execution;
using TeamNut.Models;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class NutritionCalculationServiceTests
    {
        private readonly NutritionCalculationService service;

        public NutritionCalculationServiceTests()
        {
            service = new NutritionCalculationService();
        }

        [Fact]
        public void CalculateAge_WithBirthDate25YearsAgo_Returns25()
        {
            var birthDate = new DateTimeOffset(DateTime.Today.AddYears(-25));

            var result = service.CalculateAge(birthDate);

            result.Should().Be(25);
        }

        [Fact]
        public void CalculateAge_WithBirthdayNotYetThisYear_ReturnsAgeMinusOne()
        {
            var birthDate = new DateTimeOffset(DateTime.Today.AddYears(-25).AddDays(1));

            var result = service.CalculateAge(birthDate);

            result.Should().Be(24);
        }

        [Fact]
        public void CalculateAge_WithNullBirthDate_ReturnsZero()
        {
            var result = service.CalculateAge(null);

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, 180, 23)]
        [InlineData(80, 180, 25)]
        [InlineData(90, 180, 28)]
        [InlineData(100, 200, 25)]
        [InlineData(60, 170, 21)]
        public void CalculateBmi_WithValidWeightAndHeight_ReturnsCorrectBmi(int weight, int height, double expected)
        {
            var result = service.CalculateBmi(weight, height);

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateBmi_WithZeroHeight_ReturnsZero()
        {
            var result = service.CalculateBmi(75, 0);

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateBmi_WithZeroWeight_ReturnsZero()
        {
            var result = service.CalculateBmi(0, 180);

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateBmi_WithNegativeHeight_ReturnsZero()
        {
            var result = service.CalculateBmi(75, -180);

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, 180, 25, "male", "bulk", 3020)]
        [InlineData(75, 180, 25, "male", "cut", 2420)]
        [InlineData(75, 180, 25, "male", "maintenance", 2720)]
        [InlineData(75, 180, 25, "female", "bulk", 2763)]
        [InlineData(75, 180, 25, "female", "cut", 2163)]
        [InlineData(80, 175, 30, "male", "bulk", 3011)]
        public void CalculateCalorieNeeds_WithValidInputs_ReturnsCorrectCalories(
            int weight,
            int height,
            int age,
            string gender,
            string goal,
            int expected)
        {
            var result = service.CalculateCalorieNeeds(weight, height, age, gender, goal);

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroWeight_ReturnsZero()
        {
            var result = service.CalculateCalorieNeeds(0, 180, 25, "male", "maintenance");

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroHeight_ReturnsZero()
        {
            var result = service.CalculateCalorieNeeds(75, 0, 25, "male", "maintenance");

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroAge_ReturnsZero()
        {
            var result = service.CalculateCalorieNeeds(75, 180, 0, "male", "maintenance");

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, "bulk", 150)]
        [InlineData(75, "cut", 165)]
        [InlineData(75, "maintenance", 135)]
        [InlineData(75, "well-being", 120)]
        [InlineData(80, "bulk", 160)]
        [InlineData(60, "cut", 132)]
        public void CalculateProteinNeeds_WithValidInputs_ReturnsCorrectProtein(int weight, string goal, int expected)
        {
            var result = service.CalculateProteinNeeds(weight, goal);

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateProteinNeeds_WithZeroWeight_ReturnsZero()
        {
            var result = service.CalculateProteinNeeds(0, "bulk");

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(2000, "bulk", 56)]
        [InlineData(2000, "cut", 56)]
        [InlineData(2000, "maintenance", 62)]
        [InlineData(2000, "well-being", 67)]
        [InlineData(2500, "bulk", 69)]
        public void CalculateFatNeeds_WithValidCalories_ReturnsCorrectFat(int calories, string goal, int expected)
        {
            var result = service.CalculateFatNeeds(calories, goal);

            result.Should().BeInRange(expected - 3, expected + 3);
        }

        [Fact]
        public void CalculateFatNeeds_WithZeroCalories_ReturnsZero()
        {
            var result = service.CalculateFatNeeds(0, "bulk");

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(3020, 150, 84, 416)]
        [InlineData(2420, 165, 67, 289)]
        [InlineData(2720, 135, 85, 354)]
        public void CalculateCarbNeeds_WithValidInputs_ReturnsCorrectCarbs(
            int calories,
            int protein,
            int fat,
            int expected)
        {
            var result = service.CalculateCarbNeeds(calories, protein, fat);

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateCarbNeeds_WithZeroCalories_ReturnsZero()
        {
            var result = service.CalculateCarbNeeds(0, 150, 56);

            result.Should().Be(0);
        }

        [Fact]
        public void ApplyCalculations_WithValidUserData_CalculatesAllMetrics()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            service.ApplyCalculations(userData, null!);

            userData.Bmi.Should().Be(23);
            userData.CalorieNeeds.Should().BeGreaterThan(0);
            userData.ProteinNeeds.Should().BeGreaterThan(0);
            userData.CarbNeeds.Should().BeGreaterThan(0);
            userData.FatNeeds.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ApplyCalculations_WithBirthDate_CalculatesAge()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Gender = "male",
                Goal = "maintenance"
            };
            var birthDate = new DateTimeOffset(DateTime.Today.AddYears(-30));

            service.ApplyCalculations(userData, birthDate);

            userData.Age.Should().Be(30);
        }

        [Fact]
        public void ApplyCalculations_WithNullUserData_DoesNotThrow()
        {
            Action act = () => service.ApplyCalculations(null!, null!);

            act.Should().NotThrow();
        }

        [Fact]
        public void CalculateCarbNeeds_WhenProteinAndFatExceedCalories_ReturnsZero()
        {
            var result = service.CalculateCarbNeeds(100, 50, 50);

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithNullGoal_DoesNotThrow()
        {
            var result = service.CalculateCalorieNeeds(75, 180, 25, "male", null!);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CalculateProteinNeeds_WithNullGoal_DoesNotThrow()
        {
            var result = service.CalculateProteinNeeds(75, null!);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CalculateFatNeeds_WithNullGoal_DoesNotThrow()
        {
            var result = service.CalculateFatNeeds(2000, null!);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ApplyCalculations_WithNullGoal_CarbsNeverNegative()
        {
            var userData = new UserData
            {
                Weight = 60,
                Height = 160,
                Age = 22,
                Gender = "female",
                Goal = null!
            };

            service.ApplyCalculations(userData);

            userData.CarbNeeds.Should().BeGreaterThanOrEqualTo(0);
            userData.CalorieNeeds.Should().BeGreaterThan(0);
        }
    }
}
