// <copyright file="UserViewModelTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿namespace TeamNut.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    public class UserViewModelTests
    {
        [Fact]
        public async Task RegisterCommand_WhenValidationFails_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUser(Arg.Any<User>()).Returns(new List<string> { "Password too short." });

            await vm.RegisterCommand.ExecuteAsync(null);

            Assert.Equal("Password too short.", vm.StatusMessage);
        }

        [Fact]
        public async Task RegisterCommand_WhenUsernameExists_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUser(Arg.Any<User>()).Returns(new List<string>());
            userService.CheckIfUsernameExistsAsync(Arg.Any<string>()).Returns(true);

            await vm.RegisterCommand.ExecuteAsync(null);

            Assert.Equal("Username already exists. Please choose another one.", vm.StatusMessage);
        }

        [Fact]
        public async Task RegisterCommand_WhenUserRole_RaisesRegistrationValidEvent()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            vm.IsNutritionistChecked = false;
            validationService.ValidateUser(Arg.Any<User>()).Returns(new List<string>());
            userService.CheckIfUsernameExistsAsync(Arg.Any<string>()).Returns(false);

            bool eventRaised = false;
            vm.RegistrationValid += (s, e) => eventRaised = true;

            await vm.RegisterCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
            Assert.Equal("User", vm.CurrentUser.Role);
        }

        [Fact]
        public async Task RegisterCommand_WhenNutritionistRole_RegistersAndRaisesLoginSuccessEvent()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);
            var registeredUser = new User { Id = 1, Role = "Nutritionist" };

            vm.IsNutritionistChecked = true;
            validationService.ValidateUser(Arg.Any<User>()).Returns(new List<string>());
            userService.CheckIfUsernameExistsAsync(Arg.Any<string>()).Returns(false);
            userService.RegisterUserAsync(Arg.Any<User>()).Returns(registeredUser);

            bool eventRaised = false;
            vm.LoginSuccess += (s, e) => eventRaised = true;

            await vm.RegisterCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
            Assert.Equal("Nutritionist", vm.CurrentUser.Role);
        }

        [Fact]
        public async Task SaveDataCommand_WhenValidationFails_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUserData(Arg.Any<UserData>()).Returns(new List<string> { "Height required." });

            await vm.SaveDataCommand.ExecuteAsync(null);

            Assert.Equal("Height required.", vm.StatusMessage);
        }

        [Fact]
        public async Task SaveDataCommand_WhenAgeIsInvalid_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUserData(Arg.Any<UserData>()).Returns(new List<string>());
            calcService.CalculateAge(Arg.Any<DateTimeOffset>()).Returns(0);

            await vm.SaveDataCommand.ExecuteAsync(null);

            Assert.Equal("Please select a valid birthdate.", vm.StatusMessage);
        }

        [Fact]
        public async Task SaveDataCommand_WhenRegistrationFails_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUserData(Arg.Any<UserData>()).Returns(new List<string>());
            calcService.CalculateAge(Arg.Any<DateTimeOffset>()).Returns(25);
            userService.RegisterUserAsync(Arg.Any<User>()).Returns((User?)null);

            await vm.SaveDataCommand.ExecuteAsync(null);

            Assert.Equal("Registration failed. Username might already exist.", vm.StatusMessage);
        }

        [Fact]
        public async Task SaveDataCommand_WhenSuccessful_AddsDataAndRaisesEvent()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);
            var registeredUser = new User { Id = 5 };

            validationService.ValidateUserData(Arg.Any<UserData>()).Returns(new List<string>());
            calcService.CalculateAge(Arg.Any<DateTimeOffset>()).Returns(25);
            userService.RegisterUserAsync(Arg.Any<User>()).Returns(registeredUser);

            bool eventRaised = false;
            vm.SaveDataSuccess += (s, e) => eventRaised = true;

            await vm.SaveDataCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
            Assert.Equal(5, vm.CurrentUserData.UserId);
        }

        [Fact]
        public async Task SaveDataCommand_WhenExceptionOccurs_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            validationService.ValidateUserData(Arg.Any<UserData>()).Returns(new List<string>());
            calcService.CalculateAge(Arg.Any<DateTimeOffset>()).Returns(25);
            userService.RegisterUserAsync(Arg.Any<User>()).Returns(Task.FromException<User>(new Exception("DB Offline")));

            await vm.SaveDataCommand.ExecuteAsync(null);

            Assert.Equal("An error occurred while saving: DB Offline", vm.StatusMessage);
        }

        [Fact]
        public async Task LoginCommand_WhenCredentialsMissing_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            vm.CurrentUser.Username = string.Empty;

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.Equal("Username and Password are required.", vm.StatusMessage);
        }

        [Fact]
        public async Task LoginCommand_WhenLoginFails_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            vm.CurrentUser.Username = "TestUser";
            vm.CurrentUser.Password = "TestPass";
            userService.LoginAsync("TestUser", "TestPass").Returns((User?)null);

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.Equal("Invalid username or password.", vm.StatusMessage);
        }

        [Fact]
        public async Task LoginCommand_WhenLoginSucceeds_UpdatesUserAndRaisesEvent()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);
            var loggedInUser = new User { Id = 1, Username = "TestUser" };

            vm.CurrentUser.Username = "TestUser";
            vm.CurrentUser.Password = "TestPass";
            userService.LoginAsync("TestUser", "TestPass").Returns(loggedInUser);

            bool eventRaised = false;
            vm.LoginSuccess += (s, e) => eventRaised = true;

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
            Assert.Equal(loggedInUser, vm.CurrentUser);
        }

        [Fact]
        public async Task LoginCommand_WhenExceptionOccurs_SetsStatusMessage()
        {
            var userService = Substitute.For<IUserService>();
            var validationService = Substitute.For<IValidationService>();
            var calcService = Substitute.For<INutritionCalculationService>();
            var vm = new UserViewModel(userService, validationService, calcService);

            vm.CurrentUser.Username = "TestUser";
            vm.CurrentUser.Password = "TestPass";
            userService.LoginAsync("TestUser", "TestPass").Returns(Task.FromException<User>(new Exception("Timeout")));

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.Equal("Database Connection Failed! Start SSMS and check your server. Error: Timeout", vm.StatusMessage);
        }
    }
}
