namespace TeamNut.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    public class ChatService : IChatService
    {
        private readonly IChatRepository repo;

        public ChatService(IChatRepository rrepo)
        {
            repo = rrepo;
        }

        public Task<IEnumerable<Conversation>> GetAllConversationsAsync() => repo.GetAllConversationsAsync();

        public Task<Conversation> GetOrCreateConversationForUserAsync(int userId) => repo.GetOrCreateConversationForUserAsync(userId);

        public Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId) => repo.GetMessagesForConversationAsync(conversationId);

        public Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync() => repo.GetConversationsWithMessagesAsync();

        public Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId) => repo.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

        public Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist) => repo.AddMessageAsync(conversationId, senderId, text, isNutritionist);

        public Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync() => repo.GetConversationsWithUserMessagesAsync();
    }
}
