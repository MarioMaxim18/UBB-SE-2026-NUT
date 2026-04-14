using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;

namespace TeamNut.Repositories
{
    internal class ChatRepository : IChatRepository
    {
        private readonly string connectionString;

        public ChatRepository(IDbConfig dbConfig)
        {
            connectionString = dbConfig.ConnectionString;
        }

        public async Task<IEnumerable<Conversation>> GetAllConversationsAsync()
        {
            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id ORDER BY c.has_unanswered DESC, c.id DESC", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                });
            }
            return list;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync()
        {
            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id JOIN Users su ON m.sender_id = su.id WHERE su.role <> 'Nutritionist' ORDER BY c.has_unanswered DESC, c.id DESC", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                });
            }
            return list;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync()
        {
            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id ORDER BY c.has_unanswered DESC, c.id DESC", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                });
            }
            return list;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId)
        {
            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username FROM Conversations c JOIN Users u ON c.user_id = u.id JOIN Messages m ON m.conversation_id = c.id WHERE m.sender_id = @nid ORDER BY c.has_unanswered DESC, c.id DESC", conn);
            cmd.Parameters.AddWithValue("@nid", nutritionistId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                });
            }
            return list;
        }

        public async Task<Conversation> GetOrCreateConversationForUserAsync(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            using var check = new SqliteCommand("SELECT id, has_unanswered FROM Conversations WHERE user_id = @uid", conn);
            check.Parameters.AddWithValue("@uid", userId);
            await conn.OpenAsync();
            using var reader = await check.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Conversation
                {
                    Id = reader.GetInt32(0),
                    HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                    UserId = userId,
                };
            }
            reader.Close();

            using var ins = new SqliteCommand("INSERT INTO Conversations (user_id, has_unanswered) VALUES (@uid, 0); SELECT last_insert_rowid();", conn);
            ins.Parameters.AddWithValue("@uid", userId);
            var res = await ins.ExecuteScalarAsync();
            return new Conversation { Id = Convert.ToInt32(res), UserId = userId, HasUnanswered = false };
        }

        public async Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId)
        {
            var list = new List<Message>();
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT m.id, m.sent_at, m.conversation_id, m.sender_id, m.text_content, u.username, u.role FROM Messages m JOIN Users u ON m.sender_id = u.id WHERE m.conversation_id = @cid ORDER BY m.sent_at", conn);
            cmd.Parameters.AddWithValue("@cid", conversationId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Message
                {
                    Id = reader.GetInt32(0),
                    SentAt = reader.GetDateTime(1),
                    ConversationId = reader.GetInt32(2),
                    SenderId = reader.GetInt32(3),
                    TextContent = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    SenderUsername = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SenderRole = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                });
                var last = list[list.Count - 1];
                last.IsFromCurrentUser = UserSession.UserId.HasValue && last.SenderId == UserSession.UserId.Value;
            }
            return list;
        }

        public async Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("INSERT INTO Messages (conversation_id, sender_id, text_content) VALUES (@cid, @sid, @txt)", conn);
            cmd.Parameters.AddWithValue("@cid", conversationId);
            cmd.Parameters.AddWithValue("@sid", senderId);
            cmd.Parameters.AddWithValue("@txt", text);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            using var update = new SqliteCommand("UPDATE Conversations SET has_unanswered = CASE WHEN @isUser = 1 THEN 1 ELSE 0 END WHERE id = @cid", conn);
            update.Parameters.AddWithValue("@isUser", isNutritionist ? 0 : 1);
            update.Parameters.AddWithValue("@cid", conversationId);
            await update.ExecuteNonQueryAsync();
        }
    }
}
