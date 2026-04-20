using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeamNut.Models;

namespace TeamNut.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly string connectionString = DbConfig.ConnectionString;

        public async Task<User?> GetById(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT id, username, password, role FROM Users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader[0]),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3),
                };
            }
            return null;
        }

        public async Task AddUserData(UserData data)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(@"
            INSERT INTO UserData (user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs)
            VALUES (@userId, @w, @h, @a, @gen, @goal, @bmi, @cal, @pro, @carb, @fat)", conn);

            cmd.Parameters.AddWithValue("@userId", data.UserId);
            cmd.Parameters.AddWithValue("@w", data.Weight);
            cmd.Parameters.AddWithValue("@h", data.Height);
            cmd.Parameters.AddWithValue("@a", data.Age);
            cmd.Parameters.AddWithValue("@gen", data.Gender);
            cmd.Parameters.AddWithValue("@goal", data.Goal);
            cmd.Parameters.AddWithValue("@bmi", data.Bmi);
            cmd.Parameters.AddWithValue("@cal", data.CalorieNeeds);
            cmd.Parameters.AddWithValue("@pro", data.ProteinNeeds);
            cmd.Parameters.AddWithValue("@carb", data.CarbNeeds);
            cmd.Parameters.AddWithValue("@fat", data.FatNeeds);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Add(User entity)
        {
            using var conn = new SqliteConnection(connectionString);
            string sql = "INSERT INTO Users (username, password, role) VALUES (@u, @p, @r); SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", entity.Username);
            cmd.Parameters.AddWithValue("@p", entity.Password);
            cmd.Parameters.AddWithValue("@r", entity.Role);

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            if (result != null)
            {
                entity.Id = Convert.ToInt32(result);
            }
        }

        public async Task Update(User entity)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("UPDATE Users SET username=@u, password=@p, role=@r WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@u", entity.Username);
            cmd.Parameters.AddWithValue("@p", entity.Password);
            cmd.Parameters.AddWithValue("@r", entity.Role);
            cmd.Parameters.AddWithValue("@id", entity.Id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("DELETE FROM Users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<User?> GetByUsernameAndPassword(string username, string password)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT id, username, password, role FROM Users WHERE username = @u AND password = @p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader[0]),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3),
                };
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not initialized.");
            }

            var users = new List<User>();

            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                using var command = new SqliteCommand("SELECT id, username, password, role FROM Users", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(reader[0]),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Role = reader.GetString(3),
                    });
                }
            }
            return users;
        }

        public async Task<UserData?> GetUserDataByUserId(int userId)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand("SELECT id, user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs FROM UserData WHERE user_id = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

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
                    Bmi = Convert.ToInt32(reader.GetValue(7)),
                    CalorieNeeds = Convert.ToInt32(reader.GetValue(8)),
                    ProteinNeeds = Convert.ToInt32(reader.GetValue(9)),
                    CarbNeeds = Convert.ToInt32(reader.GetValue(10)),
                    FatNeeds = Convert.ToInt32(reader.GetValue(11)),
                };
            }
            return null;
        }

        public async Task UpdateUserData(UserData data)
        {
            using var conn = new SqliteConnection(connectionString);
            using var cmd = new SqliteCommand(@"
            UPDATE UserData
            SET weight = @w, height = @h, age = @a, gender = @gen, goal = @goal,
                bmi = @bmi, calorie_needs = @cal, protein_needs = @pro,
                carb_needs = @carb, fat_needs = @fat
            WHERE user_id = @userId", conn);

            cmd.Parameters.AddWithValue("@userId", data.UserId);
            cmd.Parameters.AddWithValue("@w", data.Weight);
            cmd.Parameters.AddWithValue("@h", data.Height);
            cmd.Parameters.AddWithValue("@a", data.Age);
            cmd.Parameters.AddWithValue("@gen", data.Gender);
            cmd.Parameters.AddWithValue("@goal", data.Goal);
            cmd.Parameters.AddWithValue("@bmi", data.Bmi);
            cmd.Parameters.AddWithValue("@cal", data.CalorieNeeds);
            cmd.Parameters.AddWithValue("@pro", data.ProteinNeeds);
            cmd.Parameters.AddWithValue("@carb", data.CarbNeeds);
            cmd.Parameters.AddWithValue("@fat", data.FatNeeds);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
