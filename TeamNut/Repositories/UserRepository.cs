using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly string _connectionString = DbConfig.ConnectionString;

        public async Task<User> GetById(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT id, username, password, role FROM Users WHERE id = @userId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader[0]),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            }
            return null;
        }

        public async Task<User> GetByUsernameAndPassword(string username, string password)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT id, username, password, role FROM Users WHERE username = @username AND password = @password";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader[0]),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not initialized.");
            }

            var usersList = new List<User>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "SELECT id, username, password, role FROM Users";
            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usersList.Add(new User
                {
                    Id = Convert.ToInt32(reader[0]),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }

            return usersList;
        }

        public async Task Add(User entity)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "INSERT INTO Users (username, password, role) VALUES (@username, @password, @role); SELECT last_insert_rowid();";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@username", entity.Username);
            command.Parameters.AddWithValue("@password", entity.Password);
            command.Parameters.AddWithValue("@role", entity.Role);

            await connection.OpenAsync();

            // Get the ID of the newly created user
            var insertedId = await command.ExecuteScalarAsync();

            if (insertedId != null)
            {
                entity.Id = Convert.ToInt32(insertedId);
            }
        }

        public async Task Update(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "UPDATE Users SET username=@username, password=@password, role=@role WHERE id=@userId";
            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@userId", user.Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task Delete(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "DELETE FROM Users WHERE id = @userId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<UserData> GetUserDataByUserId(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = "SELECT id, user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs FROM UserData WHERE user_id = @userId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserData
                {
                    Id = Convert.ToInt32(reader[0]),
                    UserId = reader.GetInt32(1),
                    Weight = Convert.ToInt32(reader.GetValue(2)),
                    Height = Convert.ToInt32(reader.GetValue(3)),
                    Age = reader.GetInt32(4),
                    Gender = reader.GetString(5),
                    Goal = reader.GetString(6),
                    BodyMassIndex = Convert.ToInt32(reader.GetValue(7)),
                    CalorieNeeds = Convert.ToInt32(reader.GetValue(8)),
                    ProteinNeeds = Convert.ToInt32(reader.GetValue(9)),
                    CarbohydrateNeeds = Convert.ToInt32(reader.GetValue(10)),
                    FatNeeds = Convert.ToInt32(reader.GetValue(11))
                };
            }
            return null;
        }

        public async Task AddUserData(UserData data)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"INSERT INTO UserData (user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs)
                                  VALUES (@userId, @weight, @height, @age, @gender, @goal, @bodyMassIndex, @calorieNeeds, @proteinNeeds, @carbohydrateNeeds, @fatNeeds)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", data.UserId);
            command.Parameters.AddWithValue("@weight", data.Weight);
            command.Parameters.AddWithValue("@height", data.Height);
            command.Parameters.AddWithValue("@age", data.Age);
            command.Parameters.AddWithValue("@gender", data.Gender);
            command.Parameters.AddWithValue("@goal", data.Goal);
            command.Parameters.AddWithValue("@bodyMassIndex", data.BodyMassIndex);
            command.Parameters.AddWithValue("@calorieNeeds", data.CalorieNeeds);
            command.Parameters.AddWithValue("@proteinNeeds", data.ProteinNeeds);
            command.Parameters.AddWithValue("@carbohydrateNeeds", data.CarbohydrateNeeds);
            command.Parameters.AddWithValue("@fatNeeds", data.FatNeeds);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserData(UserData data)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string query = @"UPDATE UserData 
                                  SET weight = @weight, height = @height, age = @age, gender = @gender, goal = @goal, 
                                      bmi = @bodyMassIndex, calorie_needs = @calorieNeeds, protein_needs = @proteinNeeds, 
                                      carb_needs = @carbohydrateNeeds, fat_needs = @fatNeeds
                                  WHERE user_id = @userId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@userId", data.UserId);
            command.Parameters.AddWithValue("@weight", data.Weight);
            command.Parameters.AddWithValue("@height", data.Height);
            command.Parameters.AddWithValue("@age", data.Age);
            command.Parameters.AddWithValue("@gender", data.Gender);
            command.Parameters.AddWithValue("@goal", data.Goal);
            command.Parameters.AddWithValue("@bodyMassIndex", data.BodyMassIndex);
            command.Parameters.AddWithValue("@calorieNeeds", data.CalorieNeeds);
            command.Parameters.AddWithValue("@proteinNeeds", data.ProteinNeeds);
            command.Parameters.AddWithValue("@carbohydrateNeeds", data.CarbohydrateNeeds);
            command.Parameters.AddWithValue("@fatNeeds", data.FatNeeds);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}