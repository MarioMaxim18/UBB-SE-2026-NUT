using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class CalorieLogRepository : IRepository<CalorieLog>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;
        public async Task Add(CalorieLog log)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO CalorieLogs 
                (UserId, Date, CaloriesConsumed, CaloriesBurnt, Protein, Carbs, Fats)
                VALUES (@userId, @date, @caloriesConsumed, @protein, @carbs, @fats)";

            using var cmd = new SqlCommand(query, conn);
            AddParameters(cmd, log);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<CalorieLog>> GetAll()
        {
            var logs = new List<CalorieLog>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM CalorieLogs";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapReaderToLog(reader));
            }

            return logs;
        }

        public async Task<CalorieLog> GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM CalorieLogs WHERE Id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToLog(reader);
            }

            return null;
        }

        public async Task Update(CalorieLog log)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE CalorieLogs 
                SET CaloriesConsumed = @caloriesConsumed,
                    Protein = @protein,
                    Carbs = @carbs,
                    Fats = @fats
                WHERE Id = @id";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", log.Id);
            AddParameters(cmd, log);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "DELETE FROM CalorieLogs WHERE Id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<CalorieLog> GetByUserAndDate(int userId, DateTime date)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT * FROM CalorieLogs 
                             WHERE UserId = @userId AND CAST(Date AS DATE) = @date";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToLog(reader);
            }

            return null;
        }

        public async Task<List<CalorieLog>> GetByUserAndDateRange(int userId, DateTime start, DateTime end)
        {
            var logs = new List<CalorieLog>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT * FROM CalorieLogs 
                             WHERE UserId = @userId 
                             AND Date >= @start AND Date <= @end";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@start", start.Date);
            cmd.Parameters.AddWithValue("@end", end.Date);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapReaderToLog(reader));
            }

            return logs;
        }

        private CalorieLog MapReaderToLog(SqlDataReader reader)
        {
            return new CalorieLog
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Date = Convert.ToDateTime(reader["Date"]),
                CaloriesConsumed = Convert.ToDouble(reader["CaloriesConsumed"]),
                Protein = Convert.ToDouble(reader["Protein"]),
                Carbs = Convert.ToDouble(reader["Carbs"]),
                Fats = Convert.ToDouble(reader["Fats"])
            };
        }

        private void AddParameters(SqlCommand cmd, CalorieLog log)
        {
            cmd.Parameters.AddWithValue("@userId", log.UserId);
            cmd.Parameters.AddWithValue("@date", log.Date);
            cmd.Parameters.AddWithValue("@caloriesConsumed", log.CaloriesConsumed);
            cmd.Parameters.AddWithValue("@protein", log.Protein);
            cmd.Parameters.AddWithValue("@carbs", log.Carbs);
            cmd.Parameters.AddWithValue("@fats", log.Fats);
        }
    }
}