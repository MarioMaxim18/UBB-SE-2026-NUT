using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class ShoppingListRepository : IRepository<ShoppingItem>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task Add(ShoppingItem item)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO ShoppingItems (user_id, name, is_checked)
                             VALUES (@userId, @name, @isChecked)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", item.UserId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ShoppingItem>> GetAll()
        {
            var items = new List<ShoppingItem>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM ShoppingItems";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(MapReaderToItem(reader));
            }

            return items;
        }

        public async Task<ShoppingItem> GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM ShoppingItems WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToItem(reader);
            }

            return null;
        }

        public async Task<List<ShoppingItem>> GetAllByUserId(int userId)
        {
            var items = new List<ShoppingItem>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM ShoppingItems WHERE user_id = @userId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(MapReaderToItem(reader));
            }

            return items;
        }

        public async Task Update(ShoppingItem item)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE ShoppingItems 
                             SET name = @name, is_checked = @isChecked
                             WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "DELETE FROM ShoppingItems WHERE id = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        private ShoppingItem MapReaderToItem(SqlDataReader reader)
        {
            return new ShoppingItem
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Name = reader["name"].ToString(),
                IsChecked = Convert.ToBoolean(reader["is_checked"])
            };
        }
    }
}
