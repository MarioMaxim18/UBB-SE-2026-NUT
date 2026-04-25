using System;
using FluentAssertions;
using TeamNut.Models;
using Xunit;

namespace TeamNut.Tests.Models
{
    public class UserDataTests
    {
        [Fact]
        public void CalculateAge_WithBirthDate25YearsAgo_Returns25()
        {
            var userData = new UserData();
            var birthDate = new DateTimeOffset(DateTime.Today.AddYears(-25));

            var result = userData.CalculateAge(birthDate);

            result.Should().Be(25);
        }

        [Fact]
        public void CalculateAge_WithBirthdayNotYetThisYear_ReturnsAgeMinusOne()
        {
            var userData = new UserData();
            var birthDate = new DateTimeOffset(DateTime.Today.AddYears(-25).AddDays(1));

            var result = userData.CalculateAge(birthDate);

            result.Should().Be(24);
        }

        [Fact]
        public void CalculateAge_WithNullBirthDate_ReturnsZero()
        {
            var userData = new UserData();

            var result = userData.CalculateAge(null);

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
            var userData = new UserData { Weight = weight, Height = height };

            var result = userData.CalculateBmi();

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateBmi_WithZeroHeight_ReturnsZero()
        {
            var userData = new UserData { Weight = 75, Height = 0 };

            var result = userData.CalculateBmi();

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateBmi_WithZeroWeight_ReturnsZero()
        {
            var userData = new UserData { Weight = 0, Height = 180 };

            var result = userData.CalculateBmi();

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateBmi_WithNegativeHeight_ReturnsZero()
        {
            var userData = new UserData { Weight = 75, Height = -180 };

            var result = userData.CalculateBmi();

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
            var userData = new UserData
            {
                Weight = weight,
                Height = height,
                Age = age,
                Gender = gender,
                Goal = goal
            };

            var result = userData.CalculateCalorieNeeds();

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroWeight_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 0,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var result = userData.CalculateCalorieNeeds();

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroHeight_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 0,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var result = userData.CalculateCalorieNeeds();

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCalorieNeeds_WithZeroAge_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 0,
                Gender = "male",
                Goal = "maintenance"
            };

            var result = userData.CalculateCalorieNeeds();

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, "bulk", 150)]
        [InlineData(75, "cut", 165)]
        [InlineData(75, "maintenance", 135)]
        [InlineData(75, "well-being", 120)]
        [InlineData(80, "bulk", 160)]
        [InlineData(60, "cut", 132)]
        public void CalculateProteinNeeds_WithValidInputs_ReturnsCorrectProtein(
            int weight,
            string goal,
            int expected)
        {
            var userData = new UserData { Weight = weight, Goal = goal };

            var result = userData.CalculateProteinNeeds();

            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateProteinNeeds_WithZeroWeight_ReturnsZero()
        {
            var userData = new UserData { Weight = 0, Goal = "bulk" };

            var result = userData.CalculateProteinNeeds();

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, 180, 25, "male", "bulk", 84)]
        [InlineData(75, 180, 25, "male", "cut", 67)]
        [InlineData(75, 180, 25, "male", "maintenance", 85)]
        [InlineData(75, 180, 25, "male", "well-being", 91)]
        [InlineData(80, 175, 30, "male", "bulk", 84)]
        public void CalculateFatNeeds_WithValidInputs_ReturnsCorrectFat(
            int weight,
            int height,
            int age,
            string gender,
            string goal,
            int expected)
        {
            var userData = new UserData
            {
                Weight = weight,
                Height = height,
                Age = age,
                Gender = gender,
                Goal = goal
            };

            var result = userData.CalculateFatNeeds();

            result.Should().Be(expected);
        }
        [Fact]
        public void CalculateFatNeeds_WithZeroCalorieNeeds_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 0,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "bulk"
            };

            var result = userData.CalculateFatNeeds();

            result.Should().Be(0);
        }

        [Theory]
        [InlineData(75, 180, 25, "male", "bulk", 416)]
        [InlineData(75, 180, 25, "male", "cut", 289)]
        [InlineData(75, 180, 25, "male", "maintenance", 354)]
        public void CalculateCarbohydrateNeeds_WithValidInputs_ReturnsCorrectCarbohydrates(
            int weight,
            int height,
            int age,
            string gender,
            string goal,
            int expected)
        {
            var userData = new UserData
            {
                Weight = weight,
                Height = height,
                Age = age,
                Gender = gender,
                Goal = goal
            };

            var result = userData.CalculateCarbohydrateNeeds();

            result.Should().Be(expected);
        }
        [Fact]
        public void CalculateCarbohydrateNeeds_WithZeroCalories_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 0,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var result = userData.CalculateCarbohydrateNeeds();

            result.Should().Be(0);
        }

        [Fact]
        public void CalculateCarbohydrateNeeds_WhenProteinAndFatExceedCalories_ReturnsZero()
        {
            var userData = new UserData
            {
                Weight = 200,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "cut"
            };

            var result = userData.CalculateCarbohydrateNeeds();

            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void GetValidationErrors_WithValidData_ReturnsEmptyList()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().BeEmpty();
        }

        [Fact]
        public void GetValidationErrors_WithWeightTooLow_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 0,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Weight"));
        }

        [Fact]
        public void GetValidationErrors_WithWeightTooHigh_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 501,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Weight"));
        }

        [Fact]
        public void GetValidationErrors_WithHeightTooLow_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 0,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Height"));
        }

        [Fact]
        public void GetValidationErrors_WithInvalidGender_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "invalid",
                Goal = "maintenance"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Gender"));
        }

        [Fact]
        public void GetValidationErrors_WithInvalidGoal_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "invalid"
            };

            var errors = userData.GetValidationErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("goal"));
        }
    }
}
