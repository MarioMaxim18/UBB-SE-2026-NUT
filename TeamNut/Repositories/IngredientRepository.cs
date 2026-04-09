using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class IngredientRepository
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<int> GetOrCreateIngredientIdAsync(string name)
        {
            return await GetOrCreateIngredientIdByNameAsync(name);
        }

        public async Task<int> GetOrCreateIngredientIdByNameAsync(string name)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "SELECT food_id FROM Ingredients WHERE LOWER(name) = LOWER(@name)";
            using (var findCommand = new SqliteCommand(query, connection))
            {
                findCommand.Parameters.AddWithValue("@name", name);
                var existingFood = await findCommand.ExecuteScalarAsync();
                if (existingFood != null && existingFood != DBNull.Value)
                {
                    return Convert.ToInt32(existingFood);
                }
            }

            /*const string insertSql = @"INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                                       OUTPUT INSERTED.food_id
                                       VALUES (@name, 0, 0, 0, 0)";
            using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            var id = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);*/
            const string insertQuery = @"INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                           VALUES (@name, 0, 0, 0, 0);
                           SELECT last_insert_rowid();"; 

            using var insertCommand = new SqliteCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@name", name);
            var insertedId = await insertCommand.ExecuteScalarAsync();
            return Convert.ToInt32(insertedId);
        }

        public async Task<List<KeyValuePair<int, string>>> SearchIngredientsAsync(string search)
        {
            var foodsList = new List<KeyValuePair<int, string>>();

            const string sql = @"SELECT  food_id, name
                                 FROM Ingredients
                                 WHERE name LIKE @search
                                 ORDER BY name
                                 LIMIT 20";

            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@search", $"%{search}%");

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                foodsList.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(reader["food_id"]),
                    reader["name"]?.ToString() ?? string.Empty));
            }

            return foodsList;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            var ingredientsList = new List<Ingredient>();

            const string query = @"SELECT food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g
                                 FROM Ingredients
                                 ORDER BY name";

            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ingredientsList.Add(new Ingredient
                {
                    FoodId = Convert.ToInt32(reader["food_id"]),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    CaloriesPer100Grams = GetDoubleOrZero(reader, "calories_per_100g"),
                    ProteinPer100Grams = GetDoubleOrZero(reader, "protein_per_100g"),
                    CarbohydratesPer100Grams = GetDoubleOrZero(reader, "carbs_per_100g"),
                    FatPer100Grams = GetDoubleOrZero(reader, "fat_per_100g")
                });
            }

            return ingredientsList;
        }

        private static double GetDoubleOrZero(SqliteDataReader reader, string column)
        {
            var cellValue = reader[column];
            return cellValue == DBNull.Value ? 0 : Convert.ToDouble(cellValue);
        }
    }
}
