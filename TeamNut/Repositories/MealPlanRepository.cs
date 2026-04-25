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
            connectionString = dbConfig.ConnectionString;
        }

        public async Task<MealPlan?> GetById(int id)
        {
            const string sql = "SELECT * FROM MealPlan WHERE mealplan_id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var selectByIdCommand = new SqliteCommand(sql, conn))
            {
                selectByIdCommand.Parameters.AddWithValue("@id", id);

                using (var reader = await selectByIdCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToMealPlan(reader);
                    }
                }
            }

            return null;
        }

        public async Task<MealPlan?> GetLatestMealPlan(int userId)
        {
            const string sql = @"
                SELECT * FROM MealPlan
                WHERE user_id = @userId
                ORDER BY created_at DESC
                LIMIT 1";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var latestPlanCommand = new SqliteCommand(sql, conn))
            {
                latestPlanCommand.Parameters.AddWithValue("@userId", userId);

                using (var reader = await latestPlanCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToMealPlan(reader);
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<MealPlan>> GetAll()
        {
            const string sql = "SELECT * FROM MealPlan";
            var plans = new List<MealPlan>();

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var selectAllPlansCommand = new SqliteCommand(sql, conn))
            {
                using (var reader = await selectAllPlansCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        plans.Add(MapReaderToMealPlan(reader));
                    }
                }
            }

            return plans;
        }

        public async Task Add(MealPlan entity)
        {
            const string sql = @"
                INSERT INTO MealPlan (user_id, created_at, goal_type)
                VALUES (@uid, @created, @goal)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var insertMealPlanCommand = new SqliteCommand(sql, conn))
            {
                insertMealPlanCommand.Parameters.AddWithValue("@uid", entity.UserId);
                insertMealPlanCommand.Parameters.AddWithValue("@created", entity.CreatedAt);
                insertMealPlanCommand.Parameters.AddWithValue("@goal", entity.GoalType ?? (object)DBNull.Value);

                await insertMealPlanCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task Update(MealPlan entity)
        {
            const string sql = @"
                UPDATE MealPlan 
                SET goal_type = @goal
                WHERE mealplan_id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var updateMealPlanCommand = new SqliteCommand(sql, conn))
            {
                updateMealPlanCommand.Parameters.AddWithValue("@id", entity.Id);
                updateMealPlanCommand.Parameters.AddWithValue("@goal", entity.GoalType);

                await updateMealPlanCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            const string sql = "DELETE FROM MealPlan WHERE mealplan_id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var deleteMealPlanCommand = new SqliteCommand(sql, conn))
            {
                deleteMealPlanCommand.Parameters.AddWithValue("@id", id);
                await deleteMealPlanCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<MealPlan?> GetTodaysMealPlan(int userId)
        {
            const string sql = @"
                SELECT * FROM MealPlan
                WHERE user_id = @userId
                  AND DATE(created_at) = DATE('now', 'localtime')
                ORDER BY created_at DESC
                LIMIT 1";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var todaysPlanCommand = new SqliteCommand(sql, conn))
            {
                todaysPlanCommand.Parameters.AddWithValue("@userId", userId);

                using (var reader = await todaysPlanCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToMealPlan(reader);
                    }
                }
            }

            return null;
        }

        public async Task<int> GeneratePersonalizedDailyMealPlan(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    await EnsureMealsExistAsync(conn, transaction);

                    var nutritionTargets = await LoadUserNutritionTargetsAsync(conn, transaction, userId);
                    int mealPlanId = await InsertMealPlanAsync(conn, transaction, userId, nutritionTargets.goalType);
                    var favouriteMealIds = await LoadFavouriteMealIdsAsync(conn, transaction, userId);
                    var candidateMealPool = await LoadCandidateMealPoolAsync(conn, transaction);

                    var selectedMeals = FindBestMealCombination(
                        candidateMealPool,
                        nutritionTargets.calorieNeeds,
                        favouriteMealIds);

                    await InsertMealPlanMealsAsync(conn, transaction, mealPlanId, selectedMeals);

                    transaction.Commit();
                    return mealPlanId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Generation Failed: {ex.Message}");
                }
            }
        }

        private async Task EnsureMealsExistAsync(SqliteConnection connection, SqliteTransaction transaction)
        {
            const string checkMealsSql = "SELECT COUNT(*) FROM Meals";
            using var checkCommand = new SqliteCommand(checkMealsSql, connection, transaction);
            var mealCountScalar = await checkCommand.ExecuteScalarAsync();
            long mealCount = mealCountScalar != null ? Convert.ToInt64(mealCountScalar) : 0;

            if (mealCount == 0)
            {
                throw new Exception("No meals found in database.");
            }
        }

        private async Task<(int calorieNeeds, string goalType)> LoadUserNutritionTargetsAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int userId)
        {
            int calorieNeeds = 2000;
            string goalType = "general";

            const string getUserDataSql = @"
                SELECT calorie_needs, goal
                FROM UserData
                WHERE user_id = @userId";

            using var userDataCommand = new SqliteCommand(getUserDataSql, connection, transaction);
            userDataCommand.Parameters.AddWithValue("@userId", userId);

            using var userDataReader = await userDataCommand.ExecuteReaderAsync();
            if (await userDataReader.ReadAsync())
            {
                int rawCalories = userDataReader["calorie_needs"] != DBNull.Value
                    ? Convert.ToInt32(userDataReader["calorie_needs"])
                    : 0;

                calorieNeeds = rawCalories > 0 ? rawCalories : 2000;
                goalType = userDataReader["goal"]?.ToString() ?? "general";
            }

            return (calorieNeeds, goalType);
        }

        private async Task<int> InsertMealPlanAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int userId,
            string goalType)
        {
            const string insertPlanSql = @"
                INSERT INTO MealPlan (user_id, created_at, goal_type)
                VALUES (@userId, @createdAt, @goalType);
                SELECT last_insert_rowid();";

            using var mealPlanCommand = new SqliteCommand(insertPlanSql, connection, transaction);
            mealPlanCommand.Parameters.AddWithValue("@userId", userId);
            mealPlanCommand.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            mealPlanCommand.Parameters.AddWithValue("@goalType", goalType);

            return Convert.ToInt32(await mealPlanCommand.ExecuteScalarAsync());
        }

        private async Task<HashSet<int>> LoadFavouriteMealIdsAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int userId)
        {
            var favouriteMealIds = new HashSet<int>();

            const string favoritesSql = @"
                SELECT DISTINCT favorite.mealId
                FROM Favorites favorite
                WHERE favorite.userId = @userId
                  AND NOT EXISTS (
                      SELECT 1
                      FROM MealPlan mealPlan
                      INNER JOIN MealPlanMeal mealPlanMeal ON mealPlan.mealplan_id = mealPlanMeal.mealPlanId
                      WHERE mealPlan.user_id = @userId
                        AND mealPlanMeal.mealId = favorite.mealId
                        AND mealPlan.created_at >= DATE('now', '-3 days', 'localtime')
                  )";

            try
            {
                using var favoritesCommand = new SqliteCommand(favoritesSql, connection, transaction);
                favoritesCommand.Parameters.AddWithValue("@userId", userId);
                using var favoritesReader = await favoritesCommand.ExecuteReaderAsync();
                while (await favoritesReader.ReadAsync())
                {
                    favouriteMealIds.Add(Convert.ToInt32(favoritesReader["mealId"]));
                }
            }
            catch
            {
            }

            return favouriteMealIds;
        }

        private async Task<List<(int mealId, int calories)>> LoadCandidateMealPoolAsync(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            const string mealPoolSql = @"
                SELECT meal_id, total_calories
                FROM (
                    SELECT meal.meal_id,
                           CAST(COALESCE(SUM(ingredient.calories_per_100g * mealIngredient.quantity / 100), 0) AS INT) AS total_calories
                    FROM Meals meal
                    LEFT JOIN MealsIngredients mealIngredient ON meal.meal_id = mealIngredient.meal_id
                    LEFT JOIN Ingredients ingredient ON mealIngredient.food_id = ingredient.food_id
                    GROUP BY meal.meal_id
                )
                ORDER BY RANDOM()
                LIMIT 50";

            var mealPool = new List<(int mealId, int calories)>();
            using var mealPoolCommand = new SqliteCommand(mealPoolSql, connection, transaction);
            using var mealPoolReader = await mealPoolCommand.ExecuteReaderAsync();
            while (await mealPoolReader.ReadAsync())
            {
                mealPool.Add((
                    Convert.ToInt32(mealPoolReader["meal_id"]),
                    Convert.ToInt32(mealPoolReader["total_calories"])));
            }

            if (mealPool.Count < 3)
            {
                throw new Exception("Not enough meals in the database to generate a plan.");
            }

            return mealPool;
        }

        private (int mealId, int calories)[] FindBestMealCombination(
            List<(int mealId, int calories)> mealPool,
            int calorieNeeds,
            HashSet<int> favouriteMealIds)
        {
            int bestBreakfastIndex = 0;
            int bestLunchIndex = 1;
            int bestDinnerIndex = 2;
            int bestScore = int.MaxValue;
            bool bestIncludesFavorite = false;

            for (int breakfastCandidateIndex = 0; breakfastCandidateIndex < mealPool.Count - 2; breakfastCandidateIndex++)
            {
                for (int lunchCandidateIndex = breakfastCandidateIndex + 1; lunchCandidateIndex < mealPool.Count - 1; lunchCandidateIndex++)
                {
                    for (int dinnerCandidateIndex = lunchCandidateIndex + 1; dinnerCandidateIndex < mealPool.Count; dinnerCandidateIndex++)
                    {
                        int score = Math.Abs(
                            mealPool[breakfastCandidateIndex].calories +
                            mealPool[lunchCandidateIndex].calories +
                            mealPool[dinnerCandidateIndex].calories -
                            calorieNeeds);

                        bool includesFavoriteMeal =
                            favouriteMealIds.Contains(mealPool[breakfastCandidateIndex].mealId) ||
                            favouriteMealIds.Contains(mealPool[lunchCandidateIndex].mealId) ||
                            favouriteMealIds.Contains(mealPool[dinnerCandidateIndex].mealId);

                        bool isBetterCombination = score < bestScore ||
                            (includesFavoriteMeal && !bestIncludesFavorite && score <= bestScore + 100);

                        if (isBetterCombination)
                        {
                            bestScore = score;
                            bestIncludesFavorite = includesFavoriteMeal;
                            bestBreakfastIndex = breakfastCandidateIndex;
                            bestLunchIndex = lunchCandidateIndex;
                            bestDinnerIndex = dinnerCandidateIndex;
                        }
                    }
                }
            }

            return new[]
            {
                mealPool[bestBreakfastIndex],
                mealPool[bestLunchIndex],
                mealPool[bestDinnerIndex],
            };
        }

        private async Task InsertMealPlanMealsAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int mealPlanId,
            (int mealId, int calories)[] selectedMeals)
        {
            string[] mealTypes = { "breakfast", "lunch", "dinner" };
            const string insertMealPlanMealSql = @"
                INSERT INTO MealPlanMeal (mealPlanId, mealId, mealType, assigned_at, isConsumed)
                VALUES (@mealPlanId, @mealId, @mealType, @assignedAt, 0)";

            for (int mealIndex = 0; mealIndex < selectedMeals.Length; mealIndex++)
            {
                using var mealPlanMealCommand = new SqliteCommand(insertMealPlanMealSql, connection, transaction);
                mealPlanMealCommand.Parameters.AddWithValue("@mealPlanId", mealPlanId);
                mealPlanMealCommand.Parameters.AddWithValue("@mealId", selectedMeals[mealIndex].mealId);
                mealPlanMealCommand.Parameters.AddWithValue("@mealType", mealTypes[mealIndex]);
                mealPlanMealCommand.Parameters.AddWithValue("@assignedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await mealPlanMealCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Meal>> GetMealsForMealPlan(int mealPlanId)
        {
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

            var meals = new List<Meal>();
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var mealsForPlanCommand = new SqliteCommand(sql, conn))
            {
                mealsForPlanCommand.Parameters.AddWithValue("@planId", mealPlanId);

                using (var reader = await mealsForPlanCommand.ExecuteReaderAsync())
                {
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
                }
            }

            return meals;
        }

        public async Task<List<IngredientViewModel>> GetIngredientsForMeal(int mealId)
        {
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

            var ingredients = new List<IngredientViewModel>();
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var ingredientsForMealCommand = new SqliteCommand(sql, conn))
            {
                ingredientsForMealCommand.Parameters.AddWithValue("@mealId", mealId);

                using (var reader = await ingredientsForMealCommand.ExecuteReaderAsync())
                {
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
                }
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

            const string sql = @"
                INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            foreach (var meal in meals)
            {
                using (var insertDailyLogCommand = new SqliteCommand(sql, conn))
                {
                    insertDailyLogCommand.Parameters.AddWithValue("@userId", userId);
                    insertDailyLogCommand.Parameters.AddWithValue("@mealId", meal.Id);
                    insertDailyLogCommand.Parameters.AddWithValue("@calories", meal.Calories);
                    insertDailyLogCommand.Parameters.AddWithValue("@loggedAt", DateTime.Now);
                    await insertDailyLogCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SaveMealToDailyLog(int userId, int mealId, int calories)
        {
            const string sql = @"
                INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var insertSingleDailyLogCommand = new SqliteCommand(sql, conn))
            {
                insertSingleDailyLogCommand.Parameters.AddWithValue("@userId", userId);
                insertSingleDailyLogCommand.Parameters.AddWithValue("@mealId", mealId);
                insertSingleDailyLogCommand.Parameters.AddWithValue("@calories", calories);
                insertSingleDailyLogCommand.Parameters.AddWithValue("@loggedAt", DateTime.Now);
                await insertSingleDailyLogCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
