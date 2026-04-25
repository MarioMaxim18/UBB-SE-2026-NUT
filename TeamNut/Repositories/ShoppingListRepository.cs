namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    public class ShoppingListRepository : IShoppingListRepository
    {
        private readonly string connectionString;

        public ShoppingListRepository(IDbConfig dbConfig)
        {
            connectionString = dbConfig.ConnectionString;
        }

        public async Task Add(ShoppingItem item)
        {
            const string query = @"
                INSERT INTO ShoppingItems (user_id, ingredient_id, quantity_grams, is_checked)
                VALUES (@userId, @ingredientId, @quantityGrams, @isChecked);
                SELECT last_insert_rowid();";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@userId", item.UserId);
                command.Parameters.AddWithValue("@ingredientId", item.IngredientId);
                command.Parameters.AddWithValue("@quantityGrams", item.QuantityGrams);
                command.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    item.Id = Convert.ToInt32(result);
                }
            }
        }

        public async Task<IEnumerable<ShoppingItem>> GetAll()
        {
            const string query = @"
                SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name
                FROM ShoppingItems s
                JOIN Ingredients i ON s.ingredient_id = i.food_id";

            var items = new List<ShoppingItem>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(MapReaderToItem(reader));
                    }
                }
            }

            return items;
        }

        public async Task<ShoppingItem?> GetById(int id)
        {
            const string query = @"
                SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name
                FROM ShoppingItems s
                JOIN Ingredients i ON s.ingredient_id = i.food_id
                WHERE s.id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToItem(reader);
                    }
                }
            }

            return null;
        }

        public async Task<ShoppingItem?> GetByUserAndIngredient(int userId, int ingredientId)
        {
            const string query = @"
                SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name
                FROM ShoppingItems s
                JOIN Ingredients i ON s.ingredient_id = i.food_id
                WHERE s.user_id = @userId AND s.ingredient_id = @ingredientId";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@ingredientId", ingredientId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToItem(reader);
                    }
                }
            }

            return null;
        }

        public async Task<List<ShoppingItem>> GetAllByUserId(int userId)
        {
            const string query = @"
                SELECT s.id, s.user_id, s.ingredient_id, s.quantity_grams, s.is_checked, i.name AS ingredient_name
                FROM ShoppingItems s
                JOIN Ingredients i ON s.ingredient_id = i.food_id
                WHERE s.user_id = @userId";

            var items = new List<ShoppingItem>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(MapReaderToItem(reader));
                    }
                }
            }

            return items;
        }

        public async Task Update(ShoppingItem item)
        {
            const string query = @"
                UPDATE ShoppingItems
                SET ingredient_id = @ingredientId, quantity_grams = @quantityGrams, is_checked = @isChecked
                WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@id", item.Id);
                command.Parameters.AddWithValue("@ingredientId", item.IngredientId);
                command.Parameters.AddWithValue("@quantityGrams", item.QuantityGrams);
                command.Parameters.AddWithValue("@isChecked", item.IsChecked ? 1 : 0);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            const string query = "DELETE FROM ShoppingItems WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<ShoppingItem>> GetIngredientsNeededFromMealPlan(int userId)
        {
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

            var items = new List<ShoppingItem>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var command = new SqliteCommand(query, conn))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new ShoppingItem
                        {
                            UserId = userId,
                            IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                            IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                            QuantityGrams = Convert.ToDouble(reader["quantity_needed"]),
                            IsChecked = false
                        });
                    }
                }
            }

            return items;
        }

        private ShoppingItem MapReaderToItem(SqliteDataReader reader)
        {
            return new ShoppingItem
            {
                Id = Convert.ToInt32(reader["id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                QuantityGrams = Convert.ToDouble(reader["quantity_grams"]),
                IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                IsChecked = Convert.ToBoolean(reader["is_checked"])
            };
        }
    }
}
