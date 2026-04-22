using System.Collections.Generic;
using System.Linq;
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
        public async Task GetAllConversationsAsync_CallsRepository()
        {
            var expectedConversations = new List<Conversation>
            {
                new Conversation { Id = 1, UserId = 1 },
                new Conversation { Id = 2, UserId = 2 }
            };
            mockRepo.GetAllConversationsAsync().Returns(expectedConversations);

            var result = await service.GetAllConversationsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            await mockRepo.Received(1).GetAllConversationsAsync();
        }

        [Fact]
        public async Task GetOrCreateConversationForUserAsync_CallsRepository()
        {
            var expectedConversation = new Conversation { Id = 1, UserId = 5 };
            mockRepo.GetOrCreateConversationForUserAsync(5).Returns(expectedConversation);

            var result = await service.GetOrCreateConversationForUserAsync(5);

            result.Should().NotBeNull();
            result.UserId.Should().Be(5);
            await mockRepo.Received(1).GetOrCreateConversationForUserAsync(5);
        }

        [Fact]
        public async Task GetMessagesForConversationAsync_CallsRepository()
        {
            var expectedMessages = new List<Message>
            {
                new Message { Id = 1, ConversationId = 10, TextContent = "Hello" },
                new Message { Id = 2, ConversationId = 10, TextContent = "Hi there" }
            };
            mockRepo.GetMessagesForConversationAsync(10).Returns(expectedMessages);

            var result = await service.GetMessagesForConversationAsync(10);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            await mockRepo.Received(1).GetMessagesForConversationAsync(10);
        }

        [Fact]
        public async Task GetConversationsWithMessagesAsync_CallsRepository()
        {
            var expectedConversations = new List<Conversation>
            {
                new Conversation { Id = 1, UserId = 1 },
                new Conversation { Id = 2, UserId = 2 }
            };
            mockRepo.GetConversationsWithMessagesAsync().Returns(expectedConversations);

            var result = await service.GetConversationsWithMessagesAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            await mockRepo.Received(1).GetConversationsWithMessagesAsync();
        }

        [Fact]
        public async Task GetConversationsWhereNutritionistRespondedAsync_CallsRepository()
        {
            var nutritionistId = 3;
            var expectedConversations = new List<Conversation>
            {
                new Conversation { Id = 1, UserId = 1 }
            };
            mockRepo.GetConversationsWhereNutritionistRespondedAsync(nutritionistId)
                .Returns(expectedConversations);

            var result = await service.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            await mockRepo.Received(1).GetConversationsWhereNutritionistRespondedAsync(nutritionistId);
        }

        [Fact]
        public async Task AddMessageAsync_CallsRepository()
        {
            var conversationId = 5;
            var senderId = 10;
            var text = "Test message";
            var isNutritionist = false;

            await service.AddMessageAsync(conversationId, senderId, text, isNutritionist);

            await mockRepo.Received(1).AddMessageAsync(conversationId, senderId, text, isNutritionist);
        }

        [Fact]
        public async Task GetConversationsWithUserMessagesAsync_CallsRepository()
        {
            var expectedConversations = new List<Conversation>
            {
                new Conversation { Id = 1, UserId = 1 },
                new Conversation { Id = 2, UserId = 2 }
            };
            mockRepo.GetConversationsWithUserMessagesAsync().Returns(expectedConversations);

            var result = await service.GetConversationsWithUserMessagesAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            await mockRepo.Received(1).GetConversationsWithUserMessagesAsync();
        }

        [Fact]
        public async Task GetAllConversationsAsync_WithEmptyList_ReturnsEmptyList()
        {
            mockRepo.GetAllConversationsAsync().Returns(new List<Conversation>());

            var result = await service.GetAllConversationsAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMessagesForConversationAsync_WithEmptyList_ReturnsEmptyList()
        {
            mockRepo.GetMessagesForConversationAsync(Arg.Any<int>()).Returns(new List<Message>());

            var result = await service.GetMessagesForConversationAsync(99);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddMessageAsync_WithEmptyText_CallsRepository()
        {
            await service.AddMessageAsync(1, 2, "", false);

            await mockRepo.Received(1).AddMessageAsync(1, 2, "", false);
        }

        [Fact]
        public async Task AddMessageAsync_WithNutritionistFlag_CallsRepositoryWithCorrectFlag()
        {
            await service.AddMessageAsync(1, 2, "Hello", true);

            await mockRepo.Received(1).AddMessageAsync(1, 2, "Hello", true);
        }
    }
}