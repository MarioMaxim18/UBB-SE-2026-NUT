using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using NutApp.Domain;

namespace NutApp.Backend.Repositories
{
    public class MealRepository
    {
        private readonly string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=NutAppDB;Trusted_Connection=True;";


        public List<Meal> GetAllMeals()
        {
            var meals = new List<Meal>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Meals";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) meals.Add(MapReaderToMeal(reader));
                }
            }
            return meals;
        }

        public List<Meal> GetMealsByFilter(string filterType)
        {
            var meals = new List<Meal>();
            string columnName = filterType.ToLower() switch
            {
                "keto" => "isKeto",
                "vegan" => "isVegan",
                "nutfree" => "isNutFree",
                "lactosefree" => "isLactoseFree",
                "glutenfree" => "isGlutenFree",
                _ => null
            };

            if (columnName == null) return GetAllMeals();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = $"SELECT * FROM Meals WHERE {columnName} = 1";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) meals.Add(MapReaderToMeal(reader));
                }
            }
            return meals;
        }


        public void AddFavorite(int userId, int mealId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO Favorites (userId, mealId) VALUES (@uid, @mid)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@mid", mealId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void RemoveFavorite(int userId, int mealId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Favorites WHERE userId = @uid AND mealId = @mid";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@mid", mealId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Meal> GetUserFavorites(int userId)
        {
            var favorites = new List<Meal>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT m.* FROM Meals m 
                               INNER JOIN Favorites f ON m.meal_id = f.mealId 
                               WHERE f.userId = @uid";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) favorites.Add(MapReaderToMeal(reader));
                }
            }
            return favorites;
        }

        private Meal MapReaderToMeal(SqlDataReader reader)
        {
            return new Meal
            {
                Id = Convert.ToInt32(reader["meal_id"]),
                Name = reader["name"].ToString(),
                Description = reader["description"]?.ToString(),
                ImageUrl = reader["imageUrl"]?.ToString(),
                IsKeto = Convert.ToBoolean(reader["isKeto"]),
                IsVegan = Convert.ToBoolean(reader["isVegan"]),
                IsNutFree = Convert.ToBoolean(reader["isNutFree"]),
                IsLactoseFree = Convert.ToBoolean(reader["isLactoseFree"]),
                IsGlutenFree = Convert.ToBoolean(reader["isGlutenFree"])
            };
        }
    }
}