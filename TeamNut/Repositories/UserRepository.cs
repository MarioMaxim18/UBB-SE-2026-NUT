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
                VALUES (@userId, @w, @h, @a, @gen, @goal, @bmi, @cal, @pro, @carb, @fat)";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
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

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task Add(User entity)
        {
            const string sql = @"
                INSERT INTO Users (username, password, role) 
                VALUES (@u, @p, @r); 
                SELECT last_insert_rowid();";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@u", entity.Username);
                cmd.Parameters.AddWithValue("@p", entity.Password);
                cmd.Parameters.AddWithValue("@r", entity.Role);

                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    entity.Id = Convert.ToInt32(result);
                }
            }
        }

        public async Task Update(User entity)
        {
            const string sql = "UPDATE Users SET username=@u, password=@p, role=@r WHERE id=@id";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@u", entity.Username);
                cmd.Parameters.AddWithValue("@p", entity.Password);
                cmd.Parameters.AddWithValue("@r", entity.Role);
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
            const string sql = "SELECT id, username, password, role FROM Users WHERE username = @u AND password = @p";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

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
                            CarbNeeds = Convert.ToInt32(reader.GetValue(10)),
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
                SET weight = @w, height = @h, age = @a, gender = @gen, goal = @goal,
                    bmi = @bmi, calorie_needs = @cal, protein_needs = @pro,
                    carb_needs = @carb, fat_needs = @fat
                WHERE user_id = @userId";

            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();

            using (var cmd = new SqliteCommand(sql, conn))
            {
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

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
