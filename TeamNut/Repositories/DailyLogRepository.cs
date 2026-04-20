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
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                   VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", log.UserId);
            cmd.Parameters.AddWithValue("@mealId", log.MealId);
            cmd.Parameters.AddWithValue("@calories", log.Calories);
            cmd.Parameters.AddWithValue("@loggedAt", log.LoggedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> HasAnyLogs(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string query = "SELECT COUNT(1) FROM DailyLogs WHERE user_id = @userId";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<DailyLog> GetNutritionTotalsForRange(int userId, DateTime startInclusive, DateTime endExclusive)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

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

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@startDate", startInclusive);
            cmd.Parameters.AddWithValue("@endDate", endExclusive);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DailyLog
                {
                    UserId = userId,
                    LoggedAt = startInclusive,
                    Calories = Convert.ToDouble(reader["total_calories"]),
                    Protein = Convert.ToDouble(reader["total_protein"]),
                    Carbs = Convert.ToDouble(reader["total_carbs"]),
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
