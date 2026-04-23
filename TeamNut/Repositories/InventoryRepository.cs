// <copyright file="InventoryRepository.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    internal class InventoryRepository : IInventoryRepository
    {
        private readonly string connectionString;

        public InventoryRepository(IDbConfig dbConfig)
        {
            connectionString = dbConfig.ConnectionString;
        }

        public async Task Add(Inventory entity)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            const string checkSql = @"
        SELECT id, quantity_grams
        FROM Inventory
        WHERE user_id = @uid AND ingredient_id = @iid LIMIT 1";

            using var checkCmd = new SqliteCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@uid", entity.UserId);
            checkCmd.Parameters.AddWithValue("@iid", entity.IngredientId);

            using var reader = await checkCmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int existingId = Convert.ToInt32(reader["id"]);
                int existingQty = Convert.ToInt32(reader["quantity_grams"]);
                int newQty = existingQty + entity.QuantityGrams;

                const string updateSql = "UPDATE Inventory SET quantity_grams = @qty WHERE id = @id";
                using var updateCmd = new SqliteCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@qty", newQty);
                updateCmd.Parameters.AddWithValue("@id", existingId);

                await reader.CloseAsync();
                await updateCmd.ExecuteNonQueryAsync();
            }
            else
            {
                const string insertSql = @"
            INSERT INTO Inventory (user_id, ingredient_id, quantity_grams)
            VALUES (@uid, @iid, @qty)";

                using var insertCmd = new SqliteCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@uid", entity.UserId);
                insertCmd.Parameters.AddWithValue("@iid", entity.IngredientId);
                insertCmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);

                await reader.CloseAsync();
                await insertCmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Inventory>> GetAllByUserId(int userId)
        {
            var items = new List<Inventory>();
            const string sql = @"SELECT inv.*, ing.name
                                 FROM Inventory inv
                                 JOIN Ingredients ing ON inv.ingredient_id = ing.food_id
                                 WHERE inv.user_id = @uid";

            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = MapReaderToInventory(reader);
                item.IngredientName = reader["name"]?.ToString() ?? string.Empty;
                items.Add(item);
            }
            return items;
        }

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = "DELETE FROM Inventory WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Inventory?> GetById(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM Inventory WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReaderToInventory(reader) : null;
        }

        public async Task Update(Inventory entity)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = "UPDATE Inventory SET quantity_grams = @qty WHERE id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@qty", entity.QuantityGrams);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public Task<IEnumerable<Inventory>> GetAll()
        {
            throw new NotImplementedException("Use GetAllByUserId");
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
}
