using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Repositories.Interfaces;

namespace TeamNut.Repositories
{
    internal class IngredientRepository : IIngredientRepository
    {
        private readonly string _connectionString;

        public IngredientRepository(IDbConfig dbConfig)
        {
            _connectionString = dbConfig.ConnectionString;
        }

        public async Task<int> GetOrCreateIngredientIdAsync(string name)
        {
            return await GetOrCreateIngredientIdByNameAsync(name);
        }

        public async Task<int> GetOrCreateIngredientIdByNameAsync(string name)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string findSql = "SELECT food_id FROM Ingredients WHERE LOWER(name) = LOWER(@name)";
            using (var findCmd = new SqliteCommand(findSql, conn))
            {
                findCmd.Parameters.AddWithValue("@name", name);
                var existing = await findCmd.ExecuteScalarAsync();
                if (existing != null && existing != DBNull.Value)
                {
                    return Convert.ToInt32(existing);
                }
            }

            /*const string insertSql = @"INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                                       OUTPUT INSERTED.food_id
                                       VALUES (@name, 0, 0, 0, 0)";
            using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            var id = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);*/
            const string insertSql = @"INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                           VALUES (@name, 0, 0, 0, 0);
                           SELECT last_insert_rowid();";

            using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            var id = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            var results = new List<KeyValuePair<int, string>>();

            const string sql = @"SELECT  food_id, name
                                 FROM Ingredients
                                 WHERE name LIKE @search
                                 ORDER BY name
                                 LIMIT 20";

            using var conn = new SqliteConnection(_connectionString);
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@search", $"%{search}%");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(reader["food_id"]),
                    reader["name"]?.ToString() ?? string.Empty));
            }

            return results;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            var ingredients = new List<Ingredient>();

            const string sql = @"SELECT food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g
                                 FROM Ingredients
                                 ORDER BY name";

            using var conn = new SqliteConnection(_connectionString);
            using var cmd = new SqliteCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ingredients.Add(new Ingredient
                {
                    FoodId = Convert.ToInt32(reader["food_id"]),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    CaloriesPer100g = GetDoubleOrZero(reader, "calories_per_100g"),
                    ProteinPer100g = GetDoubleOrZero(reader, "protein_per_100g"),
                    CarbsPer100g = GetDoubleOrZero(reader, "carbs_per_100g"),
                    FatPer100g = GetDoubleOrZero(reader, "fat_per_100g")
                });
            }

            return ingredients;
        }

        private static double GetDoubleOrZero(SqliteDataReader reader, string column)
        {
            var value = reader[column];
            return value == DBNull.Value ? 0 : Convert.ToDouble(value);
        }
    }
}
