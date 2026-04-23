using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using TeamNut.Models;
using TeamNut.Services;
using TeamNut.Services.Interfaces;
using TeamNut.ViewModels;
using Xunit;

namespace TeamNut.Tests.ViewModels
{
    [Collection("UsesStaticUserSession")]
    public class NutritionistChatViewModelTests
    {
        [Fact]
        public void InputText_Setter_WhenNutritionistWithoutConversation_SetsCannotSend()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestNut", "Nutritionist");
            var vm = new NutritionistChatViewModel(chatService);

            vm.InputText = "Hello";

            Assert.False(vm.CanSend);
            Assert.Equal("Please select a conversation to respond.", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public void InputText_Setter_WhenMessageTooLong_SetsCannotSend()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestUser", "User");
            var vm = new NutritionistChatViewModel(chatService);

            vm.InputText = new string('A', 1001);

            Assert.False(vm.CanSend);
            Assert.Equal("Message too long.", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public void InputText_Setter_WhenInvalidCharacters_SetsCannotSend()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestUser", "User");
            var vm = new NutritionistChatViewModel(chatService);

            vm.InputText = "Hello <script>!";

            Assert.False(vm.CanSend);
            Assert.Equal("Only alphanumeric characters and basic punctuation are allowed.", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public void InputText_Setter_WhenValid_SetsCanSend()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestUser", "User");
            var vm = new NutritionistChatViewModel(chatService);

            vm.InputText = "Hello, this is a valid message!";

            Assert.True(vm.CanSend);
            Assert.Equal(string.Empty, vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task LoadConversationsAsync_WhenNutritionistInGlobalView_LoadsAllMessages()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestNut", "Nutritionist");
            var vm = new NutritionistChatViewModel(chatService) { IsNutritionistView = true };
            var convs = new List<Conversation> { new Conversation { Id = 1 } };

            chatService.GetConversationsWithUserMessagesAsync().Returns(convs);

            await vm.LoadConversationsAsync();

            Assert.Single(vm.Conversations);
            Assert.Equal(1, vm.Conversations[0].Id);

            UserSession.Logout();
        }

        [Fact]
        public async Task LoadConversationsAsync_WhenStandardUser_LoadsSingleConversation()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(2, "TestUser", "User");
            var vm = new NutritionistChatViewModel(chatService);
            var conv = new Conversation { Id = 5 };

            chatService.GetOrCreateConversationForUserAsync(2).Returns(conv);

            await vm.LoadConversationsAsync();

            Assert.Single(vm.Conversations);
            Assert.Equal(5, vm.Conversations[0].Id);
            Assert.Equal(conv, vm.SelectedConversation);

            UserSession.Logout();
        }

        [Fact]
        public async Task SendMessageCommand_WhenNutritionistAndNoConversation_SetsStatusMessage()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(1, "TestNut", "Nutritionist");
            var vm = new NutritionistChatViewModel(chatService);
            vm.InputText = "Hello";

            await vm.SendMessageCommand.ExecuteAsync(null);

            Assert.Equal("Nutritionists can only respond to existing conversations.", vm.StatusMessage);

            UserSession.Logout();
        }

        [Fact]
        public async Task SendMessageCommand_WhenUserAndNoConversation_CreatesConversationAndSends()
        {
            var chatService = Substitute.For<IChatService>();
            UserSession.Login(2, "TestUser", "User");
            var vm = new NutritionistChatViewModel(chatService);
            var newConv = new Conversation { Id = 10 };

            chatService.GetOrCreateConversationForUserAsync(2).Returns(newConv);
            chatService.GetMessagesForConversationAsync(10).Returns(new List<Message>());

            vm.InputText = "I need help.";

            await vm.SendMessageCommand.ExecuteAsync(null);

            await chatService.Received(1).AddMessageAsync(10, 2, "I need help.", false);
            Assert.Equal(string.Empty, vm.InputText);

            UserSession.Logout();
        }
    }
}