namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    internal class ReminderRepository : IReminderRepository
    {
        private readonly string connectionString;

        public ReminderRepository(IDbConfig dbConfig)
        {
            connectionString = dbConfig.ConnectionString;
        }

        public async Task<Reminder?> GetById(int id)
        {
            const string sql = "SELECT * FROM Reminders WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToReminder(reader);
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<Reminder>> GetAll()
        {
            const string sql = "SELECT * FROM Reminders";
            var reminders = new List<Reminder>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reminders.Add(MapReaderToReminder(reader));
                    }
                }
            }

            return reminders;
        }

        public async Task<IEnumerable<Reminder>> GetAllByUserId(int userId)
        {
            const string sql = "SELECT * FROM Reminders WHERE user_id = @uid";
            var reminders = new List<Reminder>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@uid", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reminders.Add(MapReaderToReminder(reader));
                    }
                }
            }

            return reminders;
        }

        public async Task Add(Reminder entity)
        {
            const string sql = @"
                INSERT INTO Reminders (user_id, name, has_sound, time, reminder_date, frequency)
                VALUES (@uid, @name, @sound, @time, @date, @freq)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@uid", entity.UserId);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
                command.Parameters.AddWithValue("@time", entity.Time.ToString());
                command.Parameters.AddWithValue("@date", entity.ReminderDate);
                command.Parameters.AddWithValue("@freq", entity.Frequency ?? string.Empty);

                await command.ExecuteNonQueryAsync();
            }

            using (var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn))
            {
                var scalar = await idCmd.ExecuteScalarAsync();

                if (scalar != null && long.TryParse(scalar.ToString(), out var lastId))
                {
                    entity.Id = Convert.ToInt32(lastId);
                }
            }
        }

        public async Task Update(Reminder entity)
        {
            const string sql = @"
                UPDATE Reminders
                SET name = @name, has_sound = @sound, time = @time,
                    reminder_date = @date, frequency = @freq
                WHERE id = @id AND user_id = @uid";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@id", entity.Id);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
                command.Parameters.AddWithValue("@time", entity.Time.ToString());
                command.Parameters.AddWithValue("@date", entity.ReminderDate);
                command.Parameters.AddWithValue("@freq", entity.Frequency ?? string.Empty);
                command.Parameters.AddWithValue("@uid", entity.UserId);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            const string sql = "DELETE FROM Reminders WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Reminder?> GetNextReminder(int userId)
        {
            const string sql = @"
                SELECT * FROM Reminders
                WHERE user_id = @uid AND
                (reminder_date > date('now', 'localtime')
                 OR (reminder_date = date('now', 'localtime') AND time >= time('now', 'localtime')))
                ORDER BY reminder_date ASC, time ASC
                LIMIT 1";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@uid", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToReminder(reader);
                    }
                }
            }

            return null;
        }

        private Reminder MapReaderToReminder(SqliteDataReader reader)
        {
            return new Reminder
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Name = reader["name"]?.ToString() ?? string.Empty,
                HasSound = Convert.ToBoolean(reader["has_sound"]),
                Time = TimeSpan.Parse(reader["time"]?.ToString() ?? "00:00:00"),
                ReminderDate = reader["reminder_date"]?.ToString() ?? string.Empty,
                Frequency = reader["frequency"]?.ToString() ?? string.Empty,
            };
        }
    }
}
