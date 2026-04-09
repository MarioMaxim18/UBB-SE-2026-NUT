using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class MealRepository : IRepository<Meal>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public List<Meal> GetMeals()
        {
            try
            {
                return GetAll().Result.ToList();
            }
            catch
            {
                return new List<Meal>();
            }
        }

        public async Task<IEnumerable<Meal>> GetFilteredMeals(MealFilter filter)
        {
            var userId = UserSession.UserId ?? 0;
            var mealsList = new List<Meal>();

            string baseQuery = @"
        SELECT 
            m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, 
            m.isNutFree, m.isVegan, m.isGlutenFree, m.description,
            MAX(CASE WHEN f.id IS NOT NULL THEN 1 ELSE 0 END) AS isFavorite,
            CAST(IFNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
            CAST(IFNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
            CAST(IFNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
            CAST(IFNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
        FROM Meals m
        LEFT JOIN Favorites f ON f.mealId = m.meal_id AND f.userId = @userId
        LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
        LEFT JOIN Ingredients i ON mi.food_id = i.food_id
        WHERE 1=1";

            StringBuilder query = new StringBuilder(baseQuery);
            var command = new SqliteCommand();

            // Apply dietary filters
            if (filter.IsKeto) query.Append(" AND m.isKeto = 1");
            if (filter.IsVegan) query.Append(" AND m.isVegan = 1");
            if (filter.IsNutFree) query.Append(" AND m.isNutFree = 1");
            if (filter.IsLactoseFree) query.Append(" AND m.isLactoseFree = 1");
            if (filter.IsGlutenFree) query.Append(" AND m.isGlutenFree = 1");

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query.Append(" AND m.name LIKE @searchTerm");
                command.Parameters.AddWithValue("@searchTerm", $"%{filter.SearchTerm}%");
            }

            // Group by all non-aggregated columns
            query.Append(@" GROUP BY 
        m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, 
        m.isNutFree, m.isVegan, m.isGlutenFree, m.description");

            // Execute query
            using var connection = new SqliteConnection(_connectionString);
            command.Connection = connection;
            command.CommandText = query.ToString();
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                mealsList.Add(MapReaderToMeal(reader));
            }
            return mealsList;
        }

        public async Task SetFavoriteAsync(int userId, int mealId, bool isFavorite)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            if (isFavorite)
            {
                // SQLite uses INSERT OR IGNORE instead of IF NOT EXISTS
                const string insertQuery = "INSERT OR IGNORE INTO Favorites (userId, mealId) VALUES (@userId, @mealId)";
                using var insertCommand = new SqliteCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@mealId", mealId);
                await insertCommand.ExecuteNonQueryAsync();
            }
            else
            {
                const string deleteQuery = "DELETE FROM Favorites WHERE userId = @userId AND mealId = @mealId";
                using var deleteCommand = new SqliteCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@userId", userId);
                deleteCommand.Parameters.AddWithValue("@mealId", mealId);
                await deleteCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<Meal>> GetAll()
        {
            return await GetFilteredMeals(new MealFilter());
        }

        public async Task<Meal> GetById(int id)
        {
            var result = await GetFilteredMeals(new MealFilter());
            return result.FirstOrDefault(m => m.Id == id);
        }

        public async Task Add(Meal entity)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"INSERT INTO Meals (name, imageUrl, isKeto, isVegan, isNutFree, isLactoseFree, isGlutenFree, description) 
                                VALUES (@name, @imageUrl, @isKeto, @isVegan, @isNutFree, @isLactoseFree, @isGlutenFree, @description)";
            using var command = new SqliteCommand(query, connection);
            AddMealParameters(command, entity);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task Update(Meal entity)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"UPDATE Meals SET name=@name, imageUrl=@imageUrl, isKeto=@isKeto, isVegan=@isVegan, 
                                 isNutFree=@isNutFree, isLactoseFree=@isLactoseFree, isGlutenFree=@isGlutenFree, description=@description 
                                 WHERE meal_id=@id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", entity.Id);
            AddMealParameters(command, entity);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "DELETE FROM Meals WHERE meal_id = @id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private void AddMealParameters(SqliteCommand command, Meal meal)
        {
            command.Parameters.AddWithValue("@name", meal.Name);
            command.Parameters.AddWithValue("@imageUrl", meal.ImageUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@isKeto", meal.IsKeto ? 1 : 0);
            command.Parameters.AddWithValue("@isVegan", meal.IsVegan ? 1 : 0);
            command.Parameters.AddWithValue("@isNutFree", meal.IsNutFree ? 1 : 0);
            command.Parameters.AddWithValue("@isLactoseFree", meal.IsLactoseFree ? 1 : 0);
            command.Parameters.AddWithValue("@isGlutenFree", meal.IsGlutenFree ? 1 : 0);
            command.Parameters.AddWithValue("@description", meal.Description ?? (object)DBNull.Value);
        }

        private Meal MapReaderToMeal(SqliteDataReader reader)
        {
            return new Meal
            {
                Id = (int)Convert.ToInt64(reader["meal_id"]),
                Name = reader["name"].ToString(),
                ImageUrl = reader["imageUrl"]?.ToString(),

                Calories = (int)Math.Round(Convert.ToDouble(reader["calories"])),
                Protein = (int)Math.Round(Convert.ToDouble(reader["protein"])),
                Carbohydrates = (int)Math.Round(Convert.ToDouble(reader["carbs"])),
                Fat = (int)Math.Round(Convert.ToDouble(reader["fat"])),

                IsKeto = Convert.ToInt64(reader["isKeto"]) == 1,
                IsVegan = Convert.ToInt64(reader["isVegan"]) == 1,
                IsNutFree = Convert.ToInt64(reader["isNutFree"]) == 1,
                IsLactoseFree = Convert.ToInt64(reader["isLactoseFree"]) == 1,
                IsGlutenFree = Convert.ToInt64(reader["isGlutenFree"]) == 1,
                IsFavorite = Convert.ToInt64(reader["isFavorite"]) == 1,

                Description = reader["description"]?.ToString()
            };
        }

        public async Task<List<string>> GetIngredientLinesForMealAsync(int mealId)
        {
            var ingredientsList = new List<string>();
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"
                SELECT i.name, mi.quantity
                FROM MealsIngredients mi
                INNER JOIN Ingredients i ON i.food_id = mi.food_id
                WHERE mi.meal_id = @mealId
                ORDER BY mi.quantity DESC, i.name";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@mealId", mealId);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var ingredientName = reader["name"]?.ToString() ?? "Unknown ingredient";
                var quantityGrams = Convert.ToDouble(reader["quantity"]);
                ingredientsList.Add($"- {ingredientName} ({quantityGrams:0.#}g)");
            }
            return ingredientsList;
        }
    }
}

/*using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Models;
using TeamNut.Models;
using TeamNut.Repositories;
namespace TeamNut.Repositories
{
    
    internal class MealRepository : IRepository<Meal>
    {
        public List<Meal> GetMeals()
        {
            try
            {
                return GetAll().Result.ToList();
            }
            catch
            {
                return new List<Meal>();
            }
        }
        public async Task<List<string>> GetIngredientLinesForMealAsync(int mealId)
        {
            var ingredients = new List<string>();
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT i.name, mi.quantity
                FROM MealsIngredients mi
                INNER JOIN Ingredients i ON i.food_id = mi.food_id
                WHERE mi.meal_id = @mealId
                ORDER BY mi.quantity DESC, i.name";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mealId", mealId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader["name"]?.ToString() ?? "Unknown ingredient";
                var quantity = Convert.ToDouble(reader["quantity"]);
                ingredients.Add($"- {name} ({quantity:0.#}g)");
            }
            return ingredients;
        }
        private readonly string _connectionString = DbConfig.ConnectionString;
        public async Task<IEnumerable<Meal>> GetFilteredMeals(MealFilter filter)
        {
            var userId = UserSession.UserId ?? 0;
            var meals = new List<Meal>();
            StringBuilder sql = new StringBuilder(@"
                SELECT 
                    m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, 
                    m.isNutFree, m.isVegan, m.isGlutenFree, m.description,
                    MAX(CASE WHEN f.id IS NULL THEN 0 ELSE 1 END) AS isFavorite,
                    CAST(IFNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(IFNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(IFNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(IFNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN Favorites f ON f.mealId = m.meal_id AND f.userId = @userId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE 1=1");
            var parameters = new List<SqliteParameter>();
            
            if (filter.IsKeto) sql.Append(" AND m.isKeto = 1");
            if (filter.IsVegan) sql.Append(" AND m.isVegan = 1");
            if (filter.IsNutFree) sql.Append(" AND m.isNutFree = 1");
            if (filter.IsLactoseFree) sql.Append(" AND m.isLactoseFree = 1");
            if (filter.IsGlutenFree) sql.Append(" AND m.isGlutenFree = 1");
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                sql.Append(" AND m.name LIKE @search");
                parameters.Add(new SqliteParameter("@search", $"%{filter.SearchTerm}%"));
            }
            sql.Append(@" GROUP BY 
                m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description");
            using var conn = new SqliteConnection(_connectionString);
            using var cmd = new SqliteCommand(sql.ToString(), conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddRange(parameters.ToArray());
            //addition
            if (parameters.Count > 0)
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                meals.Add(MapReaderToMeal(reader));
            }
            return meals;
        }
        public async Task<Meal> GetById(int id)
        {
            var userId = UserSession.UserId ?? 0;
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT 
                    m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, 
                    m.isNutFree, m.isVegan, m.isGlutenFree, m.description,
                    MAX(CASE WHEN f.id IS NULL THEN 0 ELSE 1 END) AS isFavorite,
                    CAST(IFNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(IFNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(IFNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(IFNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN Favorites f ON f.mealId = m.meal_id AND f.userId = @userId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE m.meal_id = @id
                GROUP BY m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMeal(reader);
            return null;
        }
        public async Task<IEnumerable<Meal>> GetAll()
        {
            var userId = UserSession.UserId ?? 0;
            var meals = new List<Meal>();
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT 
                    m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, 
                    m.isNutFree, m.isVegan, m.isGlutenFree, m.description,
                    MAX(CASE WHEN f.id IS NULL THEN 0 ELSE 1 END) AS isFavorite,
                    CAST(IFNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(IFNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(IFNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(IFNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN Favorites f ON f.mealId = m.meal_id AND f.userId = @userId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                GROUP BY m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) meals.Add(MapReaderToMeal(reader));
            return meals;
        }
        public async Task Add(Meal entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"INSERT INTO Meals (name, imageUrl, isKeto, isVegan, isNutFree, isLactoseFree, isGlutenFree, description) 
                                VALUES (@name, @img, @keto, @vegan, @nut, @lac, @glu, @desc)";
            using var cmd = new SqliteCommand(sql, conn);
            AddMealParameters(cmd, entity);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task SetFavoriteAsync(int userId, int mealId, bool isFavorite)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            if (isFavorite)
            {
                const string insertSql = @"IF NOT EXISTS (SELECT 1 FROM Favorites WHERE userId = @userId AND mealId = @mealId)
                                           INSERT INTO Favorites (userId, mealId) VALUES (@userId, @mealId)";
                using var insertCmd = new SqliteCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@userId", userId);
                insertCmd.Parameters.AddWithValue("@mealId", mealId);
                await insertCmd.ExecuteNonQueryAsync();
            }
            else
            {
                const string deleteSql = "DELETE FROM Favorites WHERE userId = @userId AND mealId = @mealId";
                using var deleteCmd = new SqliteCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@userId", userId);
                deleteCmd.Parameters.AddWithValue("@mealId", mealId);
                await deleteCmd.ExecuteNonQueryAsync();
            }
        }
        public async Task Update(Meal entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"UPDATE Meals SET name=@name, imageUrl=@img, isKeto=@keto, isVegan=@vegan, 
                                 isNutFree=@nut, isLactoseFree=@lac, isGlutenFree=@glu, description=@desc 
                                 WHERE meal_id=@id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            AddMealParameters(cmd, entity);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "DELETE FROM Meals WHERE meal_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        private void AddMealParameters(SqliteCommand cmd, Meal meal)
        {
            cmd.Parameters.AddWithValue("@name", meal.Name);
            cmd.Parameters.AddWithValue("@img", meal.ImageUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@keto", meal.IsKeto);
            cmd.Parameters.AddWithValue("@vegan", meal.IsVegan);
            cmd.Parameters.AddWithValue("@nut", meal.IsNutFree);
            cmd.Parameters.AddWithValue("@lac", meal.IsLactoseFree);
            cmd.Parameters.AddWithValue("@glu", meal.IsGlutenFree);
            cmd.Parameters.AddWithValue("@desc", meal.Description ?? (object)DBNull.Value);
        }
        private Meal MapReaderToMeal(SqliteDataReader reader)
        {
            return new Meal
            {
                Id = Convert.ToInt64(reader["meal_id"]),
                Name = reader["name"].ToString(),
                ImageUrl = reader["imageUrl"]?.ToString(),
                Calories = Convert.ToInt64(reader["calories"]),
                Protein = Convert.ToInt64(reader["protein"]),
                Carbs = Convert.ToInt64(reader["carbs"]),
                Fat = Convert.ToInt64(reader["fat"]),
                IsKeto = Convert.ToBoolean(reader["isKeto"]),
                IsVegan = Convert.ToBoolean(reader["isVegan"]),
                IsNutFree = Convert.ToBoolean(reader["isNutFree"]),
                IsLactoseFree = Convert.ToBoolean(reader["isLactoseFree"]),
                IsGlutenFree = Convert.ToBoolean(reader["isGlutenFree"]),
                IsFavorite = Convert.ToBoolean(reader["isFavorite"]),
                Description = reader["description"]?.ToString()
            };
            return ingredientsList;
        }
    }
}
*/