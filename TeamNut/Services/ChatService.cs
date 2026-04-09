using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    public class ChatService
    {
        private readonly ChatRepository _repo;
        public ChatService()
        {
            _repo = new ChatRepository();
        }

        public Task<IEnumerable<Conversation>> GetAllConversationsAsync() => _repo.GetAllConversationsAsync();

        public Task<Conversation> GetOrCreateConversationForUserAsync(int userId) => _repo.GetOrCreateConversationForUserAsync(userId);

        public Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId) => _repo.GetMessagesForConversationAsync(conversationId);

        public Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync() => _repo.GetConversationsWithMessagesAsync();

        public Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId) => _repo.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

        public Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist) => _repo.AddMessageAsync(conversationId, senderId, text, isNutritionist);

        public Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync() => _repo.GetConversationsWithUserMessagesAsync();

        public (bool CanSend, string StatusMessage) ValidateMessageInput(string? inputText, bool hasConversation, bool isNutritionist)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return (false, string.Empty);
            }

            if (isNutritionist && !hasConversation)
            {
                return (false, "Please select a conversation to respond.");
            }

            if (inputText.Length > 1000)
            {
                return (false, "message too long");
            }

            if (!Regex.IsMatch(inputText, "^[a-zA-Z0-9 .,!?'\\-()]+$"))
            {
                return (false, "Only alphanumeric characters and basic punctuation are allowed.");
            }

            return (true, string.Empty);
        }
    }
}
