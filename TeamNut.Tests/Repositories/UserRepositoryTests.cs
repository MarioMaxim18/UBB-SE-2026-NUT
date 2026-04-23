using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Moq;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using Xunit;

namespace TeamNut.Repositories.UnitTests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task AddUserData_WithValidData_PersistsRow()
        {
            string connectionString = "Data Source=file:memdb_valid?mode=memory&cache=shared";
            using var keeper = new SqliteConnection(connectionString);
            keeper.Open();

            using (var cmd = keeper.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS UserData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER,
    weight INTEGER,
    height INTEGER,
    age INTEGER,
    gender TEXT,
    goal TEXT,
    bmi REAL,
    calorie_needs INTEGER,
    protein_needs INTEGER,
    carb_needs INTEGER,
    fat_needs INTEGER
);";
                cmd.ExecuteNonQuery();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var data = new UserData
            {
                UserId = 42,
                Weight = 75,
                Height = 180,
                Age = 30,
                Gender = "male",
                Goal = "maintenance",
                Bmi = 23.1,
                CalorieNeeds = 2500,
                ProteinNeeds = 150,
                CarbNeeds = 300,
                FatNeeds = 70
            };

            Func<Task> act = async () => await repo.AddUserData(data);
            await act.Should().NotThrowAsync();

            using (var verificationCmd = keeper.CreateCommand())
            {
                verificationCmd.CommandText = @"
SELECT user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs
FROM UserData
WHERE user_id = @uid
LIMIT 1;";
                var param = verificationCmd.CreateParameter();
                param.ParameterName = "@uid";
                param.Value = data.UserId;
                verificationCmd.Parameters.Add(param);

                using var reader = verificationCmd.ExecuteReader();
                reader.Read().Should().BeTrue();

                reader.GetInt32(0).Should().Be(data.UserId);
                reader.GetInt32(1).Should().Be(data.Weight);
                reader.GetInt32(2).Should().Be(data.Height);
                reader.GetInt32(3).Should().Be(data.Age);
                reader.GetString(4).Should().Be(data.Gender);
                reader.GetString(5).Should().Be(data.Goal);
                reader.GetDouble(6).Should().BeApproximately(data.Bmi, 0.0001);
                reader.GetInt32(7).Should().Be(data.CalorieNeeds);
                reader.GetInt32(8).Should().Be(data.ProteinNeeds);
                reader.GetInt32(9).Should().Be(data.CarbNeeds);
                reader.GetInt32(10).Should().Be(data.FatNeeds);
            }
        }

        [Fact]
        public async Task AddUserData_WhenTableMissing_ThrowsSqliteException()
        {
            string connectionString = "Data Source=file:memdb_missingtable?mode=memory&cache=shared";

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var data = new UserData
            {
                UserId = 1,
                Weight = 1,
                Height = 1,
                Age = 1,
                Gender = "female",
                Goal = "cut",
                Bmi = 18.5,
                CalorieNeeds = 1200,
                ProteinNeeds = 80,
                CarbNeeds = 100,
                FatNeeds = 30
            };

            Func<Task> act = async () => await repo.AddUserData(data);

            await act.Should().ThrowAsync<SqliteException>();
        }

        [Fact]
        public void Constructor_NullDbConfig_ThrowsNullReferenceException()
        {
            IDbConfig? nullConfig = null;

            Action act = () => new UserRepository(nullConfig!);

            act.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public async Task Constructor_NullConnectionString_GetAll_ThrowsInvalidOperationException()
        {
            var mockConfig = new Mock<IDbConfig>();
            mockConfig.SetupGet(m => m.ConnectionString).Returns((string?)null);
            var repo = new UserRepository(mockConfig.Object);

            Func<Task> act = async () => await repo.GetAll();

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Connection string is not initialized.");
        }

        [Fact]
        public async Task Constructor_WithValidSharedInMemoryConnectionString_GetAll_ReturnsInsertedUser()
        {
            var connectionString = "Data Source=file:memdb_unittest?mode=memory&cache=shared";
            using var keepAlive = new SqliteConnection(connectionString);
            keepAlive.Open();

            using (var createCmd = keepAlive.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT,
                        password TEXT,
                        role TEXT
                    );
                    INSERT INTO Users (username, password, role) VALUES ('alice', 'secret', 'admin');
                ";
                createCmd.ExecuteNonQuery();
            }

            var mockConfig = new Mock<IDbConfig>();
            mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(mockConfig.Object);

            var users = await repo.GetAll();

            users.Should().NotBeNull();
            users.Should().ContainSingle(u =>
                u.Username == "alice" &&
                u.Password == "secret" &&
                u.Role == "admin");
        }

        [Fact]
        public async Task UpdateUserData_ValidData_UpdatesRow()
        {
            string connectionString = "Data Source=TestDb_Update_Valid;Mode=Memory;Cache=Shared";
            using var keeper = new SqliteConnection(connectionString);
            keeper.Open();

            await CreateUserDataTableAsync(connectionString);

            const int userId = 1;
            await InsertUserDataRowAsync(connectionString, userId,
                weight: 50, height: 160, age: 25, gender: "female", goal: "maintenance",
                bmi: 20.0, cal: 1800, pro: 80, carb: 200, fat: 60);

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var updated = new UserData
            {
                UserId = userId,
                Weight = 70,
                Height = 170,
                Age = 30,
                Gender = "male",
                Goal = "bulk",
                Bmi = 23.5,
                CalorieNeeds = 2800,
                ProteinNeeds = 120,
                CarbNeeds = 350,
                FatNeeds = 80
            };

            Func<Task> act = async () => await repo.UpdateUserData(updated);
            await act.Should().NotThrowAsync();

            using var verifyConn = new SqliteConnection(connectionString);
            await verifyConn.OpenAsync();
            using var cmd = new SqliteCommand("SELECT weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs FROM UserData WHERE user_id = @userId", verifyConn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read().Should().BeTrue();

            reader.GetInt32(0).Should().Be(updated.Weight);
            reader.GetInt32(1).Should().Be(updated.Height);
            reader.GetInt32(2).Should().Be(updated.Age);
            reader.GetString(3).Should().Be(updated.Gender);
            reader.GetString(4).Should().Be(updated.Goal);
            reader.GetDouble(5).Should().BeApproximately(updated.Bmi, 0.0001);
            reader.GetInt32(6).Should().Be(updated.CalorieNeeds);
            reader.GetInt32(7).Should().Be(updated.ProteinNeeds);
            reader.GetInt32(8).Should().Be(updated.CarbNeeds);
            reader.GetInt32(9).Should().Be(updated.FatNeeds);
        }

        [Theory]
        [MemberData(nameof(EdgeCasesData))]
        public async Task UpdateUserData_EdgeValues_StoresValues(UserData input)
        {
            string connectionString = $"Data Source=TestDb_Update_Edge_{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            using var keeper = new SqliteConnection(connectionString);
            keeper.Open();

            await CreateUserDataTableAsync(connectionString);

            int userId = input.UserId;
            await InsertUserDataRowAsync(connectionString, userId,
                weight: 1, height: 1, age: 1, gender: "init", goal: "maintenance",
                bmi: 1.0, cal: 1, pro: 1, carb: 1, fat: 1);

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repo.UpdateUserData(input);
            await act.Should().NotThrowAsync();

            using var verifyConn = new SqliteConnection(connectionString);
            await verifyConn.OpenAsync();
            using var cmd = new SqliteCommand("SELECT weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs FROM UserData WHERE user_id = @userId", verifyConn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read().Should().BeTrue();

            reader.GetInt32(0).Should().Be(input.Weight);
            reader.GetInt32(1).Should().Be(input.Height);
            reader.GetInt32(2).Should().Be(input.Age);
            reader.GetString(3).Should().Be(input.Gender);
            reader.GetString(4).Should().Be(input.Goal);
            reader.GetDouble(5).Should().BeApproximately(input.Bmi, 0.0001);
            reader.GetInt32(6).Should().Be(input.CalorieNeeds);
            reader.GetInt32(7).Should().Be(input.ProteinNeeds);
            reader.GetInt32(8).Should().Be(input.CarbNeeds);
            reader.GetInt32(9).Should().Be(input.FatNeeds);
        }

        [Fact]
        public async Task UpdateUserData_NonExistingRow_DoesNotThrowAndDoesNotInsert()
        {
            string connectionString = "Data Source=TestDb_Update_NoRow;Mode=Memory;Cache=Shared";
            using var keeper = new SqliteConnection(connectionString);
            keeper.Open();

            await CreateUserDataTableAsync(connectionString);

            int userId = 9999;
            var data = new UserData
            {
                UserId = userId,
                Weight = 10,
                Height = 10,
                Age = 20,
                Gender = "male",
                Goal = "cut",
                Bmi = 18.5,
                CalorieNeeds = 1500,
                ProteinNeeds = 50,
                CarbNeeds = 150,
                FatNeeds = 40
            };

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repo.UpdateUserData(data);
            await act.Should().NotThrowAsync();

            using var verifyConn = new SqliteConnection(connectionString);
            await verifyConn.OpenAsync();
            using var countCmd = new SqliteCommand("SELECT COUNT(1) FROM UserData WHERE user_id = @userId", verifyConn);
            countCmd.Parameters.AddWithValue("@userId", userId);
            var countObj = await countCmd.ExecuteScalarAsync();
            Convert.ToInt32(countObj).Should().Be(0);
        }

        private static async Task CreateUserDataTableAsync(string connectionString)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserData (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER,
                    weight INTEGER,
                    height INTEGER,
                    age INTEGER,
                    gender TEXT,
                    goal TEXT,
                    bmi REAL,
                    calorie_needs INTEGER,
                    protein_needs INTEGER,
                    carb_needs INTEGER,
                    fat_needs INTEGER
                );";
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertUserDataRowAsync(string connectionString, int userId, int weight, int height, int age, string gender, string goal, double bmi, int cal, int pro, int carb, int fat)
        {
            using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO UserData (user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs)
                VALUES (@userId, @w, @h, @a, @gen, @goal, @bmi, @cal, @pro, @carb, @fat);";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@w", weight);
            cmd.Parameters.AddWithValue("@h", height);
            cmd.Parameters.AddWithValue("@a", age);
            cmd.Parameters.AddWithValue("@gen", gender);
            cmd.Parameters.AddWithValue("@goal", goal);
            cmd.Parameters.AddWithValue("@bmi", bmi);
            cmd.Parameters.AddWithValue("@cal", cal);
            cmd.Parameters.AddWithValue("@pro", pro);
            cmd.Parameters.AddWithValue("@carb", carb);
            cmd.Parameters.AddWithValue("@fat", fat);
            await cmd.ExecuteNonQueryAsync();
        }

        public static IEnumerable<object[]> EdgeCasesData()
        {
            yield return new object[] { new UserData { UserId = 10, Weight = 0, Height = 0, Age = 0, Gender = string.Empty, Goal = string.Empty, Bmi = 0.0, CalorieNeeds = 0, ProteinNeeds = 0, CarbNeeds = 0, FatNeeds = 0 } };

            yield return new object[] { new UserData { UserId = 11, Weight = int.MinValue, Height = int.MaxValue, Age = int.MinValue, Gender = "male\0\u0001", Goal = "maintenance", Bmi = double.MinValue == double.MinValue ? double.MinValue : 0.0, CalorieNeeds = int.MaxValue, ProteinNeeds = int.MinValue, CarbNeeds = int.MaxValue, FatNeeds = int.MinValue } };

            yield return new object[] { new UserData { UserId = 12, Weight = 1, Height = 300, Age = 130, Gender = "female", Goal = "well-being", Bmi = 9999.9, CalorieNeeds = 250000, ProteinNeeds = 10000, CarbNeeds = 200000, FatNeeds = 50000 } };

            yield return new object[] { new UserData { UserId = 13, Weight = -1, Height = -100, Age = -25, Gender = "  ", Goal = "cut", Bmi = -5.5, CalorieNeeds = -100, ProteinNeeds = -10, CarbNeeds = -20, FatNeeds = -5 } };
        }

        [Fact]
        public async Task Delete_ExistingId_RemovesRow()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.db");
            string connectionString = $"Data Source={dbPath}";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY, username TEXT);";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Users(id, username) VALUES (@id, @username);";
                        cmd.Parameters.AddWithValue("@id", 42);
                        cmd.Parameters.AddWithValue("@username", "toDelete");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

                var repo = new UserRepository(dbConfigMock.Object);

                Func<Task> act = async () => await repo.Delete(42);
                await act.Should().NotThrowAsync();

                using (var conn = new SqliteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE id = @id;";
                        cmd.Parameters.AddWithValue("@id", 42);
                        var result = (long)await cmd.ExecuteScalarAsync();
                        result.Should().Be(0);
                    }
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                }
                catch
                {
                }
            }
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task Delete_NonExistingIds_NoThrowAndNoChange(int testId)
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.db");
            string connectionString = $"Data Source={dbPath}";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY, username TEXT);";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Users(id, username) VALUES (@id, @username);";
                        cmd.Parameters.AddWithValue("@id", 1);
                        cmd.Parameters.AddWithValue("@username", "exists");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

                var repo = new UserRepository(dbConfigMock.Object);

                Func<Task> act = async () => await repo.Delete(testId);
                await act.Should().NotThrowAsync();

                using (var conn = new SqliteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE id = @id;";
                        cmd.Parameters.AddWithValue("@id", 1);
                        var result = (long)await cmd.ExecuteScalarAsync();
                        result.Should().Be(1);
                    }
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                }
                catch
                {
                }
            }
        }

        [Fact]
        public async Task GetByUsernameAndPassword_ValidCredentials_ReturnsUser()
        {
            string connectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";
            using (var master = new SqliteConnection(connectionString))
            {
                await master.OpenAsync();

                using (var createCmd = master.CreateCommand())
                {
                    createCmd.CommandText =
                        @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = master.CreateCommand())
                {
                    insertCmd.CommandText =
                        "INSERT INTO Users (id, username, password, role) VALUES (1, 'jdoe', 'secret', 'Admin');";
                    await insertCmd.ExecuteNonQueryAsync();
                }

                var mockDbConfig = new Mock<IDbConfig>(MockBehavior.Strict);
                mockDbConfig.SetupGet(x => x.ConnectionString).Returns(connectionString);

                var repo = new UserRepository(mockDbConfig.Object);

                User? result = await repo.GetByUsernameAndPassword("jdoe", "secret");

                result.Should().NotBeNull();
                result!.Id.Should().Be(1);
                result.Username.Should().Be("jdoe");
                result.Password.Should().Be("secret");
                result.Role.Should().Be("Admin");
            }
        }

        [Theory]
        [MemberData(nameof(InvalidCredentialsData))]
        public async Task GetByUsernameAndPassword_InvalidOrEdgeCredentials_ReturnsNull(string username, string password)
        {
            string connectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";
            using (var master = new SqliteConnection(connectionString))
            {
                await master.OpenAsync();

                using (var createCmd = master.CreateCommand())
                {
                    createCmd.CommandText =
                        @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = master.CreateCommand())
                {
                    insertCmd.CommandText =
                        "INSERT INTO Users (id, username, password, role) VALUES (1, 'jdoe', 'secret', 'Admin');";
                    await insertCmd.ExecuteNonQueryAsync();
                }

                var mockDbConfig = new Mock<IDbConfig>(MockBehavior.Strict);
                mockDbConfig.SetupGet(x => x.ConnectionString).Returns(connectionString);

                var repo = new UserRepository(mockDbConfig.Object);

                User? result = await repo.GetByUsernameAndPassword(username, password);

                result.Should().BeNull();
            }
        }

        public static IEnumerable<object[]> InvalidCredentialsData()
        {
            yield return new object[] { "jdoe", "wrong" };
            yield return new object[] { "", "secret" };
            yield return new object[] { "   ", "secret" };
            yield return new object[] { new string('a', 1000), "secret" };
            yield return new object[] { "jdoe", "' OR '1'='1" };
            yield return new object[] { "nonexistent", "nop" };
        }

        public static IEnumerable<object[]> GetValidUsers()
        {
            yield return new object[] { "user1", "password1", "User", 0 };
            yield return new object[] { string.Empty, string.Empty, string.Empty, int.MinValue };
            yield return new object[] { new string('a', 300), new string('p', 300), "Nutritionist", int.MaxValue };
        }

        [Theory]
        [MemberData(nameof(GetValidUsers))]
        public async Task Add_WithVariousUserInputs_AssignsIdAndOverridesInitialId(string username, string password, string role, int initialId)
        {
            string memName = Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{memName}?mode=memory&cache=shared";
            var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            using (var createCmd = keeper.CreateCommand())
            {
                createCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            username TEXT,
                                            password TEXT,
                                            role TEXT
                                          );";
                createCmd.ExecuteNonQuery();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var user = new User
            {
                Id = initialId,
                Username = username,
                Password = password,
                Role = role
            };

            try
            {
                await repo.Add(user);

                user.Id.Should().BeGreaterThan(0);
                user.Id.Should().NotBe(initialId);

                using var verifyCmd = keeper.CreateCommand();
                verifyCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE id = @id AND username = @u";
                verifyCmd.Parameters.AddWithValue("@id", user.Id);
                verifyCmd.Parameters.AddWithValue("@u", username);
                var count = Convert.ToInt32(verifyCmd.ExecuteScalar() ?? 0);
                count.Should().Be(1);
            }
            finally
            {
                await keeper.CloseAsync();
                keeper.Dispose();
            }
        }

        [Fact]
        public async Task Add_WhenUsersTableMissing_ThrowsSqliteException()
        {
            string memName = Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{memName}?mode=memory&cache=shared";

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var user = new User { Id = 0, Username = "willfail", Password = "pw", Role = "User" };

            await FluentActions.Awaiting(() => repo.Add(user)).Should().ThrowAsync<SqliteException>();
        }

        [Fact]
        public async Task Update_ExistingUser_UpdatesDatabaseRow()
        {
            var connectionString = "Data Source=UpdateTest1;Mode=Memory;Cache=Shared";
            using var master = new SqliteConnection(connectionString);
            master.Open();

            using (var createCmd = master.CreateCommand())
            {
                createCmd.CommandText = @"CREATE TABLE Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT,
                        password TEXT,
                        role TEXT
                    );";
                createCmd.ExecuteNonQuery();
            }

            long insertedId;
            using (var insertCmd = master.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO Users (username, password, role) VALUES (@u, @p, @r);";
                insertCmd.Parameters.AddWithValue("@u", "oldUser");
                insertCmd.Parameters.AddWithValue("@p", "oldPass");
                insertCmd.Parameters.AddWithValue("@r", "user");
                insertCmd.ExecuteNonQuery();

                insertCmd.CommandText = "SELECT last_insert_rowid();";
                insertedId = (long)insertCmd.ExecuteScalar();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var entity = new User { Id = Convert.ToInt32(insertedId), Username = "newUser", Password = "newPass", Role = "admin" };

            await repo.Update(entity);

            using (var verifyCmd = master.CreateCommand())
            {
                verifyCmd.CommandText = "SELECT username, password, role FROM Users WHERE id = @id";
                verifyCmd.Parameters.AddWithValue("@id", insertedId);
                using var reader = verifyCmd.ExecuteReader();
                reader.Read().Should().BeTrue();

                var actualUsername = reader.GetString(0);
                var actualPassword = reader.GetString(1);
                var actualRole = reader.GetString(2);

                actualUsername.Should().Be(entity.Username);
                actualPassword.Should().Be(entity.Password);
                actualRole.Should().Be(entity.Role);
            }
        }

        [Theory]
        [MemberData(nameof(UpdateEdgeCases))]
        public async Task Update_VariousEdgeCaseInputs_DoesNotThrow(int id, string username, string password, string role)
        {
            var connectionString = "Data Source=SharedEdge;Mode=Memory;Cache=Shared";
            using var master = new SqliteConnection(connectionString);
            master.Open();

            using (var createCmd = master.CreateCommand())
            {
                createCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT,
                        password TEXT,
                        role TEXT
                    );";
                createCmd.ExecuteNonQuery();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(dbConfigMock.Object);

            var entity = new User { Id = id, Username = username, Password = password, Role = role };

            Func<Task> act = async () => await repo.Update(entity);

            await act.Should().NotThrowAsync();
        }

        public static IEnumerable<object[]> UpdateEdgeCases()
        {
            yield return new object[] { int.MinValue, string.Empty, "pw", "role" };
            yield return new object[] { int.MaxValue, " ", "pw", "role" };
            yield return new object[] { 0, "user0", "p0", "r0" };
            yield return new object[] { -1, "negUser", "negPass", "negRole" };
            yield return new object[] { 1, new string('a', 10000), new string('p', 8000), new string('r', 2000) };
            yield return new object[] { 2, "user\n\t\r\u0000", "p\0assword", "role!@#$%^&*()" };
        }

        [Theory]
        [InlineData("")]
        [MemberData(nameof(GetNullConnectionStringData))]
        public async Task GetAll_ConnectionStringNullOrEmpty_ThrowsInvalidOperationException(string? connectionString)
        {
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);
            var repo = new UserRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repo.GetAll();

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Connection string is not initialized.");
        }

        public static IEnumerable<object?[]> GetNullConnectionStringData()
        {
            yield return new object?[] { null };
        }

        [Fact]
        public async Task GetAll_NoRows_ReturnsEmptyCollection()
        {
            string connectionString = "Data Source=SharedDb_NoRows;Mode=Memory;Cache=Shared";
            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                using (var createCmd = keeper.CreateCommand())
                {
                    createCmd.CommandText = @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);
                var repo = new UserRepository(dbConfigMock.Object);

                var result = await repo.GetAll();

                result.Should().NotBeNull().And.BeEmpty();
            }
        }

        [Fact]
        public async Task GetAll_WithRows_MapsRowsToUsers()
        {
            string connectionString = "Data Source=SharedDb_WithRows;Mode=Memory;Cache=Shared";
            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                using (var createCmd = keeper.CreateCommand())
                {
                    createCmd.CommandText = @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = keeper.CreateCommand())
                {
                    insertCmd.CommandText = @"INSERT INTO Users (id, username, password, role) VALUES
                        (1, @u1, @p1, @r1),
                        (2, @u2, @p2, @r2);";
                    insertCmd.Parameters.AddWithValue("@u1", "alice");
                    insertCmd.Parameters.AddWithValue("@p1", "alice_pw");
                    insertCmd.Parameters.AddWithValue("@r1", "admin");
                    insertCmd.Parameters.AddWithValue("@u2", "bob\u0000\u2603");
                    insertCmd.Parameters.AddWithValue("@p2", "b0b_pw");
                    insertCmd.Parameters.AddWithValue("@r2", "user");
                    await insertCmd.ExecuteNonQueryAsync();
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);
                var repo = new UserRepository(dbConfigMock.Object);

                var result = await repo.GetAll();

                result.Should().HaveCount(2);
                result.Should().Contain(u => u.Id == 1 && u.Username == "alice" && u.Password == "alice_pw" && u.Role == "admin");
                result.Should().Contain(u => u.Id == 2 && u.Username == "bob\u0000\u2603" && u.Password == "b0b_pw" && u.Role == "user");
            }
        }

        [Fact]
        public async Task GetAll_IdTooLarge_ThrowsOverflowException()
        {
            string connectionString = "Data Source=SharedDb_OverflowId;Mode=Memory;Cache=Shared";
            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                using (var createCmd = keeper.CreateCommand())
                {
                    createCmd.CommandText = @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = keeper.CreateCommand())
                {
                    insertCmd.CommandText = @"INSERT INTO Users (id, username, password, role) VALUES
                        (2147483648, 'bigid', 'pw', 'role');";
                    await insertCmd.ExecuteNonQueryAsync();
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);
                var repo = new UserRepository(dbConfigMock.Object);

                Func<Task> act = async () => await repo.GetAll();

                await act.Should().ThrowAsync<OverflowException>();
            }
        }

        [Fact]
        public async Task GetAll_NullTextColumn_ThrowsInvalidOperationException()
        {
            string connectionString = "Data Source=SharedDb_NullText;Mode=Memory;Cache=Shared";
            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                using (var createCmd = keeper.CreateCommand())
                {
                    createCmd.CommandText = @"CREATE TABLE Users (
                            id INTEGER PRIMARY KEY,
                            username TEXT,
                            password TEXT,
                            role TEXT
                        );";
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = keeper.CreateCommand())
                {
                    insertCmd.CommandText = @"INSERT INTO Users (id, username, password, role) VALUES
                        (1, NULL, 'pw', 'role');";
                    await insertCmd.ExecuteNonQueryAsync();
                }

                var dbConfigMock = new Mock<IDbConfig>();
                dbConfigMock.Setup(c => c.ConnectionString).Returns(connectionString);
                var repo = new UserRepository(dbConfigMock.Object);

                Func<Task> act = async () => await repo.GetAll();

                await act.Should().ThrowAsync<InvalidOperationException>();
            }
        }

        public static IEnumerable<object[]> UserRecords()
        {
            yield return new object[] { 0, "normalUser", "p@ssw0rd", "admin" };
            yield return new object[] { int.MinValue, "user\nnewline", "", " " };
            yield return new object[] { int.MaxValue, new string('x', 1001), "pässwörd", "role-with-special-©" };
            yield return new object[] { -42, "", "\u0001control", "user" };
        }

        [Theory]
        [MemberData(nameof(UserRecords))]
        public async Task GetById_IdExists_ReturnsUser(int id, string username, string password, string role)
        {
            string dbName = Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared;URI=true";

            await using var keepAlive = new SqliteConnection(connectionString);
            await keepAlive.OpenAsync();

            await using (var createCmd = keepAlive.CreateCommand())
            {
                createCmd.CommandText = "CREATE TABLE Users (id INTEGER PRIMARY KEY, username TEXT, password TEXT, role TEXT);";
                await createCmd.ExecuteNonQueryAsync();
            }

            await using (var insertCmd = keepAlive.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO Users (id, username, password, role) VALUES (@id, @username, @password, @role);";
                insertCmd.Parameters.AddWithValue("@id", id);
                insertCmd.Parameters.AddWithValue("@username", username ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@password", password ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@role", role ?? (object)DBNull.Value);
                await insertCmd.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repository = new UserRepository(dbConfigMock.Object);

            var result = await repository.GetById(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Username.Should().Be(username);
            result.Password.Should().Be(password);
            result.Role.Should().Be(role);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task GetById_IdDoesNotExist_ReturnsNull(int id)
        {
            string dbName = Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared;URI=true";

            await using var keepAlive = new SqliteConnection(connectionString);
            await keepAlive.OpenAsync();

            await using (var createCmd = keepAlive.CreateCommand())
            {
                createCmd.CommandText = "CREATE TABLE Users (id INTEGER PRIMARY KEY, username TEXT, password TEXT, role TEXT);";
                await createCmd.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repository = new UserRepository(dbConfigMock.Object);

            var result = await repository.GetById(id);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public async Task GetUserDataByUserId_UserExists_ReturnsMappedUserData(int userId)
        {
            var dbName = $"memdb_{Guid.NewGuid():N}";
            var connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";

            await using var master = new SqliteConnection(connectionString);
            await master.OpenAsync();

            var createSql = @"
                CREATE TABLE UserData (
                    id INTEGER PRIMARY KEY,
                    user_id INTEGER,
                    weight INTEGER,
                    height INTEGER,
                    age INTEGER,
                    gender TEXT,
                    goal TEXT,
                    bmi INTEGER,
                    calorie_needs INTEGER,
                    protein_needs INTEGER,
                    carb_needs INTEGER,
                    fat_needs INTEGER
                );";
            await using (var createCmd = master.CreateCommand())
            {
                createCmd.CommandText = createSql;
                await createCmd.ExecuteNonQueryAsync();
            }

            var insertedId = 12345;
            var weight = 72;
            var height = 180;
            var age = 30;
            var gender = "NonBinary";
            var goal = "Maintain";
            var bmi = 22;
            var calories = 2500;
            var protein = 150;
            var carbs = 300;
            var fat = 70;

            var insertSql = @"
                INSERT INTO UserData (
                    id, user_id, weight, height, age, gender, goal, bmi, calorie_needs, protein_needs, carb_needs, fat_needs
                ) VALUES (
                    @id, @uid, @w, @h, @a, @gen, @goal, @bmi, @cal, @pro, @carb, @fat
                );";
            await using (var insertCmd = master.CreateCommand())
            {
                insertCmd.CommandText = insertSql;
                insertCmd.Parameters.AddWithValue("@id", insertedId);
                insertCmd.Parameters.AddWithValue("@uid", userId);
                insertCmd.Parameters.AddWithValue("@w", weight);
                insertCmd.Parameters.AddWithValue("@h", height);
                insertCmd.Parameters.AddWithValue("@a", age);
                insertCmd.Parameters.AddWithValue("@gen", gender);
                insertCmd.Parameters.AddWithValue("@goal", goal);
                insertCmd.Parameters.AddWithValue("@bmi", bmi);
                insertCmd.Parameters.AddWithValue("@cal", calories);
                insertCmd.Parameters.AddWithValue("@pro", protein);
                insertCmd.Parameters.AddWithValue("@carb", carbs);
                insertCmd.Parameters.AddWithValue("@fat", fat);

                await insertCmd.ExecuteNonQueryAsync();
            }

            var mockConfig = new Mock<IDbConfig>();
            mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(mockConfig.Object);

            var result = await repo.GetUserDataByUserId(userId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(insertedId);
            result.UserId.Should().Be(userId);
            result.Weight.Should().Be(weight);
            result.Height.Should().Be(height);
            result.Age.Should().Be(age);
            result.Gender.Should().Be(gender);
            result.Goal.Should().Be(goal);
            result.Bmi.Should().Be(bmi);
            result.CalorieNeeds.Should().Be(calories);
            result.ProteinNeeds.Should().Be(protein);
            result.CarbNeeds.Should().Be(carbs);
            result.FatNeeds.Should().Be(fat);
        }

        [Fact]
        public async Task GetUserDataByUserId_UserDoesNotExist_ReturnsNull()
        {
            var dbName = $"memdb_{Guid.NewGuid():N}";
            var connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";

            await using var master = new SqliteConnection(connectionString);
            await master.OpenAsync();

            var createSql = @"
                CREATE TABLE UserData (
                    id INTEGER PRIMARY KEY,
                    user_id INTEGER,
                    weight INTEGER,
                    height INTEGER,
                    age INTEGER,
                    gender TEXT,
                    goal TEXT,
                    bmi INTEGER,
                    calorie_needs INTEGER,
                    protein_needs INTEGER,
                    carb_needs INTEGER,
                    fat_needs INTEGER
                );";
            await using (var createCmd = master.CreateCommand())
            {
                createCmd.CommandText = createSql;
                await createCmd.ExecuteNonQueryAsync();
            }

            var mockConfig = new Mock<IDbConfig>();
            mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new UserRepository(mockConfig.Object);

            var result = await repo.GetUserDataByUserId(42);

            result.Should().BeNull();
        }
    }
}
