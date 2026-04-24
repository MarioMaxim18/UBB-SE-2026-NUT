using System.Collections.Generic;
using FluentAssertions;
using TeamNut.Models;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class ValidationServiceTests
    {
        private readonly ValidationService service;

        public ValidationServiceTests()
        {
            service = new ValidationService();
        }

        [Fact]
        public void ValidateUser_WithValidUser_ReturnsEmptyList()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = "Parola123",
                Role = "User"
            };

            var errors = service.ValidateUser(user);

            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateUser_WithNullUser_ReturnsError()
        {
            var errors = service.ValidateUser(null!);

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("cannot be null"));
        }

        [Fact]
        public void ValidateUser_WithInvalidUsername_ReturnsError()
        {
            var user = new User
            {
                Username = "ab",
                Password = "Parola123",
                Role = "User"
            };

            var errors = service.ValidateUser(user);

            errors.Should().NotBeEmpty();
        }

        [Fact]
        public void ValidateUserData_WithValidData_ReturnsEmptyList()
        {
            var userData = new UserData
            {
                Weight = 75,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = service.ValidateUserData(userData);

            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateUserData_WithNullData_ReturnsError()
        {
            var errors = service.ValidateUserData(null!);

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("cannot be null"));
        }

        [Fact]
        public void ValidateUserData_WithInvalidWeight_ReturnsError()
        {
            var userData = new UserData
            {
                Weight = 0,
                Height = 180,
                Age = 25,
                Gender = "male",
                Goal = "maintenance"
            };

            var errors = service.ValidateUserData(userData);

            errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("Test123")]
        [InlineData("Simple text with spaces")]
        [InlineData("Numbers 123 and letters")]
        [InlineData("Punctuation! And? Questions.")]
        public void IsValidTextInput_WithValidText_ReturnsTrue(string input)
        {
            var result = service.IsValidTextInput(input!);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsValidTextInput_WithEmptyOrWhitespace_ReturnsFalse(string? input)
        {
            var result = service.IsValidTextInput(input!);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Text with @special")]
        [InlineData("Invalid#characters")]
        [InlineData("Emoji 😊 not allowed")]
        [InlineData("Symbols < > not valid")]
        public void IsValidTextInput_WithInvalidCharacters_ReturnsFalse(string input)
        {
            var result = service.IsValidTextInput(input);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("123")]
        [InlineData("456789")]
        [InlineData("0")]
        public void IsNumericOnly_WithNumericString_ReturnsTrue(string input)
        {
            var result = service.IsNumericOnly(input!);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void IsNumericOnly_WithEmptyOrNull_ReturnsTrue(string? input)
        {
            var result = service.IsNumericOnly(input!);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("123abc")]
        [InlineData("abc")]
        [InlineData("12.34")]
        [InlineData("12-34")]
        public void IsNumericOnly_WithNonNumericCharacters_ReturnsFalse(string input)
        {
            var result = service.IsNumericOnly(input);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Morning Vitamins")]
        [InlineData("Drink Water")]
        [InlineData("Exercise")]
        [InlineData("a")]
        public void IsValidReminderName_WithValidName_ReturnsTrue(string name)
        {
            var result = service.IsValidReminderName(name!);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsValidReminderName_WithEmptyOrWhitespace_ReturnsFalse(string? name)
        {
            var result = service.IsValidReminderName(name!);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidReminderName_WithNameTooLong_ReturnsFalse()
        {
            var name = new string('a', 51);

            var result = service.IsValidReminderName(name);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidReminderName_WithNameExactlyMaxLength_ReturnsTrue()
        {
            var name = new string('a', 50);

            var result = service.IsValidReminderName(name);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("Short", 10, true)]
        [InlineData("Exactly Ten!", 12, true)]
        [InlineData("Too long for max", 10, false)]
        public void IsValidReminderName_WithCustomMaxLength_ReturnsExpected(string name, int maxLength, bool expected)
        {
            var result = service.IsValidReminderName(name, maxLength);

            result.Should().Be(expected);
        }
    }
}
