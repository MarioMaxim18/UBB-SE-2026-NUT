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
            using var conn = new SqliteConnection(_connectionString);
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
            using var conn = new SqliteConnection(_connectionString);
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
            using var conn = new SqliteConnection(_connectionString);
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
            using var conn = new SqliteConnection(_connectionString);
           
            const string sql = @"INSERT INTO Reminders (user_id, name, has_sound, time, reminder_date, frequency) 
                        VALUES (@uid, @name, @sound, @time, @date, @freq)";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
            cmd.Parameters.AddWithValue("@time", entity.Time);
            cmd.Parameters.AddWithValue("@date", entity.ReminderDate); 
            cmd.Parameters.AddWithValue("@freq", entity.Frequency);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Reminder entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            
            const string sql = @"UPDATE Reminders 
                         SET name = @name, has_sound = @sound, time = @time, 
                             reminder_date = @date, frequency = @freq 
                         WHERE id = @id AND user_id = @uid";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound ? 1 : 0);
            cmd.Parameters.AddWithValue("@time", entity.Time);
            cmd.Parameters.AddWithValue("@date", entity.ReminderDate);
            cmd.Parameters.AddWithValue("@freq", entity.Frequency);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
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

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "DELETE FROM Reminders WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Reminder> GetNextReminder(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            
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

        /*public async Task<Reminder> GetNextReminder(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"SELECT * FROM Reminders 
                         WHERE user_id = @uid AND 
                         (reminder_date > date('now') OR (reminder_date = date('now') AND time >= time('now')))
                         ORDER BY reminder_date ASC, time ASC 
                         LIMIT 1";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToReminder(reader) : null;
        }*/


    }
}
