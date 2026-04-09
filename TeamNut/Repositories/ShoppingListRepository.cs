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

        public async Task Add(ShoppingItem shoppingItem)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"INSERT INTO ShoppingItems (user_id, ingredient_id, quantity_grams, is_checked)
                                  VALUES (@userId, @ingredientId, @quantityGrams, @isChecked)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", shoppingItem.UserId);
            command.Parameters.AddWithValue("@ingredientId", shoppingItem.IngredientId);
            command.Parameters.AddWithValue("@quantityGrams", shoppingItem.QuantityGrams);
            command.Parameters.AddWithValue("@isChecked", shoppingItem.IsChecked ? 1 : 0);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ShoppingItem>> GetAll()
        {
            var shoppingItemsList = new List<ShoppingItem>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                                  FROM ShoppingItems s 
                                  JOIN Ingredients i ON s.ingredient_id = i.food_id";

            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                shoppingItemsList.Add(MapReaderToShoppingItem(reader));
            }

            return shoppingItemsList;
        }

        public async Task<ShoppingItem> GetById(int shoppingItemId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                                  FROM ShoppingItems s 
                                  JOIN Ingredients i ON s.ingredient_id = i.food_id
                                  WHERE s.id = @shoppingItemId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@shoppingItemId", shoppingItemId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToShoppingItem(reader);
            }

            return null;
        }

        public async Task<ShoppingItem> GetByUserAndIngredient(int userId, int ingredientId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                                  FROM ShoppingItems s 
                                  JOIN Ingredients i ON s.ingredient_id = i.food_id
                                  WHERE s.user_id = @userId AND s.ingredient_id = @ingredientId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@ingredientId", ingredientId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToShoppingItem(reader);
            }

            return null;
        }

        public async Task<List<ShoppingItem>> GetAllByUserId(int userId)
        {
            var shoppingItemsList = new List<ShoppingItem>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name 
                                  FROM ShoppingItems s 
                                  JOIN Ingredients i ON s.ingredient_id = i.food_id
                                  WHERE s.user_id = @userId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                shoppingItemsList.Add(MapReaderToShoppingItem(reader));
            }

            return shoppingItemsList;
        }

        public async Task Update(ShoppingItem shoppingItem)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"UPDATE ShoppingItems 
                                  SET ingredient_id = @ingredientId, quantity_grams = @quantityGrams, is_checked = @isChecked
                                  WHERE shoppingItemId = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@shoppingItemId", shoppingItem.Id);
            command.Parameters.AddWithValue("@ingredientId", shoppingItem.IngredientId);
            command.Parameters.AddWithValue("@quantityGrams", shoppingItem.QuantityGrams);
            command.Parameters.AddWithValue("@isChecked", shoppingItem.IsChecked ? 1 : 0);

            await command.ExecuteNonQueryAsync();
        }

        public async Task Delete(int shoppingItemId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "DELETE FROM ShoppingItems WHERE id = @shoppingItemId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@shoppingItemId", shoppingItemId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<ShoppingItem>> GetIngredientsNeededFromMealPlan(int userId)
        {
            var neededItemsList = new List<ShoppingItem>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
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

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                neededItemsList.Add(new ShoppingItem
                {
                    UserId = userId,
                    IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                    IngredientName = reader["ingredient_name"].ToString(),
                    QuantityGrams = Convert.ToDouble(reader["quantity_needed"]),
                    IsChecked = false
                });
            }

            return neededItemsList;
        }

        private ShoppingItem MapReaderToShoppingItem(SqliteDataReader reader)
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