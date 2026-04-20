using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Repositories; 
using TeamNut.Models;
using TeamNut.Views.MealPlanView;

namespace TeamNut.Repositories
{
    internal class MealPlanRepository : IRepository<MealPlan>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<MealPlan> GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "SELECT * FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMealPlan(reader);
            return null;
        }

        public async Task<MealPlan> GetLatestMealPlan(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"SELECT * FROM MealPlan
                                WHERE user_id = @userId
                                ORDER BY created_at DESC
                                LIMIT 1";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMealPlan(reader);
            return null;
        }

        public async Task<IEnumerable<MealPlan>> GetAll()
        {
            var plans = new List<MealPlan>();
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "SELECT * FROM MealPlan";
            using var cmd = new SqliteCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) plans.Add(MapReaderToMealPlan(reader));
            return plans;
        }

        public async Task Add(MealPlan entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"INSERT INTO MealPlan (user_id, created_at, goal_type) 
                                VALUES (@uid, @created, @goal)";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@created", entity.CreatedAt);
            cmd.Parameters.AddWithValue("@goal", entity.GoalType ?? (object)DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(MealPlan entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"UPDATE MealPlan SET goal_type = @goal 
                                 WHERE mealplan_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@goal", entity.GoalType);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = "DELETE FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MealPlan> GetTodaysMealPlan(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            const string sql = @"SELECT * FROM MealPlan
                                WHERE user_id = @userId
                                  AND DATE(created_at) = DATE('now', 'localtime')
                                ORDER BY created_at DESC
                                LIMIT 1";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMealPlan(reader);
            return null;
        }

        public async Task<int> GeneratePersonalizedDailyMealPlan(int userId)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
               

                const string checkMealsSql = "SELECT COUNT(*) FROM Meals";
                using var checkCmd = new SqliteCommand(checkMealsSql, conn, transaction);
                long mealCount = (long)await checkCmd.ExecuteScalarAsync();

                if (mealCount == 0)
                    throw new Exception("No meals found in database.");

                
                const string insertPlanSql = @"INSERT INTO MealPlan (user_id, created_at, goal_type) 
                                     VALUES (@uid, @created, @goal);
                                     SELECT last_insert_rowid();";
                using var planCmd = new SqliteCommand(insertPlanSql, conn, transaction);
                planCmd.Parameters.AddWithValue("@uid", userId);
                planCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                planCmd.Parameters.AddWithValue("@goal", "general"); // Simplified for now

                int mealPlanId = Convert.ToInt32(await planCmd.ExecuteScalarAsync());

                
                const string getMealWithNutritionSql = @"
            SELECT m.meal_id, 
                   CAST(SUM(i.calories_per_100g * mi.quantity / 100) AS INT) as total_calories
            FROM Meals m
            LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
            LEFT JOIN Ingredients i ON mi.food_id = i.food_id
            GROUP BY m.meal_id
            ORDER BY RANDOM() LIMIT 3";

                var mealIds = new List<int>();
                using (var mealsCmd = new SqliteCommand(getMealWithNutritionSql, conn, transaction))
                {
                    using var mealReader = await mealsCmd.ExecuteReaderAsync();
                    while (await mealReader.ReadAsync())
                    {
                        mealIds.Add(Convert.ToInt32(mealReader["meal_id"]));
                    }
                }

               
                const string insertMealPlanMealSql = @"INSERT INTO MealPlanMeal (mealPlanId, mealId, mealType, assigned_at, isConsumed) 
                                               VALUES (@planId, @mealId, @mealType, @assignedAt, 0)";
                var mealTypes = new[] { "breakfast", "lunch", "dinner" };

                for (int i = 0; i < mealIds.Count; i++)
                {
                    using var mpmCmd = new SqliteCommand(insertMealPlanMealSql, conn, transaction);
                    mpmCmd.Parameters.AddWithValue("@planId", mealPlanId);
                    mpmCmd.Parameters.AddWithValue("@mealId", mealIds[i]);
                    mpmCmd.Parameters.AddWithValue("@mealType", mealTypes[i]);
                    mpmCmd.Parameters.AddWithValue("@assignedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    await mpmCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return mealPlanId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Generation Failed: {ex.Message}");
            }
        }
        public async Task<List<Meal>> GetMealsForMealPlan(int mealPlanId)
        {
            var meals = new List<Meal>();
            using var conn = new SqliteConnection(_connectionString);

            const string sql = @"
                SELECT 
                    m.meal_id,
                    m.name,
                    m.imageUrl,
                    m.isKeto,
                    m.isVegan,
                    m.isNutFree,
                    m.isLactoseFree,
                    m.isGlutenFree,
                    m.description,
                    mpm.mealType,
                    mpm.isConsumed,
                    COALESCE(SUM(i.calories_per_100g * mi.quantity / 100), 0) as total_calories,
                    COALESCE(SUM(i.protein_per_100g * mi.quantity / 100), 0) as total_protein,
                    COALESCE(SUM(i.carbs_per_100g * mi.quantity / 100), 0) as total_carbs,
                    COALESCE(SUM(i.fat_per_100g * mi.quantity / 100), 0) as total_fat
                FROM Meals m
                INNER JOIN MealPlanMeal mpm ON m.meal_id = mpm.mealId
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE mpm.mealPlanId = @planId
                GROUP BY m.meal_id, m.name, m.imageUrl, m.isKeto, m.isVegan, m.isNutFree, 
                         m.isLactoseFree, m.isGlutenFree, m.description, mpm.mealType, mpm.isConsumed
                ORDER BY 
                    CASE mpm.mealType 
                        WHEN 'breakfast' THEN 1 
                        WHEN 'lunch' THEN 2 
                        WHEN 'dinner' THEN 3 
                        ELSE 4 
                    END";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@planId", mealPlanId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                meals.Add(new Meal
                {
                    Id = Convert.ToInt32(reader["meal_id"]),
                    Name = reader["name"].ToString(),
                    ImageUrl = reader["imageUrl"]?.ToString(),
                    IsKeto = Convert.ToBoolean(reader["isKeto"]),
                    IsVegan = Convert.ToBoolean(reader["isVegan"]),
                    IsNutFree = Convert.ToBoolean(reader["isNutFree"]),
                    IsLactoseFree = Convert.ToBoolean(reader["isLactoseFree"]),
                    IsGlutenFree = Convert.ToBoolean(reader["isGlutenFree"]),
                    Description = reader["description"]?.ToString(),
                    Calories = Convert.ToInt32(reader["total_calories"]),
                    Protein = Convert.ToInt32(reader["total_protein"]),
                    Carbs = Convert.ToInt32(reader["total_carbs"]),
                    Fat = Convert.ToInt32(reader["total_fat"])
                });
            }

            return meals;
        }

        public async Task<List<IngredientViewModel>> GetIngredientsForMeal(int mealId)
        {
            var ingredients = new List<IngredientViewModel>();
            using var conn = new SqliteConnection(_connectionString);

            const string sql = @"
                SELECT 
                    mi.food_id,
                    i.name,
                    mi.quantity,
                    i.calories_per_100g,
                    i.protein_per_100g,
                    i.carbs_per_100g,
                    i.fat_per_100g
                FROM MealsIngredients mi
                INNER JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE mi.meal_id = @mealId
                ORDER BY mi.quantity DESC";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mealId", mealId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int ingredientId = Convert.ToInt32(reader["food_id"]);
                double quantity = Convert.ToDouble(reader["quantity"]);
                double caloriesPer100g = Convert.ToDouble(reader["calories_per_100g"]);
                double proteinPer100g = Convert.ToDouble(reader["protein_per_100g"]);
                double carbsPer100g = Convert.ToDouble(reader["carbs_per_100g"]);
                double fatPer100g = Convert.ToDouble(reader["fat_per_100g"]);

                ingredients.Add(new TeamNut.Views.MealPlanView.IngredientViewModel
                {
                    IngredientId = ingredientId,
                    Name = reader["name"].ToString(),
                    Quantity = quantity,
                    Calories = Math.Round(caloriesPer100g * quantity / 100, 1),
                    Protein = Math.Round(proteinPer100g * quantity / 100, 1),
                    Carbs = Math.Round(carbsPer100g * quantity / 100, 1),
                    Fat = Math.Round(fatPer100g * quantity / 100, 1)
                });
            }

            return ingredients;
        }

        private MealPlan MapReaderToMealPlan(SqliteDataReader reader)
        {
            return new MealPlan
            {
                Id = Convert.ToInt32(reader["mealplan_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                GoalType = reader["goal_type"]?.ToString()
            };
        }

        public async Task SaveMealsToDailyLog(int userId, List<Meal> meals)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                VALUES (@userId, @mealId, @calories, @loggedAt)";

            foreach (var meal in meals)
            {
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@mealId", meal.Id);
                cmd.Parameters.AddWithValue("@calories", meal.Calories);
                cmd.Parameters.AddWithValue("@loggedAt", DateTime.Now);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveMealToDailyLog(int userId, int mealId, int calories)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@mealId", mealId);
            cmd.Parameters.AddWithValue("@calories", calories);
            cmd.Parameters.AddWithValue("@loggedAt", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}