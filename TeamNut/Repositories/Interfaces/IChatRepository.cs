using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist);
        Task<IEnumerable<Conversation>> GetAllConversationsAsync();
        Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId);
        Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync();
        Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync();
        Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId);
        Task<Conversation> GetOrCreateConversationForUserAsync(int userId);
    }
}
