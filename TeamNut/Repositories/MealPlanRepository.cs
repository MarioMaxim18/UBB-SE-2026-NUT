namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Views.MealPlanView;
    using TeamNut.Repositories.Interfaces;

    internal class MealPlanRepository : IMealPlanRepository
    {
        private readonly string connectionString;

        public MealPlanRepository(IDbConfig dbConfig)
        {
            this.connectionString = dbConfig.ConnectionString;
        }

        public async Task<MealPlan?> GetById(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return this.MapReaderToMealPlan(reader);
            }
            return null;
        }

        public async Task<MealPlan?> GetLatestMealPlan(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = @"SELECT * FROM MealPlan
                                WHERE user_id = @userId
                                ORDER BY created_at DESC
                                LIMIT 1";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return this.MapReaderToMealPlan(reader);
            }
            return null;
        }

        public async Task<IEnumerable<MealPlan>> GetAll()
        {
            var plans = new List<MealPlan>();
            using var conn = new SqliteConnection(connectionString);
            const string sql = "SELECT * FROM MealPlan";
            using var cmd = new SqliteCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                plans.Add(this.MapReaderToMealPlan(reader));
            }
            return plans;
        }

        public async Task Add(MealPlan entity)
        {
            using var conn = new SqliteConnection(connectionString);
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
            using var conn = new SqliteConnection(connectionString);
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
            using var conn = new SqliteConnection(connectionString);
            const string sql = "DELETE FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MealPlan?> GetTodaysMealPlan(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            const string sql = @"SELECT * FROM MealPlan
                                WHERE user_id = @userId
                                  AND DATE(created_at) = DATE('now', 'localtime')
                                ORDER BY created_at DESC
                                LIMIT 1";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return this.MapReaderToMealPlan(reader);
            }
            return null;
        }

        public async Task<int> GeneratePersonalizedDailyMealPlan(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string checkMealsSql = "SELECT COUNT(*) FROM Meals";
                using var checkCmd = new SqliteCommand(checkMealsSql, conn, transaction);
                var mealCountScalar = await checkCmd.ExecuteScalarAsync();
                long mealCount = mealCountScalar != null ? Convert.ToInt64(mealCountScalar) : 0;
                if (mealCount == 0)
                {
                    throw new Exception("No meals found in database.");
                }

                const string getUserDataSql = @"SELECT calorie_needs, protein_needs, carb_needs, fat_needs, goal
                                                FROM UserData WHERE user_id = @userId";
                using var userDataCmd = new SqliteCommand(getUserDataSql, conn, transaction);
                userDataCmd.Parameters.AddWithValue("@userId", userId);

                int calorieNeeds = 2000, proteinNeeds = 150, carbohydrateNeeds = 200, fatNeeds = 65;
                string goal = "general";

                using (var udReader = await userDataCmd.ExecuteReaderAsync())
                {
                    if (await udReader.ReadAsync())
                    {
                        int rawCalories = udReader["calorie_needs"] != DBNull.Value ? Convert.ToInt32(udReader["calorie_needs"]) : 0;
                        int rawProtein = udReader["protein_needs"] != DBNull.Value ? Convert.ToInt32(udReader["protein_needs"]) : 0;
                        int rawCarbohydrates = udReader["carb_needs"] != DBNull.Value ? Convert.ToInt32(udReader["carb_needs"]) : 0;
                        int rawFat = udReader["fat_needs"] != DBNull.Value ? Convert.ToInt32(udReader["fat_needs"]) : 0;
                        calorieNeeds = rawCalories > 0 ? rawCalories : 2000;
                        proteinNeeds = rawProtein > 0 ? rawProtein : 150;
                        carbohydrateNeeds = rawCarbohydrates > 0 ? rawCarbohydrates : 200;
                        fatNeeds = rawFat > 0 ? rawFat : 65;
                        goal = udReader["goal"]?.ToString() ?? "general";
                    }
                }

                const string insertPlanSql = @"INSERT INTO MealPlan (user_id, created_at, goal_type)
                                               VALUES (@uid, @created, @goal);
                                               SELECT last_insert_rowid();";
                using var planCmd = new SqliteCommand(insertPlanSql, conn, transaction);
                planCmd.Parameters.AddWithValue("@uid", userId);
                planCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                planCmd.Parameters.AddWithValue("@goal", goal);
                int mealPlanId = Convert.ToInt32(await planCmd.ExecuteScalarAsync());

                var favouriteIds = new HashSet<int>();
                try
                {
                    const string favSql = @"
                        SELECT DISTINCT f.mealId
                        FROM Favorites f
                        WHERE f.userId = @userId
                          AND NOT EXISTS (
                              SELECT 1
                              FROM MealPlan mp
                              INNER JOIN MealPlanMeal mpm ON mp.mealplan_id = mpm.mealPlanId
                              WHERE mp.user_id = @userId
                                AND mpm.mealId = f.mealId
                                AND mp.created_at >= DATE('now', '-3 days', 'localtime')
                          )";
                    using var favCmd = new SqliteCommand(favSql, conn, transaction);
                    favCmd.Parameters.AddWithValue("@userId", userId);
                    using var favReader = await favCmd.ExecuteReaderAsync();
                    while (await favReader.ReadAsync())
                    {
                        favouriteIds.Add(Convert.ToInt32(favReader["mealId"]));
                    }
                }
                catch
                {
                }

                const string poolSql = @"
                    SELECT meal_id, total_calories, total_protein, total_carbohydrates, total_fat
                    FROM (
                        SELECT m.meal_id,
                               CAST(COALESCE(SUM(i.calories_per_100g * mi.quantity / 100), 0) AS INT) AS total_calories,
                               CAST(COALESCE(SUM(i.protein_per_100g  * mi.quantity / 100), 0) AS INT) AS total_protein,
                               CAST(COALESCE(SUM(i.carbs_per_100g    * mi.quantity / 100), 0) AS INT) AS total_carbohydrates,
                               CAST(COALESCE(SUM(i.fat_per_100g      * mi.quantity / 100), 0) AS INT) AS total_fat
                        FROM Meals m
                        LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                        LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                        GROUP BY m.meal_id
                    )
                    ORDER BY RANDOM()
                    LIMIT 50";

                var pool = new List<(int id, int calories, int protein, int carbohydrates, int fat)>();
                using (var poolCmd = new SqliteCommand(poolSql, conn, transaction))
                using (var poolReader = await poolCmd.ExecuteReaderAsync())
                {
                    while (await poolReader.ReadAsync())
                    {
                        pool.Add((
                            Convert.ToInt32(poolReader["meal_id"]),
                            Convert.ToInt32(poolReader["total_calories"]),
                            Convert.ToInt32(poolReader["total_protein"]),
                            Convert.ToInt32(poolReader["total_carbohydrates"]),
                            Convert.ToInt32(poolReader["total_fat"])));
                    }
                }

                if (pool.Count < 3)
                {
                    throw new Exception("Not enough meals in the database to generate a plan.");
                }

                int bestBreakfastIndex = 0, bestLunchIndex = 1, bestDinnerIndex = 2;
                int bestScore = int.MaxValue;
                bool bestHasFavourite = false;

                for (int i = 0; i < pool.Count - 2; i++)
                {
                    for (int j = i + 1; j < pool.Count - 1; j++)
                    {
                        for (int k = j + 1; k < pool.Count; k++)
                        {
                            int score = Math.Abs(pool[i].calories + pool[j].calories + pool[k].calories - calorieNeeds);
                            bool hasFav = favouriteIds.Contains(pool[i].id)
                                       || favouriteIds.Contains(pool[j].id)
                                       || favouriteIds.Contains(pool[k].id);

                            bool better = score < bestScore
                                       || (hasFav && !bestHasFavourite && score <= bestScore + 100);

                            if (better)
                            {
                                bestScore = score;
                                bestHasFavourite = hasFav;
                                bestBreakfastIndex = i;
                                bestLunchIndex = j;
                                bestDinnerIndex = k;
                            }
                        }
                    }
                }

                var selected = new[] { pool[bestBreakfastIndex], pool[bestLunchIndex], pool[bestDinnerIndex] };
                var mealTypes = new[] { "breakfast", "lunch", "dinner" };

                const string insertMealPlanMealSql = @"INSERT INTO MealPlanMeal (mealPlanId, mealId, mealType, assigned_at, isConsumed)
                                                       VALUES (@planId, @mealId, @mealType, @assignedAt, 0)";

                for (int i = 0; i < selected.Length; i++)
                {
                    using var mpmCmd = new SqliteCommand(insertMealPlanMealSql, conn, transaction);
                    mpmCmd.Parameters.AddWithValue("@planId", mealPlanId);
                    mpmCmd.Parameters.AddWithValue("@mealId", selected[i].id);
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
            using var conn = new SqliteConnection(connectionString);

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
                    COALESCE(SUM(i.carbs_per_100g * mi.quantity / 100), 0) as total_carbohydrates,
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
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    ImageUrl = reader["imageUrl"]?.ToString() ?? string.Empty,
                    IsKeto = Convert.ToBoolean(reader["isKeto"]),
                    IsVegan = Convert.ToBoolean(reader["isVegan"]),
                    IsNutFree = Convert.ToBoolean(reader["isNutFree"]),
                    IsLactoseFree = Convert.ToBoolean(reader["isLactoseFree"]),
                    IsGlutenFree = Convert.ToBoolean(reader["isGlutenFree"]),
                    Description = reader["description"]?.ToString() ?? string.Empty,
                    Calories = Convert.ToInt32(reader["total_calories"]),
                    Protein = Convert.ToInt32(reader["total_protein"]),
                    Carbohydrates = Convert.ToInt32(reader["total_carbohydrates"]),
                    Fat = Convert.ToInt32(reader["total_fat"]),
                });
            }

            return meals;
        }

        public async Task<List<IngredientViewModel>> GetIngredientsForMeal(int mealId)
        {
            var ingredients = new List<IngredientViewModel>();
            using var conn = new SqliteConnection(connectionString);

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
                double carbohydratesPer100g = Convert.ToDouble(reader["carbs_per_100g"]);
                double fatPer100g = Convert.ToDouble(reader["fat_per_100g"]);

                ingredients.Add(new IngredientViewModel
                {
                    IngredientId = ingredientId,
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Quantity = quantity,
                    Calories = Math.Round(caloriesPer100g * quantity / 100, 1),
                    Protein = Math.Round(proteinPer100g * quantity / 100, 1),
                    Carbohydrates = Math.Round(carbohydratesPer100g * quantity / 100, 1),
                    Fat = Math.Round(fatPer100g * quantity / 100, 1),
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
                GoalType = reader["goal_type"]?.ToString() ?? string.Empty,
            };
        }

        public async Task SaveMealsToDailyLog(int userId, List<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
            {
                return;
            }

            using var conn = new SqliteConnection(connectionString);
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
            using var conn = new SqliteConnection(connectionString);
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
