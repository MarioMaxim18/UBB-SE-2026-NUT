using System.Collections.Generic;
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

        public Task<IEnumerable<Conversation>> GetAllConversationsAsync()
            => _repo.GetAllConversationsAsync();

        public Task<Conversation> GetOrCreateConversationAsync(int userId)
            => _repo.GetOrCreateConversationForUserAsync(userId);

        public Task<IEnumerable<Message>> GetMessagesAsync(int conversationId)
            => _repo.GetMessagesForConversationAsync(conversationId);

        public Task<IEnumerable<Conversation>> GetActiveConversationsAsync()
            => _repo.GetConversationsWithMessagesAsync();

        public Task<IEnumerable<Conversation>> GetNutritionistRepliesAsync(int nutritionistId)
            => _repo.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

        public Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist)
            => _repo.AddMessageAsync(conversationId, senderId, text, isNutritionist);

        public Task<IEnumerable<Conversation>> GetUserMessagesAsync()
            => _repo.GetConversationsWithUserMessagesAsync();
    }
}
