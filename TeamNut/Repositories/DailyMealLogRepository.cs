using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class DailyMealLogRepository : IRepository<DailyMealLog>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task Add(DailyMealLog log)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO DailyMealLogs 
                (user_id, meal_id, meal_name, log_date, calories, protein, carbs, fat, meal_type)
                VALUES (@userId, @mealId, @mealName, @logDate, @calories, @protein, @carbs, @fat, @mealType)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", log.UserId);
            cmd.Parameters.AddWithValue("@mealId", log.MealId);
            cmd.Parameters.AddWithValue("@mealName", log.MealName);
            cmd.Parameters.AddWithValue("@logDate", log.LogDate);
            cmd.Parameters.AddWithValue("@calories", log.Calories);
            cmd.Parameters.AddWithValue("@protein", log.Protein);
            cmd.Parameters.AddWithValue("@carbs", log.Carbs);
            cmd.Parameters.AddWithValue("@fat", log.Fat);
            cmd.Parameters.AddWithValue("@mealType", log.MealType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DailyMealLog> GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT id, user_id, meal_id, meal_name, log_date, calories, protein, carbs, fat, meal_type FROM DailyMealLogs WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToLog(reader);
            }

            return null;
        }

        public async Task<IEnumerable<DailyMealLog>> GetAll()
        {
            var logs = new List<DailyMealLog>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT id, user_id, meal_id, meal_name, log_date, calories, protein, carbs, fat, meal_type FROM DailyMealLogs";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapReaderToLog(reader));
            }

            return logs;
        }

        public async Task Update(DailyMealLog log)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE DailyMealLogs 
                SET meal_name = @mealName,
                    calories = @calories,
                    protein = @protein,
                    carbs = @carbs,
                    fat = @fat,
                    meal_type = @mealType
                WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", log.Id);
            cmd.Parameters.AddWithValue("@mealName", log.MealName);
            cmd.Parameters.AddWithValue("@calories", log.Calories);
            cmd.Parameters.AddWithValue("@protein", log.Protein);
            cmd.Parameters.AddWithValue("@carbs", log.Carbs);
            cmd.Parameters.AddWithValue("@fat", log.Fat);
            cmd.Parameters.AddWithValue("@mealType", log.MealType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "DELETE FROM DailyMealLogs WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<DailyMealLog>> GetByUserAndDate(int userId, DateTime date)
        {
            var logs = new List<DailyMealLog>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT id, user_id, meal_id, meal_name, log_date, calories, protein, carbs, fat, meal_type 
                FROM DailyMealLogs 
                WHERE user_id = @userId AND CAST(log_date AS DATE) = CAST(@date AS DATE)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@date", date);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapReaderToLog(reader));
            }

            return logs;
        }

        public async Task<IEnumerable<DailyMealLog>> GetByUserAndDateRange(int userId, DateTime startDate, DateTime endDate)
        {
            var logs = new List<DailyMealLog>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT id, user_id, meal_id, meal_name, log_date, calories, protein, carbs, fat, meal_type 
                FROM DailyMealLogs 
                WHERE user_id = @userId AND log_date >= @startDate AND log_date < @endDate";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapReaderToLog(reader));
            }

            return logs;
        }

        private DailyMealLog MapReaderToLog(SqlDataReader reader)
        {
            return new DailyMealLog
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                MealId = reader.GetInt32(2),
                MealName = reader.GetString(3),
                LogDate = reader.GetDateTime(4),
                Calories = reader.GetInt32(5),
                Protein = reader.GetInt32(6),
                Carbs = reader.GetInt32(7),
                Fat = reader.GetInt32(8),
                MealType = reader.GetString(9)
            };
        }
    }
}
