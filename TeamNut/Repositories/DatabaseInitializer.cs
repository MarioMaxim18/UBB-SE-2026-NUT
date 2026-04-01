using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task EnsureDailyMealLogsTableExists()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if table exists
            var checkQuery = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DailyMealLogs')
                BEGIN
                    CREATE TABLE DailyMealLogs (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        user_id INT NOT NULL,
                        meal_id INT NOT NULL,
                        meal_name NVARCHAR(255) NOT NULL,
                        log_date DATETIME NOT NULL,
                        calories INT NOT NULL,
                        protein INT NOT NULL,
                        carbs INT NOT NULL,
                        fat INT NOT NULL,
                        meal_type NVARCHAR(50) NOT NULL,
                        CONSTRAINT FK_DailyMealLogs_Users FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
                        CONSTRAINT FK_DailyMealLogs_Meals FOREIGN KEY (meal_id) REFERENCES Meals(meal_id)
                    );

                    CREATE INDEX IX_DailyMealLogs_UserDate ON DailyMealLogs(user_id, log_date);
                END";

            using var cmd = new SqlCommand(checkQuery, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> TableExists(string tableName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tableName", tableName);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }
}
