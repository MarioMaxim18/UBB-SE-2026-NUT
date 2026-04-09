using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class ChatRepository
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<IEnumerable<Conversation>> GetAllConversationsAsync()
        {
            var conversationList = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("SELECT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id ORDER BY c.has_unanswered DESC, c.id DESC", connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversationList.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnansweredMessages = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.GetString(3)
                });
            }
            return conversationList;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync()
        {
            var conversationList = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id JOIN Users su ON m.sender_id = su.id WHERE su.role <> 'Nutritionist' ORDER BY c.has_unanswered DESC, c.id DESC", connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversationList.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnansweredMessages = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
            return conversationList;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync()
        {
            var conversationList = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id ORDER BY c.has_unanswered DESC, c.id DESC", connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversationList.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnansweredMessages = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
            return conversationList;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId)
        {
            var conversationList = new List<Conversation>();
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id WHERE m.sender_id = @nutritionistid ORDER BY c.has_unanswered DESC, c.id DESC", connection);
            command.Parameters.AddWithValue("@nutritionistid", nutritionistId);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversationList.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnansweredMessages = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
            return conversationList;
        }

        public async Task<Conversation> GetOrCreateConversationForUserAsync(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            using var checkCommand = new SqliteCommand("SELECT id, has_unanswered FROM Conversations WHERE user_id = @userid", connection);
            checkCommand.Parameters.AddWithValue("@userid", userId);
            await connection.OpenAsync();
            using var reader = await checkCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnansweredMessages = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = userId
                };
            }
            reader.Close();

            using var insertCommand = new SqliteCommand("INSERT INTO Conversations (user_id, has_unanswered) VALUES (@userid, 0); SELECT last_insert_rowid();", connection);
            insertCommand.Parameters.AddWithValue("@userid", userId);
            var insertedId = await insertCommand.ExecuteScalarAsync();
            return new Conversation { Id = Convert.ToInt32(insertedId), UserId = userId, HasUnansweredMessages = false };
        }

        public async Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId)
        {
            var messageList = new List<Message>();
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("SELECT m.id, m.sent_at, m.conversation_id, m.sender_id, m.text_content, u.username, u.role FROM Messages m JOIN Users u ON m.sender_id = u.id WHERE m.conversation_id = @cid ORDER BY m.sent_at", connection);
            command.Parameters.AddWithValue("@cid", conversationId);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messageList.Add(new Message
                {
                    Id = reader.GetInt32(0),
                    SentAt = reader.GetDateTime(1),
                    ConversationId = reader.GetInt32(2),
                    SenderId = reader.GetInt32(3),
                    TextContent = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    SenderUsername = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SenderRole = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
                // set helper fields
                var currentMessage = messageList[messageList.Count - 1];
                try
                {
                    currentMessage.IsFromCurrentUser = currentMessage.SenderId == TeamNut.Models.UserSession.UserId;
                }
                catch { }
            }
            return messageList;
        }

        public async Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist)
        {
            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand("INSERT INTO Messages (conversation_id, sender_id, text_content) VALUES (@conversationid, @senderid, @text)", connection);
            command.Parameters.AddWithValue("@conversationid", conversationId);
            command.Parameters.AddWithValue("@senderid", senderId);
            command.Parameters.AddWithValue("@text", text);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            // if sender is user (not nutritionist), mark conversation as unanswered
            using var updateCommand = new SqliteCommand("UPDATE Conversations SET has_unanswered = CASE WHEN @isUser = 1 THEN 1 ELSE 0 END WHERE id = @conversationid", connection);
            // @isUser should be 1 when the sender is a regular user, 0 when sender is a nutritionist
            updateCommand.Parameters.AddWithValue("@isUser", isNutritionist ? 0 : 1);
            updateCommand.Parameters.AddWithValue("@conversationid", conversationId);
            await updateCommand.ExecuteNonQueryAsync();
        }
    }
}
