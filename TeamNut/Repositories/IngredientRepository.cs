using System;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class IngredientRepository
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<int> GetOrCreateIngredientIdByNameAsync(string name)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string findSql = "SELECT food_id FROM Ingredients WHERE LOWER(name) = LOWER(@name)";
            using (var findCmd = new SqlCommand(findSql, conn))
            {
                findCmd.Parameters.AddWithValue("@name", name);
                var existing = await findCmd.ExecuteScalarAsync();
                if (existing != null && existing != DBNull.Value)
                {
                    return Convert.ToInt32(existing);
            }
            }

            const string insertSql = @"INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                                       OUTPUT INSERTED.food_id
                                       VALUES (@name, 0, 0, 0, 0)";
            using var insertCmd = new SqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            var id = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            var ingredients = new List<Ingredient>();

            const string sql = @"SELECT food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g
                                 FROM Ingredients
                                 ORDER BY name";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            using var reader = await selectCmd.ExecuteReaderAsync();
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

        private static double GetDoubleOrZero(SqlDataReader reader, string column)
        {
            var value = reader[column];
            return value == DBNull.Value ? 0 : Convert.ToDouble(value);
        }
    }
}
