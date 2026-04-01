using Microsoft.Data.SqlClient;
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

            using var conn = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT i.name, mi.quantity
                FROM MealsIngredients mi
                INNER JOIN Ingredients i ON i.food_id = mi.food_id
                WHERE mi.meal_id = @mealId
                ORDER BY mi.quantity DESC, i.name";

            using var cmd = new SqlCommand(sql, conn);
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
            var meals = new List<Meal>();
            StringBuilder sql = new StringBuilder(@"
                SELECT 
                    m.*,
                    CAST(ISNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(ISNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(ISNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(ISNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE 1=1");
            var parameters = new List<SqlParameter>();

            
            if (filter.IsKeto) sql.Append(" AND m.isKeto = 1");
            if (filter.IsVegan) sql.Append(" AND m.isVegan = 1");
            if (filter.IsNutFree) sql.Append(" AND m.isNutFree = 1");
            if (filter.IsLactoseFree) sql.Append(" AND m.isLactoseFree = 1");
            if (filter.IsGlutenFree) sql.Append(" AND m.isGlutenFree = 1");

            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                sql.Append(" AND m.name LIKE @search");
                parameters.Add(new SqlParameter("@search", $"%{filter.SearchTerm}%"));
            }

            sql.Append(@" GROUP BY 
                m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description");

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());

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
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT 
                    m.*,
                    CAST(ISNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(ISNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(ISNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(ISNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                WHERE m.meal_id = @id
                GROUP BY m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapReaderToMeal(reader);
            return null;
        }

        public async Task<IEnumerable<Meal>> GetAll()
        {
            var meals = new List<Meal>();
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT 
                    m.*,
                    CAST(ISNULL(SUM(i.calories_per_100g * mi.quantity / 100.0), 0) AS INT) AS calories,
                    CAST(ISNULL(SUM(i.protein_per_100g * mi.quantity / 100.0), 0) AS INT) AS protein,
                    CAST(ISNULL(SUM(i.carbs_per_100g * mi.quantity / 100.0), 0) AS INT) AS carbs,
                    CAST(ISNULL(SUM(i.fat_per_100g * mi.quantity / 100.0), 0) AS INT) AS fat
                FROM Meals m
                LEFT JOIN MealsIngredients mi ON m.meal_id = mi.meal_id
                LEFT JOIN Ingredients i ON mi.food_id = i.food_id
                GROUP BY m.meal_id, m.imageUrl, m.name, m.isKeto, m.isLactoseFree, m.isNutFree, m.isVegan, m.isGlutenFree, m.description";
            using var cmd = new SqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) meals.Add(MapReaderToMeal(reader));
            return meals;
        }

        public async Task Add(Meal entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"INSERT INTO Meals (name, imageUrl, isKeto, isVegan, isNutFree, isLactoseFree, isGlutenFree, description) 
                                VALUES (@name, @img, @keto, @vegan, @nut, @lac, @glu, @desc)";
            using var cmd = new SqlCommand(sql, conn);
            AddMealParameters(cmd, entity);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Meal entity)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = @"UPDATE Meals SET name=@name, imageUrl=@img, isKeto=@keto, isVegan=@vegan, 
                                 isNutFree=@nut, isLactoseFree=@lac, isGlutenFree=@glu, description=@desc 
                                 WHERE meal_id=@id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            AddMealParameters(cmd, entity);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Meals WHERE meal_id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private void AddMealParameters(SqlCommand cmd, Meal meal)
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

        private Meal MapReaderToMeal(SqlDataReader reader)
        {
            return new Meal
            {
                Id = Convert.ToInt32(reader["meal_id"]),
                Name = reader["name"].ToString(),
                ImageUrl = reader["imageUrl"]?.ToString(),
                Calories = Convert.ToInt32(reader["calories"]),
                Protein = Convert.ToInt32(reader["protein"]),
                Carbs = Convert.ToInt32(reader["carbs"]),
                Fat = Convert.ToInt32(reader["fat"]),
                IsKeto = Convert.ToBoolean(reader["isKeto"]),
                IsVegan = Convert.ToBoolean(reader["isVegan"]),
                IsNutFree = Convert.ToBoolean(reader["isNutFree"]),
                IsLactoseFree = Convert.ToBoolean(reader["isLactoseFree"]),
                IsGlutenFree = Convert.ToBoolean(reader["isGlutenFree"]),
                Description = reader["description"]?.ToString()
            };
        }
    }
}