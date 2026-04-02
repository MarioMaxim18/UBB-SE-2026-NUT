using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    public class IngredientRepository
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<int> GetOrCreateIngredientIdAsync(string name)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string selectQuery = "SELECT food_id FROM Ingredients WHERE name = @name";
            using var selectCmd = new SqlCommand(selectQuery, conn);
            selectCmd.Parameters.AddWithValue("@name", name);

            var result = await selectCmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            string insertQuery = @"INSERT INTO Ingredients (name) VALUES (@name);
                                   SELECT SCOPE_IDENTITY();";
            using var insertCmd = new SqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            
            var newId = await insertCmd.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }
        public async Task<System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>> SearchIngredientsAsync(string query)
        {
            var results = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string selectQuery = "SELECT TOP 10 food_id, name FROM Ingredients WHERE name LIKE '%' + @query + '%'";
            using var selectCmd = new SqlCommand(selectQuery, conn);
            selectCmd.Parameters.AddWithValue("@query", query);

            using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new System.Collections.Generic.KeyValuePair<int, string>(
                    Convert.ToInt32(reader["food_id"]),
                    reader["name"].ToString()
                ));
            }
            return results;
        }
    }
}
