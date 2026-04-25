namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    internal class ChatRepository : IChatRepository
    {
        private readonly string connectionString;

        public ChatRepository(IDbConfig dbConfig)
        {
            this.connectionString = dbConfig.ConnectionString;
        }

        public async Task<IEnumerable<Conversation>> GetAllConversationsAsync()
        {
            const string sql = @"
                SELECT c.id, c.has_unanswered, c.user_id, u.username 
                FROM Conversations c 
                JOIN Users u ON c.user_id = u.id 
                ORDER BY c.has_unanswered DESC, c.id DESC";

            return await ExecuteConversationQueryAsync(sql);
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithUserMessagesAsync()
        {
            const string sql = @"
                SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username 
                FROM Conversations c 
                JOIN Users u ON c.user_id = u.id 
                JOIN Messages m ON m.conversation_id = c.id 
                JOIN Users su ON m.sender_id = su.id 
                WHERE su.role <> 'Nutritionist' 
                ORDER BY c.has_unanswered DESC, c.id DESC";

            return await ExecuteConversationQueryAsync(sql);
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWithMessagesAsync()
        {
            const string sql = @"
                SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username 
                FROM Conversations c 
                JOIN Users u ON c.user_id = u.id 
                JOIN Messages m ON m.conversation_id = c.id 
                ORDER BY c.has_unanswered DESC, c.id DESC";

            return await ExecuteConversationQueryAsync(sql);
        }

        public async Task<IEnumerable<Conversation>> GetConversationsWhereNutritionistRespondedAsync(int nutritionistId)
        {
            const string sql = @"
                SELECT DISTINCT c.id, c.has_unanswered, c.user_id, u.username 
                FROM Conversations c 
                JOIN Users u ON c.user_id = u.id 
                JOIN Messages m ON m.conversation_id = c.id 
                WHERE m.sender_id = @nid 
                ORDER BY c.has_unanswered DESC, c.id DESC";

            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var command = new SqliteCommand(sql, conn);

            command.Parameters.AddWithValue("@nid", nutritionistId);

            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToConversation(reader));
            }

            return list;
        }

        public async Task<Conversation> GetOrCreateConversationForUserAsync(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            const string checkSql = "SELECT id, has_unanswered FROM Conversations WHERE user_id = @uid";
            using (var checkCmd = new SqliteCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("@uid", userId);
                using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Conversation
                        {
                            Id = reader.GetInt32(0),
                            HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                            UserId = userId,
                        };
                    }
                }
            }

            const string insertSql = @"
                INSERT INTO Conversations (user_id, has_unanswered) 
                VALUES (@uid, 0); 
                SELECT last_insert_rowid();";

            using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@uid", userId);

            var resultId = await insertCmd.ExecuteScalarAsync();

            return new Conversation
            {
                Id = Convert.ToInt32(resultId),
                UserId = userId,
                HasUnanswered = false
            };
        }

        public async Task<IEnumerable<Message>> GetMessagesForConversationAsync(int conversationId)
        {
            const string sql = @"
                SELECT m.id, m.sent_at, m.conversation_id, m.sender_id, m.text_content, u.username, u.role 
                FROM Messages m 
                JOIN Users u ON m.sender_id = u.id 
                WHERE m.conversation_id = @cid 
                ORDER BY m.sent_at";

            var list = new List<Message>();
            using var conn = new SqliteConnection(connectionString);
            using var command = new SqliteCommand(sql, conn);

            command.Parameters.AddWithValue("@cid", conversationId);

            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var message = new Message
                {
                    Id = reader.GetInt32(0),
                    SentAt = reader.GetDateTime(1),
                    ConversationId = reader.GetInt32(2),
                    SenderId = reader.GetInt32(3),
                    TextContent = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    SenderUsername = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SenderRole = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                };

                message.IsFromCurrentUser = UserSession.UserId.HasValue && message.SenderId == UserSession.UserId.Value;
                list.Add(message);
            }

            return list;
        }

        public async Task AddMessageAsync(int conversationId, int senderId, string text, bool isNutritionist)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            const string insertSql = @"
                INSERT INTO Messages (conversation_id, sender_id, text_content) 
                VALUES (@cid, @sid, @txt)";

            using (var command = new SqliteCommand(insertSql, conn))
            {
                command.Parameters.AddWithValue("@cid", conversationId);
                command.Parameters.AddWithValue("@sid", senderId);
                command.Parameters.AddWithValue("@txt", text);
                await command.ExecuteNonQueryAsync();
            }

            const string updateSql = @"
                UPDATE Conversations 
                SET has_unanswered = CASE WHEN @isUser = 1 THEN 1 ELSE 0 END 
                WHERE id = @cid";

            using (var updateCmd = new SqliteCommand(updateSql, conn))
            {
                updateCmd.Parameters.AddWithValue("@isUser", isNutritionist ? 0 : 1);
                updateCmd.Parameters.AddWithValue("@cid", conversationId);
                await updateCmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<IEnumerable<Conversation>> ExecuteConversationQueryAsync(string query)
        {
            var list = new List<Conversation>();
            using var conn = new SqliteConnection(connectionString);
            using var command = new SqliteCommand(query, conn);

            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToConversation(reader));
            }

            return list;
        }

        private Conversation MapReaderToConversation(SqliteDataReader reader)
        {
            return new Conversation
            {
                Id = reader.GetInt32(0),
                HasUnanswered = Convert.ToBoolean(reader.GetValue(1)),
                UserId = reader.GetInt32(2),
                Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            };
        }
    }
}
