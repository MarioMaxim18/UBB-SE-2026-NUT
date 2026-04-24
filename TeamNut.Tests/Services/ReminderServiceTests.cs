using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class ReminderServiceTests
    {
        private readonly IReminderRepository mockRepo;
        private readonly ReminderService service;

        public ReminderServiceTests()
        {
            mockRepo = Substitute.For<IReminderRepository>();
            service = new ReminderService(mockRepo);
        }

        [Fact]
        public async Task SaveReminder_ValidName_ReturnsSuccess()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = "Micul dejun",
                ReminderDate = "2024-01-01",
                Time = new System.TimeSpan(8, 0, 0)
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Success");
            await mockRepo.Received(1).Add(Arg.Any<Reminder>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task SaveReminder_EmptyOrNullName_ReturnsError(string? name)
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = name ?? string.Empty,
                ReminderDate = "2024-01-01"
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Error: Name must be between 1 and 50 characters.");
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_NameOver50Chars_ReturnsError()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = new string('x', 51),
                ReminderDate = "2024-01-01"
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Error: Name must be between 1 and 50 characters.");
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_NameExactly50Chars_Succeeds()
        {
            var reminder = new Reminder { UserId = 1, Name = new string('a', 50) };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Success");
        }

        [Fact]
        public async Task SaveReminder_IdZero_CallsAdd()
        {
            var reminder = new Reminder { Id = 0, UserId = 1, Name = "Pranz" };

            await service.SaveReminder(reminder);

            await mockRepo.Received(1).Add(Arg.Any<Reminder>());
            await mockRepo.DidNotReceive().Update(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_ExistingId_CallsUpdate()
        {
            var reminder = new Reminder { Id = 5, UserId = 1, Name = "Cina" };

            await service.SaveReminder(reminder);

            await mockRepo.Received(1).Update(Arg.Any<Reminder>());
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task DeleteReminder_FiresRemindersChangedEvent()
        {
            var existing = new Reminder { Id = 3, UserId = 1, Name = "Masa de pranz" };
            mockRepo.GetById(3).Returns(existing);
            bool eventFired = false;
            service.RemindersChanged += (s, uid) => eventFired = true;

            await service.DeleteReminder(3);

            await mockRepo.Received(1).Delete(3);
            eventFired.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteReminder_NonexistentId_StillCallsDelete()
        {
            mockRepo.GetById(999).Returns((Reminder?)null);

            await service.DeleteReminder(999);

            await mockRepo.Received(1).Delete(999);
        }

        [Fact]
        public async Task GetUserReminders_ReturnsList()
        {
            var reminders = new List<Reminder>
            {
                new Reminder { Id = 1, Name = "Micul dejun", UserId = 5 },
                new Reminder { Id = 2, Name = "Cina", UserId = 5 }
            };
            mockRepo.GetAllByUserId(5).Returns(reminders);

            var result = await service.GetUserReminders(5);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetNextReminder_WhenExists_ReturnsIt()
        {
            var next = new Reminder { Id = 1, Name = "Masa de seara", UserId = 1 };
            mockRepo.GetNextReminder(1).Returns(next);

            var result = await service.GetNextReminder(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Masa de seara");
        }

        [Fact]
        public async Task GetNextReminder_WhenNone_ReturnsNull()
        {
            mockRepo.GetNextReminder(1).Returns((Reminder?)null);

            var result = await service.GetNextReminder(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SaveReminder_FiresEventWithCorrectUserId()
        {
            int receivedUserId = -1;
            service.RemindersChanged += (s, uid) => receivedUserId = uid;
            var reminder = new Reminder { UserId = 7, Name = "Gustare" };

            await service.SaveReminder(reminder);

            receivedUserId.Should().Be(7);
        }
    }
}
