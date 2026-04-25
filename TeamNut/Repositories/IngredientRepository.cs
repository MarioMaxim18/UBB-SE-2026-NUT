namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    internal class IngredientRepository : IIngredientRepository
    {
        private readonly string connectionString;

        public IngredientRepository(IDbConfig dbConfig)
        {
            this.connectionString = dbConfig.ConnectionString;
        }

        public async Task<int> GetOrCreateIngredientIdAsync(string name)
        {
            return await GetOrCreateIngredientIdByNameAsync(name);
        }

        public async Task<int> GetOrCreateIngredientIdByNameAsync(string name)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            const string findSql = "SELECT food_id FROM Ingredients WHERE LOWER(name) = LOWER(@name)";

            using (var findCmd = new SqliteCommand(findSql, conn))
            {
                findCmd.Parameters.AddWithValue("@name", name);
                var existingResult = await findCmd.ExecuteScalarAsync();

                if (existingResult != null && existingResult != DBNull.Value)
                {
                    return Convert.ToInt32(existingResult);
                }
            }

            const string insertSql = @"
                INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                VALUES (@name, 0, 0, 0, 0);
                SELECT last_insert_rowid();";

            using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", name);

            var newIdResult = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(newIdResult);
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            const string sql = @"
                SELECT food_id, name
                FROM Ingredients
                WHERE name LIKE @search
                ORDER BY name
                LIMIT 20";

            var results = new List<KeyValuePair<int, string>>();
            using var conn = new SqliteConnection(connectionString);
            using var command = new SqliteCommand(sql, conn);

            command.Parameters.AddWithValue("@search", $"%{search}%");

            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var id = Convert.ToInt32(reader["food_id"]);
                var ingredientName = reader["name"]?.ToString() ?? string.Empty;
                results.Add(new KeyValuePair<int, string>(id, ingredientName));
            }

            return results;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            const string sql = @"
                SELECT food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g
                FROM Ingredients
                ORDER BY name";

            var ingredients = new List<Ingredient>();
            using var conn = new SqliteConnection(connectionString);
            using var command = new SqliteCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ingredients.Add(new Ingredient
                {
                    FoodId = Convert.ToInt32(reader["food_id"]),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    CaloriesPer100g = GetDoubleOrZero(reader, "calories_per_100g"),
                    ProteinPer100g = GetDoubleOrZero(reader, "protein_per_100g"),
                    CarbohydratesPer100g = GetDoubleOrZero(reader, "carbs_per_100g"),
                    FatPer100g = GetDoubleOrZero(reader, "fat_per_100g"),
                });
            }

            return ingredients;
        }

        private static double GetDoubleOrZero(SqliteDataReader reader, string column)
        {
            var value = reader[column];

            if (value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDouble(value);
        }
    }
}