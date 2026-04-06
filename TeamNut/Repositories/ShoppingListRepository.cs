using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class ShoppingListRepository : IRepository<ShoppingItem>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task Add(ShoppingItem item)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO ShoppingItems (user_id, ingredient_id, quantity_grams, is_checked)
                             VALUES (@userId, @ingredientId, @quantityGrams, @isChecked)";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", item.UserId);
            cmd.Parameters.AddWithValue("@ingredientId", item.IngredientId);
            cmd.Parameters.AddWithValue("@quantityGrams", item.QuantityGrams);
            cmd.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ShoppingItem>> GetAll()
        {
            var items = new List<ShoppingItem>();

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                             FROM ShoppingItems s 
                             JOIN Ingredients i ON s.ingredient_id = i.food_id";

            using var cmd = new SqliteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(MapReaderToItem(reader));
            }

            return items;
        }

        public async Task<ShoppingItem> GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                             FROM ShoppingItems s 
                             JOIN Ingredients i ON s.ingredient_id = i.food_id
                             WHERE s.id = @id";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToItem(reader);
            }

            return null;
        }

        public async Task<ShoppingItem> GetByUserAndIngredient(int userId, int ingredientId)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                             FROM ShoppingItems s 
                             JOIN Ingredients i ON s.ingredient_id = i.food_id
                             WHERE s.user_id = @userId AND s.ingredient_id = @ingredientId";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@ingredientId", ingredientId);

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

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                             FROM ShoppingItems s 
                             JOIN Ingredients i ON s.ingredient_id = i.food_id
                             WHERE s.user_id = @userId";

            using var cmd = new SqliteCommand(query, conn);
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
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE ShoppingItems 
                             SET ingredient_id = @ingredientId, quantity_grams = @quantityGrams, is_checked = @isChecked
                             WHERE id = @id";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.Parameters.AddWithValue("@ingredientId", item.IngredientId);
            cmd.Parameters.AddWithValue("@quantityGrams", item.QuantityGrams);
            cmd.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "DELETE FROM ShoppingItems WHERE id = @id";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<List<ShoppingItem>> GetIngredientsNeededFromMealPlan(int userId)
        {
            var items = new List<ShoppingItem>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

           
            string query = @"
        SELECT 
            mi.food_id as ingredient_id, 
            i.name as ingredient_name, 
            (SUM(mi.quantity) - IFNULL(MAX(inv.quantity_grams), 0)) as quantity_needed
        FROM MealPlan mp
        JOIN MealPlanMeal mpm ON mp.mealplan_id = mpm.mealPlanId
        JOIN MealsIngredients mi ON mpm.mealId = mi.meal_id
        JOIN Ingredients i ON mi.food_id = i.food_id
        LEFT JOIN Inventory inv ON i.food_id = inv.ingredient_id AND inv.user_id = @userId
        WHERE mp.mealplan_id = (
            SELECT mealplan_id
            FROM MealPlan
            WHERE user_id = @userId
              AND DATE(created_at) = DATE('now', 'localtime')
            ORDER BY created_at DESC
            LIMIT 1
        )
        GROUP BY mi.food_id, i.name
        HAVING quantity_needed > 0";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                    IngredientName = reader["ingredient_name"].ToString(),
                  
                    QuantityGrams = Convert.ToDouble(reader["quantity_needed"]),
                    IsChecked = false
                });
            }

            
            
            return items;
        }

        /*
        public async Task<List<ShoppingItem>> GetIngredientsNeededFromMealPlan(int userId)
        {
            var items = new List<ShoppingItem>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT mi.food_id as ingredient_id, i.name as ingredient_name, SUM(mi.quantity) as quantity_grams
                FROM MealPlan mp
                JOIN MealPlanMeal mpm ON mp.mealplan_id = mpm.mealPlanId
                JOIN MealsIngredients mi ON mpm.mealId = mi.meal_id
                JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE mp.user_id = @userId 
                  AND mp.created_at >= CAST(GETDATE() AS DATE)
                GROUP BY mi.food_id, i.name";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                    IngredientName = reader["ingredient_name"].ToString(),
                    QuantityGrams = Convert.ToDouble(reader["quantity_grams"])
                });
            }
            return items;
        }*/

        private ShoppingItem MapReaderToItem(SqliteDataReader reader)
        {
            return new ShoppingItem
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                QuantityGrams = Convert.ToDouble(reader["quantity_grams"]),
                IngredientName = reader["ingredient_name"].ToString(),
                IsChecked = Convert.ToBoolean(reader["is_checked"])
            };
        }
    }
}
