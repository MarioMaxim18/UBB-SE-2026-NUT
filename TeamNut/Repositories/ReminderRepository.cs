using TeamNut.Models;
using Microsoft.Data.SqlClient;
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
            using var conn = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Reminders WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
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
            using var conn = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Reminders";
            using var cmd = new SqlCommand(sql, conn);

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
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"INSERT INTO Reminders (user_id, name, has_sound, time, frequency) 
                                VALUES (@uid, @name, @sound, @time, @freq)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound);
            cmd.Parameters.AddWithValue("@time", entity.Time);
            cmd.Parameters.AddWithValue("@freq", entity.Frequency);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Reminder entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"UPDATE Reminders SET name = @name, has_sound = @sound, 
                                 time = @time, frequency = @freq WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@sound", entity.HasSound);
            cmd.Parameters.AddWithValue("@time", entity.Time);
            cmd.Parameters.AddWithValue("@freq", entity.Frequency);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Reminders WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        
        private Reminder MapReaderToReminder(SqlDataReader reader)
        {
            return new Reminder
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Name = reader["name"].ToString(),
                HasSound = Convert.ToBoolean(reader["has_sound"]),
                Time = (TimeSpan)reader["time"],
                Frequency = reader["frequency"].ToString()
            };
        }
    }
}
