using System.Collections.Generic;
using FluentAssertions;
using TeamNut.Models;
using Xunit;

namespace TeamNut.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void ValidateAndReturnErrors_WithValidUser_ReturnsEmptyList()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateAndReturnErrors_WithEmptyUsername_ReturnsError()
        {
            var user = new User
            {
                Username = string.Empty,
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Username"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithUsernameTooShort_ReturnsError()
        {
            var user = new User
            {
                Username = "ab",
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Username"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithUsernameTooLong_ReturnsError()
        {
            var user = new User
            {
                Username = new string('a', 31),
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Username"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithNonAlphanumericUsername_ReturnsError()
        {
            var user = new User
            {
                Username = "Ion@Popescu",
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("alphanumeric"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithEmptyPassword_ReturnsError()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = string.Empty,
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Password"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithPasswordTooShort_ReturnsError()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = "Par123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Password"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithPasswordTooLong_ReturnsError()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = new string('a', 31),
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Password"));
        }

        [Fact]
        public void ValidateAndReturnErrors_WithInvalidRole_ReturnsError()
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = "Parola123",
                Role = "InvalidRole"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Role"));
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Nutritionist")]
        public void ValidateAndReturnErrors_WithValidRoles_ReturnsEmptyList(string role)
        {
            var user = new User
            {
                Username = "PopescuIon",
                Password = "Parola123",
                Role = role
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateAndReturnErrors_WithMultipleErrors_ReturnsAllErrors()
        {
            var user = new User
            {
                Username = "ab",
                Password = "short",
                Role = "InvalidRole"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().HaveCountGreaterThan(1);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("PopescuIon123")]
        [InlineData("Nelu1")]
        [InlineData("gigel123")]
        public void ValidateAndReturnErrors_WithValidAlphanumericUsernames_ReturnsEmptyList(string username)
        {
            var user = new User
            {
                Username = username,
                Password = "Parola123",
                Role = "User"
            };

            var errors = user.ValidateAndReturnErrors();

            errors.Should().BeEmpty();
        }
    }
}
