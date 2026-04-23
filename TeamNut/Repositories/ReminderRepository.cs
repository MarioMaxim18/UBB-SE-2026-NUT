// <copyright file="ReminderRepository.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut;
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
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM Reminders WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToReminder(reader);
            }
            return null;
        }

        public async Task<IEnumerable<Reminder>> GetAll()
        {
            var reminders = new List<Reminder>();
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM Reminders";
            using var cmd = new SqliteCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reminders.Add(MapReaderToReminder(reader));
            }
            return reminders;
        }

        public async Task<IEnumerable<Reminder>> GetAllByUserId(int userId)
        {
            var reminders = new List<Reminder>();
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM Reminders WHERE user_id = @uid";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reminders.Add(MapReaderToReminder(reader));
            }
            return reminders;
        }

        public async Task Add(Reminder entity)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = @"INSERT INTO Reminders (user_id, name, has_sound, time, reminder_date, frequency)
                        VALUES (@uid, @name, @sound, @time, @date, @freq)";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
            cmd.Parameters.AddWithValue("@time", entity.Time.ToString());
            cmd.Parameters.AddWithValue("@date", entity.ReminderDate);
            cmd.Parameters.AddWithValue("@freq", entity.Frequency ?? string.Empty);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            using var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn);
            var scalar = await idCmd.ExecuteScalarAsync();
            if (scalar != null && long.TryParse(scalar.ToString(), out var lastId))
            {
                entity.Id = Convert.ToInt32(lastId);
            }
        }

        public async Task Update(Reminder entity)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = @"UPDATE Reminders
                         SET name = @name, has_sound = @sound, time = @time,
                             reminder_date = @date, frequency = @freq
                         WHERE id = @id AND user_id = @uid";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
            cmd.Parameters.AddWithValue("@time", entity.Time.ToString());
            cmd.Parameters.AddWithValue("@date", entity.ReminderDate);
            cmd.Parameters.AddWithValue("@freq", entity.Frequency ?? string.Empty);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
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

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = "DELETE FROM Reminders WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Reminder?> GetNextReminder(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = @"SELECT * FROM Reminders
                         WHERE user_id = @uid AND
                         (reminder_date > date('now', 'localtime')
                          OR (reminder_date = date('now', 'localtime') AND time >= time('now', 'localtime')))
                         ORDER BY reminder_date ASC, time ASC
                         LIMIT 1";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToReminder(reader) : null;
        }
    }
}
