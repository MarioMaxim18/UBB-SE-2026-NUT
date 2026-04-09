using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class InventoryRepository : IRepository<Inventory>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task Add(Inventory inventory)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            
            const string query = @"
        SELECT id, quantity_grams 
        FROM Inventory 
        WHERE user_id = @userid AND ingredient_id = @ingredientid LIMIT 1";

            using var checkCommand = new SqliteCommand(query, connection);
            checkCommand.Parameters.AddWithValue("@userid", inventory.UserId);
            checkCommand.Parameters.AddWithValue("@ingredientid", inventory.IngredientId);

            using var reader = await checkCommand.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                
                int existingId = Convert.ToInt32(reader["id"]);
                int existingQuantity = Convert.ToInt32(reader["quantity_grams"]);
                int newQuantity = existingQuantity + inventory.QuantityGrams;

                const string updateQuery = "UPDATE Inventory SET quantity_grams = @quantity WHERE id = @id";
                using var updateCmd = new SqliteCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@quantity", newQuantity);
                updateCmd.Parameters.AddWithValue("@id", existingId);

                await reader.CloseAsync(); 
                await updateCmd.ExecuteNonQueryAsync();
            }
            else
            {
                
                const string insertQuery = @"
            INSERT INTO Inventory (user_id, ingredient_id, quantity_grams) 
            VALUES (@uid, @iid, @qty)";

                using var insertCommand = new SqliteCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@uid", inventory.UserId);
                insertCommand.Parameters.AddWithValue("@iid", inventory.IngredientId);
                insertCommand.Parameters.AddWithValue("@qty", inventory.QuantityGrams);

                await reader.CloseAsync(); 
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Inventory>> GetAllByUserId(int userId)
        {
            var inventoryList = new List<Inventory>();
            const string query = @"SELECT inv.*, ing.name 
                                 FROM Inventory inv 
                                 JOIN Ingredients ing ON inv.ingredient_id = ing.food_id 
                                 WHERE inv.user_id = @uid";

            using var connection = new SqliteConnection(_connectionString);
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@uid", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var inventoryItem = MapReaderToInventory(reader);
                inventoryItem.IngredientName = reader["name"].ToString();
                inventoryList.Add(inventoryItem);
            }
            return inventoryList;
        }

        public async Task Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "DELETE FROM Inventory WHERE id = @id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<Inventory> GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT * FROM Inventory WHERE id = @id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToInventory(reader) : null;
        }

        public async Task Update(Inventory entity)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "UPDATE Inventory SET quantity_grams = @qty WHERE id = @id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", entity.Id);
            command.Parameters.AddWithValue("@qty", entity.QuantityGrams);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Inventory>> GetAll() => throw new NotImplementedException("Use GetAllByUserId");

        private Inventory MapReaderToInventory(SqliteDataReader reader)
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



/*using Microsoft.Data.Sqlite;
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
            const string sql = @"SELECT inv.*, ing.name 
                                 FROM Inventory inv 
                                 JOIN Ingredients ing ON inv.ingredient_id = ing.food_id 
                                 WHERE inv.user_id = @uid";

            using var conn = new SqliteConnection(_connectionString);
            using var cmd = new SqliteCommand(sql, conn);
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
            using var conn = new SqliteConnection(_connectionString);
            
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

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@iid", entity.IngredientId);
            cmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        
        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "DELETE FROM Inventory WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        
        public async Task<Inventory> GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "SELECT * FROM Inventory WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToInventory(reader) : null;
        }

        public async Task<IEnumerable<Inventory>> GetAll() => throw new NotImplementedException("Use GetAllByUserId instead");

        public async Task Update(Inventory entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "UPDATE Inventory SET quantity_grams = @qty WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private Inventory MapReaderToInventory(SqliteDataReader reader)
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
}*/
