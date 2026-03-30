using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using TeamNut.Repositories;

namespace TeamNut.Repositories
{
    
    internal class MealRepository : IRepository<Meal>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<IEnumerable<Meal>> GetFilteredMeals(MealFilter filter)
        {
            var meals = new List<Meal>();
            StringBuilder sql = new StringBuilder("SELECT * FROM Meals WHERE 1=1");
            var parameters = new List<SqlParameter>();

            
            if (filter.IsKeto) sql.Append(" AND isKeto = 1");
            if (filter.IsVegan) sql.Append(" AND isVegan = 1");
            if (filter.IsNutFree) sql.Append(" AND isNutFree = 1");
            if (filter.IsLactoseFree) sql.Append(" AND isLactoseFree = 1");
            if (filter.IsGlutenFree) sql.Append(" AND isGlutenFree = 1");

            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                sql.Append(" AND name LIKE @search");
                parameters.Add(new SqlParameter("@search", $"%{filter.SearchTerm}%"));
            }

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
            const string sql = "SELECT * FROM Meals WHERE meal_id = @id";
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
            const string sql = "SELECT * FROM Meals";
            using var cmd = new SqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) meals.Add(MapReaderToMeal(reader));
            return meals;
        }

       
        public async Task<IEnumerable<Meal>> GetFilteredMeals(MealFilter filter)
        {
            var meals = new List<Meal>();
            StringBuilder sql = new StringBuilder("SELECT * FROM Meals WHERE 1=1");
            var parameters = new List<SqlParameter>();

            if (filter.IsKeto) sql.Append(" AND isKeto = 1");
            if (filter.IsVegan) sql.Append(" AND isVegan = 1");
            if (filter.IsNutFree) sql.Append(" AND isNutFree = 1");
            if (filter.IsLactoseFree) sql.Append(" AND isLactoseFree = 1");
            if (filter.IsGlutenFree) sql.Append(" AND isGlutenFree = 1");

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                sql.Append(" AND name LIKE @search");
                parameters.Add(new SqlParameter("@search", $"%{filter.SearchTerm}%"));
            }

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());

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