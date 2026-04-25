namespace TeamNut.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using TeamNut.Models;
    using TeamNut.Repositories.Interfaces;

    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(IDbConfig dbConfig)
        {
            connectionString = dbConfig.ConnectionString;
        }

        public async Task<User?> GetById(int id)
        {
            const string sql = "SELECT id, username, password, role FROM Users WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
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
                }
            }

            return null;
        }

        public async Task AddUserData(UserData data)
        {
            const string sql = @"
                INSERT INTO UserData (user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs)
                VALUES (@userId, @weight, @height, @age, @gender, @goal, @bmi, @calories, @protein, @carbohydrates, @fat)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@userId", data.UserId);
                cmd.Parameters.AddWithValue("@weight", data.Weight);
                cmd.Parameters.AddWithValue("@height", data.Height);
                cmd.Parameters.AddWithValue("@age", data.Age);
                cmd.Parameters.AddWithValue("@gender", data.Gender);
                cmd.Parameters.AddWithValue("@goal", data.Goal);
                cmd.Parameters.AddWithValue("@bmi", data.Bmi);
                cmd.Parameters.AddWithValue("@calories", data.CalorieNeeds);
                cmd.Parameters.AddWithValue("@protein", data.ProteinNeeds);
                cmd.Parameters.AddWithValue("@carbohydrates", data.CarbohydrateNeeds);
                cmd.Parameters.AddWithValue("@fat", data.FatNeeds);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task Add(User entity)
        {
            const string sql = @"
                INSERT INTO Users (username, password, role)
                VALUES (@username, @password, @role);
                SELECT last_insert_rowid();";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@username", entity.Username);
                cmd.Parameters.AddWithValue("@password", entity.Password);
                cmd.Parameters.AddWithValue("@role", entity.Role);

                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    entity.Id = Convert.ToInt32(result);
                }
            }
        }

        public async Task Update(User entity)
        {
            const string sql = "UPDATE Users SET username=@username, password=@password, role=@role WHERE id=@id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@username", entity.Username);
                cmd.Parameters.AddWithValue("@password", entity.Password);
                cmd.Parameters.AddWithValue("@role", entity.Role);
                cmd.Parameters.AddWithValue("@id", entity.Id);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            const string sql = "DELETE FROM Users WHERE id = @id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<User?> GetByUsernameAndPassword(string username, string password)
        {
            const string sql = "SELECT id, username, password, role FROM Users WHERE username = @username AND password = @password";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
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
                }
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not initialized.");
            }

            const string sql = "SELECT id, username, password, role FROM Users";
            var users = new List<User>();

            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqliteCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
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
                }
            }

            return users;
        }

        public async Task<UserData?> GetUserDataByUserId(int userId)
        {
            const string sql = @"
                SELECT id, user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs 
                FROM UserData 
                WHERE user_id = @userId";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
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
                            CarbohydrateNeeds = Convert.ToInt32(reader.GetValue(10)),
                            FatNeeds = Convert.ToInt32(reader.GetValue(11)),
                        };
                    }
                }
            }

            return null;
        }

        public async Task UpdateUserData(UserData data)
        {
            const string sql = @"
                UPDATE UserData
                SET weight = @weight, height = @height, age = @age, gender = @gender, goal = @goal,
                    bmi = @bmi, calorie_needs = @calories, protein_needs = @protein,
                    carb_needs = @carbohydrates, fat_needs = @fat
                WHERE user_id = @userId";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@userId", data.UserId);
                cmd.Parameters.AddWithValue("@weight", data.Weight);
                cmd.Parameters.AddWithValue("@height", data.Height);
                cmd.Parameters.AddWithValue("@age", data.Age);
                cmd.Parameters.AddWithValue("@gender", data.Gender);
                cmd.Parameters.AddWithValue("@goal", data.Goal);
                cmd.Parameters.AddWithValue("@bmi", data.Bmi);
                cmd.Parameters.AddWithValue("@calories", data.CalorieNeeds);
                cmd.Parameters.AddWithValue("@protein", data.ProteinNeeds);
                cmd.Parameters.AddWithValue("@carbohydrates", data.CarbohydrateNeeds);
                cmd.Parameters.AddWithValue("@fat", data.FatNeeds);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
