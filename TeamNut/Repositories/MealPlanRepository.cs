using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamNut.Repositories; 
using TeamNut.Models;

namespace TeamNut.Repositories
{
    internal class MealPlanRepository : IRepository<MealPlan>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<MealPlan> GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMealPlan(reader);
            return null;
        }

        public async Task<IEnumerable<MealPlan>> GetAll()
        {
            var plans = new List<MealPlan>();
            using var conn = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM MealPlan";
            using var cmd = new SqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) plans.Add(MapReaderToMealPlan(reader));
            return plans;
        }

        public async Task Add(MealPlan entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"INSERT INTO MealPlan (user_id, created_at, goal_type) 
                                VALUES (@uid, @created, @goal)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", entity.UserId);
            cmd.Parameters.AddWithValue("@created", entity.CreatedAt);
            cmd.Parameters.AddWithValue("@goal", entity.GoalType ?? (object)DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(MealPlan entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"UPDATE MealPlan SET goal_type = @goal 
                                 WHERE mealplan_id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@goal", entity.GoalType);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM MealPlan WHERE mealplan_id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MealPlan> GetTodaysMealPlan(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"SELECT TOP 1 * FROM MealPlan 
                                WHERE user_id = @userId 
                                AND CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)
                                ORDER BY created_at DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMealPlan(reader);
            return null;
        }

        public async Task<int> GeneratePersonalizedDailyMealPlan(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                // First check if there are any meals in the database
                const string checkMealsSql = "SELECT COUNT(*) FROM Meals";
                using var checkCmd = new SqlCommand(checkMealsSql, conn, transaction);
                int mealCount = (int)await checkCmd.ExecuteScalarAsync();

                if (mealCount == 0)
                {
                    throw new Exception("No meals found in database. Please add meals before generating a meal plan.");
                }

                const string getUserDataSql = @"SELECT calorie_needs, protein_needs, carb_needs, fat_needs, goal 
                                               FROM UserData WHERE user_id = @userId";
                using var userDataCmd = new SqlCommand(getUserDataSql, conn, transaction);
                userDataCmd.Parameters.AddWithValue("@userId", userId);

                int calorieNeeds = 2000;
                int proteinNeeds = 150;
                int carbNeeds = 200;
                int fatNeeds = 65;
                string goal = "general";

                using var reader = await userDataCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    calorieNeeds = reader["calorie_needs"] != DBNull.Value ? Convert.ToInt32(reader["calorie_needs"]) : 2000;
                    proteinNeeds = reader["protein_needs"] != DBNull.Value ? Convert.ToInt32(reader["protein_needs"]) : 150;
                    carbNeeds = reader["carb_needs"] != DBNull.Value ? Convert.ToInt32(reader["carb_needs"]) : 200;
                    fatNeeds = reader["fat_needs"] != DBNull.Value ? Convert.ToInt32(reader["fat_needs"]) : 65;
                    goal = reader["goal"]?.ToString() ?? "general";
                }
                reader.Close();

                int dailyCalorieMin = calorieNeeds - 200;
                int dailyCalorieMax = calorieNeeds + 200;
                int dailyProteinMin = proteinNeeds - 30;
                int dailyProteinMax = proteinNeeds + 30;
                int dailyCarbMin = carbNeeds - 30;
                int dailyCarbMax = carbNeeds + 30;
                int dailyFatMin = fatNeeds - 30;
                int dailyFatMax = fatNeeds + 30;

                int targetCaloriesPerMeal = calorieNeeds / 3;
                int targetProteinPerMeal = proteinNeeds / 3;
                int targetCarbPerMeal = carbNeeds / 3;
                int targetFatPerMeal = fatNeeds / 3;

                int flexibleCalorieMin = targetCaloriesPerMeal - 250;
                int flexibleCalorieMax = targetCaloriesPerMeal + 250;
                int flexibleProteinMin = Math.Max(0, targetProteinPerMeal - 40);
                int flexibleProteinMax = targetProteinPerMeal + 40;
                int flexibleCarbMin = Math.Max(0, targetCarbPerMeal - 40);
                int flexibleCarbMax = targetCarbPerMeal + 40;
                int flexibleFatMin = Math.Max(0, targetFatPerMeal - 40);
                int flexibleFatMax = targetFatPerMeal + 40;

                const string insertPlanSql = @"INSERT INTO MealPlan (user_id, created_at, goal_type) 
                                               OUTPUT INSERTED.mealplan_id
                                               VALUES (@uid, @created, @goal)";
                using var planCmd = new SqlCommand(insertPlanSql, conn, transaction);
                planCmd.Parameters.AddWithValue("@uid", userId);
                planCmd.Parameters.AddWithValue("@created", DateTime.Now);
                planCmd.Parameters.AddWithValue("@goal", goal);

                int mealPlanId = (int)await planCmd.ExecuteScalarAsync();

                const string getMealWithNutritionSql = @"
                    WITH MealNutrition AS (
                        SELECT 
                            m.meal_id,
                            COALESCE(SUM(i.calories_per_100g * mi.quantity / 100), 0) as total_calories,
                            COALESCE(SUM(i.protein_per_100g * mi.quantity / 100), 0) as total_protein,
                            COALESCE(SUM(i.carbs_per_100g * mi.quantity / 100), 0) as total_carbs,
                            COALESCE(SUM(i.fat_per_100g * mi.quantity / 100), 0) as total_fat
                        FROM Meals m
                        LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                        LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                        GROUP BY m.meal_id
                    )
                    SELECT TOP 20 meal_id, total_calories, total_protein, total_carbs, total_fat
                    FROM MealNutrition
                    WHERE total_calories BETWEEN @minCal AND @maxCal
                        AND total_protein BETWEEN @minPro AND @maxPro
                        AND total_carbs BETWEEN @minCarb AND @maxCarb
                        AND total_fat BETWEEN @minFat AND @maxFat
                    ORDER BY NEWID()";

                var mealTypes = new[] { "breakfast", "lunch", "dinner" };
                List<(int mealId, int calories, int protein, int carbs, int fat)> selectedMeals = null;
                List<(int mealId, int calories, int protein, int carbs, int fat)> bestAttempt = null;
                int maxAttempts = 20;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var candidateMeals = new List<(int mealId, int calories, int protein, int carbs, int fat)>();

                    using var mealsCmd = new SqlCommand(getMealWithNutritionSql, conn, transaction);
                    mealsCmd.Parameters.AddWithValue("@minCal", flexibleCalorieMin);
                    mealsCmd.Parameters.AddWithValue("@maxCal", flexibleCalorieMax);
                    mealsCmd.Parameters.AddWithValue("@minPro", flexibleProteinMin);
                    mealsCmd.Parameters.AddWithValue("@maxPro", flexibleProteinMax);
                    mealsCmd.Parameters.AddWithValue("@minCarb", flexibleCarbMin);
                    mealsCmd.Parameters.AddWithValue("@maxCarb", flexibleCarbMax);
                    mealsCmd.Parameters.AddWithValue("@minFat", flexibleFatMin);
                    mealsCmd.Parameters.AddWithValue("@maxFat", flexibleFatMax);

                    using var mealReader = await mealsCmd.ExecuteReaderAsync();
                    while (await mealReader.ReadAsync())
                    {
                        candidateMeals.Add((
                            Convert.ToInt32(mealReader["meal_id"]),
                            Convert.ToInt32(mealReader["total_calories"]),
                            Convert.ToInt32(mealReader["total_protein"]),
                            Convert.ToInt32(mealReader["total_carbs"]),
                            Convert.ToInt32(mealReader["total_fat"])
                        ));
                    }
                    mealReader.Close();

                    if (candidateMeals.Count >= 3)
                    {
                        var testMeals = candidateMeals.Take(3).ToList();

                        // Keep track of best attempt
                        if (bestAttempt == null)
                        {
                            bestAttempt = testMeals;
                        }

                        int totalCalories = testMeals.Sum(m => m.calories);
                        int totalProtein = testMeals.Sum(m => m.protein);
                        int totalCarbs = testMeals.Sum(m => m.carbs);
                        int totalFat = testMeals.Sum(m => m.fat);

                        if (totalCalories >= dailyCalorieMin && totalCalories <= dailyCalorieMax &&
                            totalProtein >= dailyProteinMin && totalProtein <= dailyProteinMax &&
                            totalCarbs >= dailyCarbMin && totalCarbs <= dailyCarbMax &&
                            totalFat >= dailyFatMin && totalFat <= dailyFatMax)
                        {
                            selectedMeals = testMeals;
                            break;
                        }
                        else
                        {
                            // Update best attempt if this one is closer to calorie target
                            int currentDiff = Math.Abs(totalCalories - calorieNeeds);
                            int bestDiff = Math.Abs(bestAttempt.Sum(m => m.calories) - calorieNeeds);
                            if (currentDiff < bestDiff)
                            {
                                bestAttempt = testMeals;
                            }
                        }
                    }
                }

                List<int> mealIds;
                if (selectedMeals != null && selectedMeals.Count == 3)
                {
                    mealIds = selectedMeals.Select(m => m.mealId).ToList();
                }
                else if (bestAttempt != null && bestAttempt.Count >= 3)
                {
                    // Use best attempt if no perfect match found
                    mealIds = bestAttempt.Take(3).Select(m => m.mealId).ToList();
                }
                else
                {
                    // Only fall back to random if absolutely necessary
                    mealIds = new List<int>();
                    const string fallbackSql = "SELECT TOP 3 meal_id FROM Meals ORDER BY NEWID()";
                    using var fallbackCmd = new SqlCommand(fallbackSql, conn, transaction);
                    var fallbackReader = await fallbackCmd.ExecuteReaderAsync();
                    while (await fallbackReader.ReadAsync())
                    {
                        mealIds.Add(Convert.ToInt32(fallbackReader["meal_id"]));
                    }
                    fallbackReader.Close();
                }

                if (mealIds.Count == 0)
                {
                    throw new Exception("Could not select any meals for the plan. Database may be empty.");
                }

                const string insertMealPlanMealSql = @"INSERT INTO MealPlanMeal (mealPlanId, mealId, mealType, assigned_at, isConsumed) 
                                                       VALUES (@planId, @mealId, @mealType, @assignedAt, 0)";

                for (int i = 0; i < mealIds.Count && i < mealTypes.Length; i++)
                {
                    using var mealPlanMealCmd = new SqlCommand(insertMealPlanMealSql, conn, transaction);
                    mealPlanMealCmd.Parameters.AddWithValue("@planId", mealPlanId);
                    mealPlanMealCmd.Parameters.AddWithValue("@mealId", mealIds[i]);
                    mealPlanMealCmd.Parameters.AddWithValue("@mealType", mealTypes[i]);
                    mealPlanMealCmd.Parameters.AddWithValue("@assignedAt", DateTime.Now);
                    await mealPlanMealCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return mealPlanId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Meal>> GetMealsForMealPlan(int mealPlanId)
        {
            var meals = new List<Meal>();
            using var conn = new SqlConnection(_connectionString);

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

            using var cmd = new SqlCommand(sql, conn);
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

        public async Task<List<Views.MealPlanView.IngredientViewModel>> GetIngredientsForMeal(int mealId)
        {
            var ingredients = new List<Views.MealPlanView.IngredientViewModel>();
            using var conn = new SqlConnection(_connectionString);

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

            using var cmd = new SqlCommand(sql, conn);
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

                ingredients.Add(new Views.MealPlanView.IngredientViewModel
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

        private MealPlan MapReaderToMealPlan(SqlDataReader reader)
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
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                VALUES (@userId, @mealId, @calories, @loggedAt)";

            foreach (var meal in meals)
            {
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@mealId", meal.Id);
                cmd.Parameters.AddWithValue("@calories", meal.Calories);
                cmd.Parameters.AddWithValue("@loggedAt", DateTime.Now);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveMealToDailyLog(int userId, int mealId, int calories)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"INSERT INTO DailyLogs (user_id, mealId, calories, created_at)
                                VALUES (@userId, @mealId, @calories, @loggedAt)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@mealId", mealId);
            cmd.Parameters.AddWithValue("@calories", calories);
            cmd.Parameters.AddWithValue("@loggedAt", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
