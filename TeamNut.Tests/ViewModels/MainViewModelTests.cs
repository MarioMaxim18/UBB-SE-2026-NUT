namespace TeamNut.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using NSubstitute;
    using TeamNut.Models;
    using TeamNut.Services.Interfaces;
    using TeamNut.ViewModels;
    using Xunit;

    public class MainViewModelTests
    {
        [Fact]
        public async Task UpdateHeaderReminder_WhenUserHasNextReminder_SetsFormattedText()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new MainViewModel(reminderService);
            var reminder = new Reminder { Name = "Breakfast", Time = new TimeSpan(8, 30, 0) };

            UserSession.Login(1, "TestUser", "User");
            reminderService.GetNextReminder(1).Returns(reminder);

            await vm.UpdateHeaderReminder();

            Assert.Equal("Breakfast at 08:30", vm.NextReminderText);

            UserSession.Logout();
        }

        [Fact]
        public async Task UpdateHeaderReminder_WhenUserHasNoNextReminder_SetsNoMealsText()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new MainViewModel(reminderService);
            UserSession.Login(1, "TestUser", "User");

            reminderService.GetNextReminder(Arg.Any<int>()).Returns(Task.FromResult<Reminder?>(null));

            await vm.UpdateHeaderReminder();

            Assert.Equal("No upcoming meals", vm.NextReminderText);

            UserSession.Logout();
        }

        [Fact]
        public async Task UpdateHeaderReminder_WhenUserIdIsInvalid_DoesNotUpdateText()
        {
            var reminderService = Substitute.For<IReminderService>();
            var vm = new MainViewModel(reminderService);
            vm.NextReminderText = "Loading...";
            UserSession.Logout();

            await vm.UpdateHeaderReminder();

            Assert.Equal("Loading...", vm.NextReminderText);
        }
    }
}
