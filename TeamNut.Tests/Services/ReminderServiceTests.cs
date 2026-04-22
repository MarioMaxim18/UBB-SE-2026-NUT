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
        public async Task SaveReminder_WithValidName_ReturnsSuccess()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = "Breakfast",
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
        public async Task SaveReminder_WithEmptyName_ReturnsError(string name)
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = name,
                ReminderDate = "2024-01-01"
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Contain("Error");
            result.Should().Contain("Name");
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_WithNameTooLong_ReturnsError()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = new string('a', 51),
                ReminderDate = "2024-01-01"
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Contain("Error");
            result.Should().Contain("50 characters");
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_WithNameExactlyMaxLength_ReturnsSuccess()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = new string('a', 50),
                ReminderDate = "2024-01-01"
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Success");
        }

        [Theory]
        [InlineData("Morning Pills")]
        [InlineData("Drink Water")]
        [InlineData("a")]
        [InlineData("Breakfast at 8")]
        [InlineData("Take vitamin D supplement")]
        public async Task SaveReminder_WithValidNames_ReturnsSuccess(string name)
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = name
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Success");
        }

        [Fact]
        public async Task SaveReminder_WithIdZero_CallsRepositoryAdd()
        {
            var reminder = new Reminder
            {
                Id = 0,
                UserId = 1,
                Name = "Lunch"
            };

            await service.SaveReminder(reminder);

            await mockRepo.Received(1).Add(Arg.Any<Reminder>());
            await mockRepo.DidNotReceive().Update(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_WithExistingId_CallsRepositoryUpdate()
        {
            var reminder = new Reminder
            {
                Id = 5,
                UserId = 1,
                Name = "Dinner"
            };

            await service.SaveReminder(reminder);

            await mockRepo.Received(1).Update(Arg.Any<Reminder>());
            await mockRepo.DidNotReceive().Add(Arg.Any<Reminder>());
        }

        [Fact]
        public async Task SaveReminder_WithName49Characters_ReturnsSuccess()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = new string('a', 49)
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Be("Success");
        }

        [Fact]
        public async Task SaveReminder_WithWhitespaceOnlyName_ReturnsError()
        {
            var reminder = new Reminder
            {
                UserId = 1,
                Name = "     "
            };

            var result = await service.SaveReminder(reminder);

            result.Should().Contain("Error");
        }
    }
}