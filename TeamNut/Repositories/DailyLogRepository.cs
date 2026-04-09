using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class DailyLogRepository
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task Add(DailyLog log)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                   VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", log.UserId);
            command.Parameters.AddWithValue("@mealId", log.MealId);
            command.Parameters.AddWithValue("@calories", log.Calories);
            command.Parameters.AddWithValue("@loggedAt", log.LoggedAt);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> HasAnyLogs(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "SELECT COUNT(1) FROM DailyLogs WHERE user_id = @userId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            var dailyLogCount = Convert.ToInt32(await command.ExecuteScalarAsync());
            return dailyLogCount > 0;
        }

        public async Task<DailyLog> GetNutritionTotalsForRange(int userId, DateTime startInclusive, DateTime endExclusive)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
                SELECT
                    COALESCE(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS total_calories,
                    COALESCE(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS total_protein,
                    COALESCE(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS total_carbs,
                    COALESCE(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS total_fats
                FROM DailyLogs dl
                INNER JOIN Meals m ON m.meal_id = dl.mealId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON i.food_id = mi.food_id
                WHERE dl.user_id = @userId
                  AND dl.created_at >= @startDate
                  AND dl.created_at < @endDate";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@startDate", startInclusive);
            command.Parameters.AddWithValue("@endDate", endExclusive);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DailyLog
                {
                    UserId = userId,
                    LoggedAt = startInclusive,
                    Calories = Convert.ToDouble(reader["total_calories"]),
                    Protein = Convert.ToDouble(reader["total_protein"]),
                    Carbohydrates = Convert.ToDouble(reader["total_carbs"]),
                    Fats = Convert.ToDouble(reader["total_fats"])
                };
            }

            return new DailyLog
            {
                UserId = userId,
                LoggedAt = startInclusive
            };
        }
    }
}
