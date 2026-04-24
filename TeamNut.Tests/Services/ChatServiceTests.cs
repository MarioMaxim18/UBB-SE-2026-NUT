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
    public class ChatServiceTests
    {
        private readonly IChatRepository mockRepo;
        private readonly ChatService service;

        public ChatServiceTests()
        {
            mockRepo = Substitute.For<IChatRepository>();
            service = new ChatService(mockRepo);
        }

        [Fact]
        public async Task GetOrCreateConversationForUserAsync_ReturnsConversationWithCorrectUser()
        {
            var expected = new Conversation { Id = 1, UserId = 5 };
            mockRepo.GetOrCreateConversationForUserAsync(5).Returns(expected);

            var result = await service.GetOrCreateConversationForUserAsync(5);

            result.Should().NotBeNull();
            result.UserId.Should().Be(5);
        }

        [Fact]
        public async Task GetAllConversationsAsync_WhenEmpty_ReturnsEmptyList()
        {
            mockRepo.GetAllConversationsAsync().Returns(new List<Conversation>());

            var result = await service.GetAllConversationsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddMessageAsync_WithSenderIdZero_StillCallsRepository()
        {
            await service.AddMessageAsync(1, 0, "mesaj fara user", true);

            await mockRepo.Received(1).AddMessageAsync(1, 0, "mesaj fara user", true);
        }
    }
}
