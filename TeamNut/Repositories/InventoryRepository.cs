using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class InventoryRepository : IRepository<Inventory>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        
        public async Task<IEnumerable<Inventory>> GetAllByUserId(int userId)
        {
            var items = new List<Inventory>();
            // join with Ingredients table to get the Name for the UI
            const string sql = @"SELECT inv.*, ing.name 
                                 FROM Inventory inv 
                                 JOIN Ingredients ing ON inv.ingredient_id = ing.food_id 
                                 WHERE inv.user_id = @uid";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = MapReaderToInventory(reader);
                item.IngredientName = reader["name"].ToString();
                items.Add(item);
            }
            return items;
        }

        public async Task Add(Inventory entity)
        {
            using var conn = new SqlConnection(_connectionString);
            
            const string sql = @"
                IF EXISTS (SELECT 1 FROM Inventory WHERE user_id = @uid AND ingredient_id = @iid)
                BEGIN
                    UPDATE Inventory SET quantity_grams = quantity_grams + @qty 
                    WHERE user_id = @uid AND ingredient_id = @iid
                END
                ELSE
                BEGIN
                    INSERT INTO Inventory (user_id, ingredient_id, quantity_grams) 
                    VALUES (@uid, @iid, @qty)
                END";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@iid", entity.IngredientId);
            cmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        
        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Inventory WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        
        public async Task<Inventory> GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Inventory WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToInventory(reader) : null;
        }

        public async Task<IEnumerable<Inventory>> GetAll() => throw new NotImplementedException("Use GetAllByUserId instead");

        public async Task Update(Inventory entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "UPDATE Inventory SET quantity_grams = @qty WHERE id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private Inventory MapReaderToInventory(SqlDataReader reader)
        {
            return new Inventory
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                QuantityGrams = Convert.ToInt32(reader["quantity_grams"])
            };
        }
    }
}