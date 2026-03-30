using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
            const string sql = @"INSERT INTO MealPlan (user_id, created_at, [goal type]) 
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
            const string sql = @"UPDATE MealPlan SET [goal type] = @goal 
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

        
        private MealPlan MapReaderToMealPlan(SqlDataReader reader)
        {
            return new MealPlan
            {
                Id = Convert.ToInt32(reader["mealplan_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                GoalType = reader["goal type"]?.ToString()
            };
        }
    }
}