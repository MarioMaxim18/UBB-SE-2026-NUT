namespace TeamNut.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    public class DailyLogRepository : IDailyLogRepository
    {
        private readonly string connectionString;

        public DailyLogRepository(IDbConfig dbConfig)
        {
            this.connectionString = dbConfig.ConnectionString;
        }

        public async Task Add(DailyLog log)
        {
            const string query = @"
                INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(query, conn);

            cmd.Parameters.AddWithValue("@userId", log.UserId);
            cmd.Parameters.AddWithValue("@mealId", log.MealId);
            cmd.Parameters.AddWithValue("@calories", log.Calories);
            cmd.Parameters.AddWithValue("@loggedAt", log.LoggedAt);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> HasAnyLogs(int userId)
        {
            const string query = "SELECT COUNT(1) FROM DailyLogs WHERE user_id = @userId";

            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(query, conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            var countResult = await cmd.ExecuteScalarAsync();
            int count = Convert.ToInt32(countResult);

            return count > 0;
        }

        public async Task<DailyLog> GetNutritionTotalsForRange(int userId, DateTime startInclusive, DateTime endExclusive)
        {
            const string query = @"
                SELECT
                    COALESCE(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS total_calories,
                    COALESCE(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS total_protein,
                    COALESCE(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS total_carbohydrates,
                    COALESCE(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS total_fats
                FROM DailyLogs dl
                INNER JOIN Meals m ON m.meal_id = dl.mealId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON i.food_id = mi.food_id
                WHERE dl.user_id = @userId
                  AND dl.created_at >= @startDate
                  AND dl.created_at < @endDate";

            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(query, conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@startDate", startInclusive);
            cmd.Parameters.AddWithValue("@endDate", endExclusive);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new DailyLog
                {
                    UserId = userId,
                    LoggedAt = startInclusive,
                    Calories = Convert.ToDouble(reader["total_calories"]),
                    Protein = Convert.ToDouble(reader["total_protein"]),
                    Carbohydrates = Convert.ToDouble(reader["total_carbohydrates"]),
                    Fats = Convert.ToDouble(reader["total_fats"]),
                };
            }

            return new DailyLog
            {
                UserId = userId,
                LoggedAt = startInclusive,
            };
        }
    }
}
