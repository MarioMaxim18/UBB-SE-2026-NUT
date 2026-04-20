using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories;

namespace TeamNut.Services
{
    /// <summary>Service for nutritionist chat conversations and messages.</summary>
    public class ChatService
    {
        private readonly ChatRepository repo;

        /// <summary>Initializes a new instance of the <see cref="ChatService"/> class.</summary>
        public ChatService()
        {
            repo = new ChatRepository();
        }

        /// <summary>Gets all conversations.</summary>
        /// <returns>All conversations in the system.</returns>
        public Task<IEnumerable<Conversation>> GetAllConversationsAsync() => repo.GetAllConversationsAsync();

        /// <summary>Gets or creates a conversation for the given user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The existing or newly created conversation.</returns>
        public Task<Conversation> GetOrCreateConversationForUserAsync(int userId) => repo.GetOrCreateConversationForUserAsync(userId);

        /// <summary>Gets messages for the given conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns>All messages in the conversation.</returns>
        public Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId) => repo.GetMessagesForConversationAsync(conversationId);

        /// <summary>Gets conversations that contain at least one message.</summary>
        /// <returns>Conversations with messages.</returns>
        public Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync() => repo.GetConversationsWithMessagesAsync();

        /// <summary>Gets conversations where a specific nutritionist has responded.</summary>
        /// <param name="nutritionistId">The nutritionist user identifier.</param>
        /// <returns>Conversations the nutritionist responded to.</returns>
        public Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId) => repo.GetConversationsWhereNutritionistRespondedAsync(nutritionistId);

        /// <summary>Adds a message to a conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="senderId">The sender's user identifier.</param>
        /// <param name="text">The message text.</param>
        /// <param name="isNutritionist"><c>true</c> if the sender is a nutritionist.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist) => repo.AddMessageAsync(conversationId, senderId, text, isNutritionist);

        /// <summary>Gets conversations that have unanswered user messages.</summary>
        /// <returns>Conversations with unanswered messages.</returns>
        public Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync() => repo.GetConversationsWithUserMessagesAsync();
    }
}
