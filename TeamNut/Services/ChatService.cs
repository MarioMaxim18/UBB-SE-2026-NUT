namespace TeamNut.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public Task<IEnumerable<Conversation>> GetAllConversationsAsync() => chatRepository.GetAllConversationsAsync();

        public Task<Conversation> GetOrCreateConversationForUserAsync(int userId) => chatRepository.GetOrCreateConversationForUserAsync(userId);

        public Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId) => chatRepository.GetMessagesForConversationAsync(conversationId);

        public Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync() => chatRepository.GetConversationsWithMessagesAsync();

        public Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId) => chatRepository.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

        public Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist) => chatRepository.AddMessageAsync(conversationId, senderId, text, isNutritionist);

        public Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync() => chatRepository.GetConversationsWithUserMessagesAsync();
    }
}
