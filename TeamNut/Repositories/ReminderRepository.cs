using TeamNut.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TeamNut;
using TeamNut.Repositories;

namespace TeamNut.Repositories
{
    internal class ReminderRepository : IRepository<Reminder>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<Reminder> GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT * FROM Reminders WHERE id = @id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToReminder(reader);
            }
            return null;
        }

        public async Task<IEnumerable<Reminder>> GetAll()
        {
            var remindersList = new List<Reminder>();
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT * FROM Reminders";
            using var command = new SqliteCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                remindersList.Add(MapReaderToReminder(reader));
            }
            return remindersList;
        }

        public async Task<IEnumerable<Reminder>> GetAllByUserId(int userId)
        {
            var remindersList = new List<Reminder>();
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT * FROM Reminders WHERE user_id = @userId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                remindersList.Add(MapReaderToReminder(reader));
            }
            return remindersList;
        }

        public async Task Add(Reminder reminder)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"INSERT INTO Reminders (user_id, name, has_sound, time, reminder_date, frequency) 
                                  VALUES (@userId, @reminderName, @hasSound, @reminderTime, @reminderDate, @frequency)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", reminder.UserId);
            command.Parameters.AddWithValue("@reminderName", reminder.Name);
            command.Parameters.AddWithValue("@hasSound", reminder.HasSound ? 1 : 0);
            command.Parameters.AddWithValue("@reminderTime", reminder.Time.ToString());
            command.Parameters.AddWithValue("@reminderDate", reminder.ReminderDate);
            command.Parameters.AddWithValue("@frequency", reminder.Frequency ?? string.Empty);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            // Get the ID of the newly inserted reminder
            using var idCommand = new SqliteCommand("SELECT last_insert_rowid();", connection);
            var insertedId = await idCommand.ExecuteScalarAsync();
            if (insertedId != null && long.TryParse(insertedId.ToString(), out var newReminderId))
            {
                reminder.Id = Convert.ToInt32(newReminderId);
            }
        }

        public async Task Update(Reminder reminder)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"UPDATE Reminders 
                                  SET name = @reminderName, has_sound = @hasSound, time = @reminderTime, 
                                      reminder_date = @reminderDate, frequency = @frequency 
                                  WHERE id = @id AND user_id = @userId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", reminder.Id);
            command.Parameters.AddWithValue("@reminderName", reminder.Name);
            command.Parameters.AddWithValue("@hasSound", reminder.HasSound ? 1 : 0);
            command.Parameters.AddWithValue("@reminderTime", reminder.Time.ToString());
            command.Parameters.AddWithValue("@reminderDate", reminder.ReminderDate);
            command.Parameters.AddWithValue("@frequency", reminder.Frequency ?? string.Empty);
            command.Parameters.AddWithValue("@userId", reminder.UserId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task Delete(int reminderId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "DELETE FROM Reminders WHERE id = @reminderId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@reminderId", reminderId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<Reminder> GetNextReminder(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"SELECT * FROM Reminders 
                                  WHERE user_id = @userId AND 
                                  (reminder_date > date('now', 'localtime') 
                                   OR (reminder_date = date('now', 'localtime') AND time >= time('now', 'localtime')))
                                  ORDER BY reminder_date ASC, time ASC 
                                  LIMIT 1";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToReminder(reader) : null;
        }

        private Reminder MapReaderToReminder(SqliteDataReader reader)
        {
            return new Reminder
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Name = reader["name"].ToString(),
                HasSound = Convert.ToBoolean(reader["has_sound"]),
                Time = TimeSpan.Parse(reader["time"].ToString()),
                ReminderDate = reader["reminder_date"].ToString(),
                Frequency = reader["frequency"].ToString()
            };
        }
    }
}