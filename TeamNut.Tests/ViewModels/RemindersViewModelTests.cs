// <copyright file="RemindersViewModelTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿namespace TeamNut.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    [Collection("UsesStaticUserSession")]
    public class RemindersViewModelTests
    {
        [Fact]
        public async Task SaveReminderAsync_WhenReminderIsNull_ReturnsInvalidMessage()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);

            var result = await vm.SaveReminderAsync(null!);

            Assert.Equal("Error: invalid reminder", result);
        }

        [Fact]
        public async Task SaveReminderAsync_WhenSuccess_LoadsRemindersAndReturnsSuccess()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);
            var reminder = new Reminder { Id = 1 };
            var loadedReminders = new List<Reminder> { reminder };

            UserSession.Login(1, "TestUser", "User");
            reminderService.SaveReminder(reminder).Returns("Success");
            reminderService.GetUserReminders(1).Returns(loadedReminders);

            var result = await vm.SaveReminderAsync(reminder);

            Assert.Equal("Success", result);
            Assert.Single(vm.Reminders);

            UserSession.Logout();
        }

        [Fact]
        public async Task LoadRemindersCommand_WhenUserIdIsInvalid_LeavesRemindersEmpty()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);

            UserSession.Logout();

            await vm.LoadRemindersCommand.ExecuteAsync(null);

            Assert.Empty(vm.Reminders);
            Assert.Null(vm.NextReminder);
        }

        [Fact]
        public async Task LoadRemindersCommand_WhenIsBusy_DoesNotModifyState()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);
            vm.IsBusy = true;

            UserSession.Login(1, "TestUser", "User");

            await vm.LoadRemindersCommand.ExecuteAsync(null);

            Assert.Empty(vm.Reminders);
            Assert.True(vm.IsBusy);

            UserSession.Logout();
        }

        [Fact]
        public async Task LoadRemindersCommand_WhenValid_PopulatesRemindersAndNextReminder()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);
            var reminders = new List<Reminder> { new Reminder { Id = 1 }, new Reminder { Id = 2 } };
            var nextReminder = new Reminder { Id = 2 };

            UserSession.Login(1, "TestUser", "User");
            reminderService.GetUserReminders(1).Returns(reminders);
            reminderService.GetNextReminder(1).Returns(nextReminder);

            await vm.LoadRemindersCommand.ExecuteAsync(null);

            Assert.Equal(2, vm.Reminders.Count);
            Assert.Equal(nextReminder, vm.NextReminder);
            Assert.False(vm.IsBusy);

            UserSession.Logout();
        }

        [Fact]
        public void PrepareNewReminderCommand_AssignsCurrentUserIdToSelectedReminder()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);

            UserSession.Login(99, "TestUser", "User");

            vm.PrepareNewReminderCommand.Execute(null);

            Assert.NotNull(vm.SelectedReminder);
            Assert.Equal(99, vm.SelectedReminder.UserId);

            UserSession.Logout();
        }

        [Fact]
        public async Task DeleteReminderCommand_WhenValid_RemovesFromCollection()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new RemindersViewModel(reminderService);
            var reminder = new Reminder { Id = 5 };
            vm.Reminders.Add(reminder);

            UserSession.Login(1, "TestUser", "User");

            await vm.DeleteReminderCommand.ExecuteAsync(reminder);

            Assert.Empty(vm.Reminders);

            UserSession.Logout();
        }
    }
}
