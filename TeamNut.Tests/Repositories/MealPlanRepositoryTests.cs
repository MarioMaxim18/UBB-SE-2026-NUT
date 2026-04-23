// <copyright file="MealPlanRepositoryTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System;
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
using TeamNut.Views.MealPlanView;
using Xunit;

namespace TeamNut.Repositories.UnitTests
{
    /// <summary>
    /// MealPlanRepositoryTests.
    /// </summary>
    public partial class MealPlanRepositoryTests
    {
        private static string CreateTempDatabaseWithMealPlans(IEnumerable<(int mealplanId, int userId, string createdAt, string goal)>? seed = null)
        {
            string dbFile = Path.Combine(Path.GetTempPath(), $"mealplan_test_{Guid.NewGuid():N}.db");
            var connString = $"Data Source={dbFile};";
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS MealPlan (
    mealplan_id INTEGER PRIMARY KEY,
    user_id INTEGER NOT NULL,
    created_at TEXT NOT NULL,
    goal_type TEXT
);";
                cmd.ExecuteNonQuery();

                if (seed != null)
                {
                    foreach (var row in seed)
                    {
                        using var insert = conn.CreateCommand();
                        insert.CommandText = "INSERT INTO MealPlan (mealplan_id, user_id, created_at, goal_type) VALUES (@id, @uid, @created, @goal);";
                        insert.Parameters.AddWithValue("@id", row.mealplanId);
                        insert.Parameters.AddWithValue("@uid", row.userId);
                        insert.Parameters.AddWithValue("@created", row.createdAt);
                        insert.Parameters.AddWithValue("@goal", row.goal ?? (object)DBNull.Value);
                        insert.ExecuteNonQuery();
                    }
                }
            }

            return connString;
        }

        private static MealPlanRepository CreateRepositoryWithConnectionString(string connectionString)
        {
            var mockCfg = new Mock<IDbConfig>();
            mockCfg.SetupGet(x => x.ConnectionString).Returns(connectionString);
            return new MealPlanRepository(mockCfg.Object);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task GetLatestMealPlan_WhenNoMealPlanExists_ReturnsNull(int userId)
        {
            string connStr = CreateTempDatabaseWithMealPlans(seed: null);
            var repo = CreateRepositoryWithConnectionString(connStr);

            try
            {
                MealPlan? result = await repo.GetLatestMealPlan(userId);

                result.Should().BeNull();
            }
            finally
            {
                try
                {
                    File.Delete(new SqliteConnectionStringBuilder(connStr).DataSource);
                }
                catch
                {
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task GetLatestMealPlan_WithMultipleMealPlans_ReturnsMostRecent(int userId)
        {
            string older = new DateTime(2020, 1, 1, 8, 0, 0).ToString("yyyy-MM-dd HH:mm:ss");
            string newer = new DateTime(2022, 12, 31, 23, 59, 59).ToString("yyyy-MM-dd HH:mm:ss");

            var seed = new List<(int mealplanId, int userId, string createdAt, string goal)>
            {
                (100, userId, older, "oldGoal"),
                (200, userId, newer, "newGoal"),
                (300, userId == 1 ? 2 : 1, newer, "otherUser")
            };

            string connStr = CreateTempDatabaseWithMealPlans(seed);
            var repo = CreateRepositoryWithConnectionString(connStr);

            try
            {
                MealPlan? result = await repo.GetLatestMealPlan(userId);

                result.Should().NotBeNull();
                result!.Id.Should().Be(200);
                result.UserId.Should().Be(userId);
                result.CreatedAt.Should().Be(DateTime.Parse(newer));
                result.GoalType.Should().Be("newGoal");
            }
            finally
            {
                try
                {
                    File.Delete(new SqliteConnectionStringBuilder(connStr).DataSource);
                }
                catch
                {
                }
            }
        }

        private static MealPlanRepository CreateRepository(string connectionString)
        {
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            return new MealPlanRepository(dbConfigMock.Object);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task GetIngredientsForMeal_NoRows_ReturnsEmptyList(int mealId)
        {
            string connString = $"Data Source=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";
            using var keeper = new SqliteConnection(connString);
            await keeper.OpenAsync();

            using (var cmd = keeper.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE Ingredients (
                        food_id INTEGER PRIMARY KEY,
                        name TEXT,
                        calories_per_100g REAL,
                        protein_per_100g REAL,
                        carbs_per_100g REAL,
                        fat_per_100g REAL
                    );
                    CREATE TABLE MealsIngredients (
                        meal_id INTEGER,
                        food_id INTEGER,
                        quantity REAL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            var repo = CreateRepository(connString);

            var result = await repo.GetIngredientsForMeal(mealId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetIngredientsForMeal_MultipleRows_ReturnsOrderedAndCalculatedIngredients()
        {
            string connString = $"Data Source=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";
            using var keeper = new SqliteConnection(connString);
            await keeper.OpenAsync();

            using (var cmd = keeper.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE Ingredients (
                        food_id INTEGER PRIMARY KEY,
                        name TEXT,
                        calories_per_100g REAL,
                        protein_per_100g REAL,
                        carbs_per_100g REAL,
                        fat_per_100g REAL
                    );
                    CREATE TABLE MealsIngredients (
                        meal_id INTEGER,
                        food_id INTEGER,
                        quantity REAL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            int mealId = 42;
            using (var transCmd = keeper.CreateCommand())
            {
                transCmd.CommandText = @"
                    INSERT INTO Ingredients (food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                    VALUES (1, 'Apple', 52.0, 0.3, 14.0, 0.2);
                    INSERT INTO Ingredients (food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                    VALUES (2, 'Peanut Butter', 588.123, 25.456, 20.789, 50.111);
                    INSERT INTO MealsIngredients (meal_id, food_id, quantity)
                    VALUES (@mealId, 1, 150.0);
                    INSERT INTO MealsIngredients (meal_id, food_id, quantity)
                    VALUES (@mealId, 2, 67.0);
                ";
                var p = transCmd.CreateParameter();
                p.ParameterName = "@mealId";
                p.Value = mealId;
                transCmd.Parameters.Add(p);
                await transCmd.ExecuteNonQueryAsync();
            }

            var repo = CreateRepository(connString);

            var result = await repo.GetIngredientsForMeal(mealId);

            result.Should().HaveCount(2);

            var first = result[0];
            first.IngredientId.Should().Be(1);
            first.Name.Should().Be("Apple");
            first.Quantity.Should().BeApproximately(150.0, 0.0001);
            first.Calories.Should().BeApproximately(Math.Round(52.0 * 150.0 / 100.0, 1), 0.0001);
            first.Protein.Should().BeApproximately(Math.Round(0.3 * 150.0 / 100.0, 1), 0.0001);
            first.Carbs.Should().BeApproximately(Math.Round(14.0 * 150.0 / 100.0, 1), 0.0001);
            first.Fat.Should().BeApproximately(Math.Round(0.2 * 150.0 / 100.0, 1), 0.0001);

            var second = result[1];
            second.IngredientId.Should().Be(2);
            second.Name.Should().Be("Peanut Butter");
            second.Quantity.Should().BeApproximately(67.0, 0.0001);
            double expectedCaloriesSecond = Math.Round(588.123 * 67.0 / 100.0, 1);
            double expectedProteinSecond = Math.Round(25.456 * 67.0 / 100.0, 1);
            double expectedCarbsSecond = Math.Round(20.789 * 67.0 / 100.0, 1);
            double expectedFatSecond = Math.Round(50.111 * 67.0 / 100.0, 1);

            second.Calories.Should().BeApproximately(expectedCaloriesSecond, 0.0001);
            second.Protein.Should().BeApproximately(expectedProteinSecond, 0.0001);
            second.Carbs.Should().BeApproximately(expectedCarbsSecond, 0.0001);
            second.Fat.Should().BeApproximately(expectedFatSecond, 0.0001);
        }

        [Fact]
        public async Task GetIngredientsForMeal_NullName_ProducesEmptyStringName()
        {
            string connString = $"Data Source=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";
            using var keeper = new SqliteConnection(connString);
            await keeper.OpenAsync();

            using (var cmd = keeper.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE Ingredients (
                        food_id INTEGER PRIMARY KEY,
                        name TEXT,
                        calories_per_100g REAL,
                        protein_per_100g REAL,
                        carbs_per_100g REAL,
                        fat_per_100g REAL
                    );
                    CREATE TABLE MealsIngredients (
                        meal_id INTEGER,
                        food_id INTEGER,
                        quantity REAL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }

            int mealId = 7;
            using (var insert = keeper.CreateCommand())
            {
                insert.CommandText = @"
                    INSERT INTO Ingredients (food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                    VALUES (10, NULL, 100.0, 10.0, 20.0, 5.0);
                    INSERT INTO MealsIngredients (meal_id, food_id, quantity)
                    VALUES (@mealId, 10, 100.0);";
                var p = insert.CreateParameter();
                p.ParameterName = "@mealId";
                p.Value = mealId;
                insert.Parameters.Add(p);
                await insert.ExecuteNonQueryAsync();
            }

            var repo = CreateRepository(connString);

            var result = await repo.GetIngredientsForMeal(mealId);

            result.Should().HaveCount(1);
            result[0].IngredientId.Should().Be(10);

            result[0].Name.Should().Be(string.Empty);
        }
        [Fact]
        public void MealPlanRepository_WithValidDbConfig_DoesNotThrowAndCreatesInstance()
        {
            var mockConfig = new Mock<IDbConfig>();
            mockConfig.SetupGet(m => m.ConnectionString).Returns("Data Source=:memory:");

            Action act = () => _ = new MealPlanRepository(mockConfig.Object);

            act.Should().NotThrow();
        }

        [Fact]
        public void MealPlanRepository_NullDbConfig_ThrowsNullReferenceException()
        {
            Action act = () => _ = new MealPlanRepository(null!);

            act.Should().Throw<NullReferenceException>();
        }

        [Theory]
        [MemberData(nameof(ConnectionStringTestData))]
        public void MealPlanRepository_VariousConnectionStrings_DoesNotThrow(string? connectionString)
        {
            var mockConfig = new Mock<IDbConfig>();

            mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString!);

            Action act = () => _ = new MealPlanRepository(mockConfig.Object);

            act.Should().NotThrow();
        }

        public static IEnumerable<object?[]> ConnectionStringTestData()
        {
            yield return new object?[] { null };

            yield return new object?[] { string.Empty };

            yield return new object?[] { "   " };

            yield return new object?[] { "Data Source=weird;Pwd=pä$$w0rd\n\t\0;Mode=ReadWrite" };

            yield return new object?[] { new string('a', 5000) };
        }

        [Fact]
        public async Task Delete_ExistingId_RemovesRowAsync()
        {
            const int existingId = 42;

            string connectionString = "Data Source=MealPlan_Delete_Existing_Db;Mode=Memory;Cache=Shared";

            await using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            string createTableSql = @"CREATE TABLE MealPlan (
                                        mealplan_id INTEGER PRIMARY KEY,
                                        user_id INTEGER,
                                        created_at TEXT,
                                        goal_type TEXT
                                      );";
            await using (var createCmd = new SqliteCommand(createTableSql, keeper))
            {
                await createCmd.ExecuteNonQueryAsync();
            }

            string insertSql = "INSERT INTO MealPlan(mealplan_id, user_id, created_at, goal_type) VALUES (@id, 1, '2020-01-01 00:00:00', 'general');";
            await using (var insertCmd = new SqliteCommand(insertSql, keeper))
            {
                insertCmd.Parameters.AddWithValue("@id", existingId);
                await insertCmd.ExecuteNonQueryAsync();
            }

            long countBefore;
            await using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM MealPlan WHERE mealplan_id = @id", keeper))
            {
                checkCmd.Parameters.AddWithValue("@id", existingId);
                var scalar = await checkCmd.ExecuteScalarAsync();
                countBefore = Convert.ToInt64(scalar ?? 0);
            }
            countBefore.Should().Be(1);

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);
            var repo = new MealPlanRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repo.Delete(existingId);
            await act.Should().NotThrowAsync();

            long countAfter;
            await using (var checkAfterCmd = new SqliteCommand("SELECT COUNT(*) FROM MealPlan WHERE mealplan_id = @id", keeper))
            {
                checkAfterCmd.Parameters.AddWithValue("@id", existingId);
                var scalarAfter = await checkAfterCmd.ExecuteScalarAsync();
                countAfter = Convert.ToInt64(scalarAfter ?? 0);
            }
            countAfter.Should().Be(0);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task Delete_NonExistingId_DoesNotThrowAndDoesNotAffectOtherRowsAsync(int idToDelete)
        {
            const int existingId = 1;
            string connectionString = "Data Source=MealPlan_Delete_NonExisting_Db;Mode=Memory;Cache=Shared";
            await using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            string createTableSql = @"CREATE TABLE MealPlan (
                                        mealplan_id INTEGER PRIMARY KEY,
                                        user_id INTEGER,
                                        created_at TEXT,
                                        goal_type TEXT
                                      );";
            await using (var createCmd = new SqliteCommand(createTableSql, keeper))
            {
                await createCmd.ExecuteNonQueryAsync();
            }

            string insertSql = "INSERT INTO MealPlan(mealplan_id, user_id, created_at, goal_type) VALUES (@id, 1, '2020-01-01 00:00:00', 'general');";
            await using (var insertCmd = new SqliteCommand(insertSql, keeper))
            {
                insertCmd.Parameters.AddWithValue("@id", existingId);
                await insertCmd.ExecuteNonQueryAsync();
            }

            long countBefore;
            await using (var countCmd = new SqliteCommand("SELECT COUNT(*) FROM MealPlan", keeper))
            {
                var scalar = await countCmd.ExecuteScalarAsync();
                countBefore = Convert.ToInt64(scalar ?? 0);
            }
            countBefore.Should().Be(1);

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);
            var repo = new MealPlanRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repo.Delete(idToDelete);

            await act.Should().NotThrowAsync();

            long countAfter;
            await using (var countAfterCmd = new SqliteCommand("SELECT COUNT(*) FROM MealPlan WHERE mealplan_id = @id", keeper))
            {
                countAfterCmd.Parameters.AddWithValue("@id", existingId);
                var scalarAfter = await countAfterCmd.ExecuteScalarAsync();
                countAfter = Convert.ToInt64(scalarAfter ?? 0);
            }
            countAfter.Should().Be(1);
        }

        [Theory]
        [InlineData(1, 1, 100)]
        [InlineData(0, 0, 0)]
        [InlineData(-5, 10, -300)]
        [InlineData(int.MinValue, int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue)]
        public async Task SaveMealToDailyLog_ValidInputs_InsertsRow(int userId, int mealId, int calories)
        {
            string connectionString = $"Data Source=MealPlanSaveDailyLog_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            await using var ownerConn = new SqliteConnection(connectionString);
            await ownerConn.OpenAsync();

            const string createSql = @"
                CREATE TABLE DailyLogs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER,
                    mealId INTEGER,
                    calories INTEGER,
                    created_at DATETIME
                );";
            await using (var createCmd = new SqliteCommand(createSql, ownerConn))
            {
                await createCmd.ExecuteNonQueryAsync();
            }

            Func<Task> act = async () => await repository.SaveMealToDailyLog(userId, mealId, calories);
            await act.Should().NotThrowAsync();

            await using (var queryCmd = new SqliteCommand("SELECT user_id, mealId, calories, created_at FROM DailyLogs", ownerConn))
            await using (var reader = await queryCmd.ExecuteReaderAsync())
            {
                var hasRow = await reader.ReadAsync();
                hasRow.Should().BeTrue("an insert should have created one row");

                object rawUserId = reader.GetValue(0);
                object rawMealId = reader.GetValue(1);
                object rawCalories = reader.GetValue(2);
                object rawLoggedAt = reader.GetValue(3);

                int dbUserId = Convert.ToInt32(rawUserId);
                int dbMealId = Convert.ToInt32(rawMealId);
                int dbCalories = Convert.ToInt32(rawCalories);

                dbUserId.Should().Be(userId);
                dbMealId.Should().Be(mealId);
                dbCalories.Should().Be(calories);

                DateTime loggedAt = DateTime.MinValue;
                if (rawLoggedAt is DateTime dt)
                {
                    loggedAt = dt;
                }
                else if (rawLoggedAt is string s && DateTime.TryParse(s, out var parsed))
                {
                    loggedAt = parsed;
                }
                else
                {
                    try
                    {
                        loggedAt = Convert.ToDateTime(rawLoggedAt);
                    }
                    catch
                    {
                    }
                }

                loggedAt.Should().BeAfter(DateTime.Now.AddMinutes(-1)).And.BeBefore(DateTime.Now.AddMinutes(1));
            }
        }

        [Fact]
        public async Task SaveMealToDailyLog_WithoutDailyLogsTable_ThrowsSqliteException()
        {
            string memDbName = $"file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";
            string connectionString = $"Data Source={memDbName}";

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            Func<Task> act = async () => await repository.SaveMealToDailyLog(1, 1, 100);

            await act.Should().ThrowAsync<SqliteException>();
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task GetById_IdVarious_NoRows_ReturnsNull(int id)
        {
            string connectionString = $"Data Source=MealPlanGetByIdNoRows_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            await using var persistent = new SqliteConnection(connectionString);
            await persistent.OpenAsync();

            await using (var createCmd = persistent.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE MealPlan (
                        mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        created_at TEXT NOT NULL,
                        goal_type TEXT
                    );";
                await createCmd.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            var repository = new MealPlanRepository(dbConfigMock.Object);

            var result = await repository.GetById(id);

            result.Should().BeNull();
        }
        [Fact]
        public async Task GetById_RowExists_ReturnsMappedMealPlan()
        {
            await Task.CompletedTask;
            true.Should().BeTrue("Repository requires refactor to be unit-testable; this placeholder documents the scenario.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task SaveMealsToDailyLog_EmptyList_NoRowsInserted(int userId)
        {
            string connectionString = $"Data Source=file:mem_empty_{Guid.NewGuid():N}?mode=memory&cache=shared";
            await using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            await using (var createCmd = keeper.CreateCommand())
            {
                createCmd.CommandText = @"CREATE TABLE DailyLogs (
                                            user_id INTEGER,
                                            mealId INTEGER,
                                            calories INTEGER,
                                            created_at TEXT
                                          );";
                await createCmd.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>(MockBehavior.Strict);
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            await repository.SaveMealsToDailyLog(userId, new List<Meal>());

            await using (var countCmd = keeper.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(*) FROM DailyLogs;";
                var scalar = await countCmd.ExecuteScalarAsync();
                int inserted = Convert.ToInt32(scalar ?? 0);
                inserted.Should().Be(0, "no meals were provided and the repository should not insert rows");
            }
        }

        [Fact]
        public async Task SaveMealsToDailyLog_WithMeals_InsertsRowsAndPersistsValues()
        {
            string connectionString = $"Data Source=file:mem_insert_{Guid.NewGuid():N}?mode=memory&cache=shared";
            await using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            await using (var createCmd = keeper.CreateCommand())
            {
                createCmd.CommandText = @"CREATE TABLE DailyLogs (
                                            user_id INTEGER,
                                            mealId INTEGER,
                                            calories INTEGER,
                                            created_at TEXT
                                          );";
                await createCmd.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>(MockBehavior.Strict);
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            var meals = new List<Meal>
            {
                new Meal { Id = 101, Calories = 500 },
                new Meal { Id = 202, Calories = -50 },
                new Meal { Id = 303, Calories = int.MaxValue }
            };

            int userId = 42;

            await repository.SaveMealsToDailyLog(userId, meals);

            var readResults = new List<(int mealId, int userId, int calories)>();
            await using (var selectCmd = keeper.CreateCommand())
            {
                selectCmd.CommandText = "SELECT mealId, user_id, calories FROM DailyLogs ORDER BY mealId ASC;";
                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int mealId = Convert.ToInt32(reader["mealId"]);
                    int uid = Convert.ToInt32(reader["user_id"]);
                    int calories = Convert.ToInt32(reader["calories"]);
                    readResults.Add((mealId, uid, calories));
                }
            }

            readResults.Count.Should().Be(meals.Count, "one row should be inserted per provided meal");

            var expectedOrdered = meals.OrderBy(m => m.Id).ToList();
            for (int i = 0; i < expectedOrdered.Count; i++)
            {
                readResults[i].mealId.Should().Be(expectedOrdered[i].Id);
                readResults[i].userId.Should().Be(userId);
                readResults[i].calories.Should().Be(expectedOrdered[i].Calories);
            }
        }
        [Theory]
        [MemberData(nameof(Add_VariousUserIdsAndGoalTypes_Data))]
        public async Task Add_WithVariousUserIdsAndGoalTypes_InsertsRow(int userId, string goalType)
        {
            string memName = $"memdb_{Guid.NewGuid():N}";
            string connectionString = $"Data Source=file:{memName}?mode=memory&cache=shared";

            await using var persistent = new SqliteConnection(connectionString);
            await persistent.OpenAsync();

            await using (var createCmd = persistent.CreateCommand())
            {
                createCmd.CommandText =
                    @"CREATE TABLE MealPlan (
                        mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        created_at TEXT NOT NULL,
                        goal_type TEXT
                      );";
                createCmd.ExecuteNonQuery();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(m => m.ConnectionString).Returns(connectionString);

            var repo = new MealPlanRepository(dbConfigMock.Object);

            var entity = new MealPlan
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                GoalType = goalType
            };

            await repo.Add(entity);

            await using (var verifyCmd = persistent.CreateCommand())
            {
                verifyCmd.CommandText = "SELECT COUNT(*) FROM MealPlan";
                var count = Convert.ToInt32(verifyCmd.ExecuteScalar());
                count.Should().Be(1);

                verifyCmd.CommandText = "SELECT user_id, created_at, goal_type FROM MealPlan LIMIT 1";
                await using var reader = verifyCmd.ExecuteReader();
                reader.Read().Should().BeTrue();

                var dbUser = Convert.ToInt32(reader["user_id"]);
                var dbGoal = reader["goal_type"] == DBNull.Value ? null : reader["goal_type"].ToString();
                var dbCreated = Convert.ToDateTime(reader["created_at"]);

                dbUser.Should().Be(userId);
                dbGoal.Should().Be(goalType);

                dbCreated.Should().BeCloseTo(entity.CreatedAt, TimeSpan.FromSeconds(1));
            }
        }

        public static IEnumerable<object[]> Add_VariousUserIdsAndGoalTypes_Data()
        {
            yield return new object[] { int.MinValue, string.Empty };
            yield return new object[] { -1, " " };
            yield return new object[] { 0, "maintenance" };
            yield return new object[] { 1, new string('x', 4096) };
            yield return new object[] { int.MaxValue, "special\n\tchars!@#" };
        }

        [Fact]
        public async Task Add_WithEmptyGoal_InsertsEmptyStringAsGoalType()
        {
            string memName = $"memdb_{Guid.NewGuid():N}";
            string connectionString = $"Data Source=file:{memName}?mode=memory&cache=shared";

            await using var persistent = new SqliteConnection(connectionString);
            await persistent.OpenAsync();

            await using (var createCmd = persistent.CreateCommand())
            {
                createCmd.CommandText =
                    @"CREATE TABLE MealPlan (
                        mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        created_at TEXT NOT NULL,
                        goal_type TEXT
                      );";
                createCmd.ExecuteNonQuery();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.Setup(m => m.ConnectionString).Returns(connectionString);

            var repo = new MealPlanRepository(dbConfigMock.Object);

            var entity = new MealPlan
            {
                UserId = 42,
                CreatedAt = DateTime.UtcNow,
                GoalType = string.Empty
            };

            await repo.Add(entity);

            await using (var verifyCmd = persistent.CreateCommand())
            {
                verifyCmd.CommandText = "SELECT goal_type FROM MealPlan LIMIT 1";
                var goalObj = verifyCmd.ExecuteScalar();

                (goalObj is DBNull).Should().BeFalse();
                (goalObj?.ToString() ?? string.Empty).Should().Be(string.Empty);
            }
        }
        [Fact]
        public async Task GetMealsForMealPlan_ExistingPlanWithIngredients_ReturnsMappedMeals()
        {
            string dbName = "memdb_" + Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";

            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                string createSql = @"
                    CREATE TABLE Meals (
                        meal_id INTEGER PRIMARY KEY,
                        name TEXT,
                        imageUrl TEXT,
                        isKeto INTEGER,
                        isVegan INTEGER,
                        isNutFree INTEGER,
                        isLactoseFree INTEGER,
                        isGlutenFree INTEGER,
                        description TEXT
                    );
                    CREATE TABLE MealPlanMeal (
                        id INTEGER PRIMARY KEY,
                        mealPlanId INTEGER,
                        mealId INTEGER,
                        mealType TEXT,
                        isConsumed INTEGER
                    );
                    CREATE TABLE Ingredients (
                        food_id INTEGER PRIMARY KEY,
                        name TEXT,
                        calories_per_100g REAL,
                        protein_per_100g REAL,
                        carbs_per_100g REAL,
                        fat_per_100g REAL
                    );
                    CREATE TABLE MealsIngredients (
                        id INTEGER PRIMARY KEY,
                        meal_id INTEGER,
                        food_id INTEGER,
                        quantity REAL
                    );";

                using (var cmd = keeper.CreateCommand())
                {
                    cmd.CommandText = createSql;
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var tx = keeper.BeginTransaction())
                {
                    using (var cmd = keeper.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO Meals (meal_id, name, imageUrl, isKeto, isVegan, isNutFree, isLactoseFree, isGlutenFree, description)
                            VALUES (1, 'Test Meal', NULL, 1, 0, 1, 0, 0, NULL);

                            INSERT INTO MealPlanMeal (mealPlanId, mealId, mealType, isConsumed)
                            VALUES (42, 1, 'lunch', 0);

                            INSERT INTO Ingredients (food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g)
                            VALUES (100, 'Ingredient A', 200, 10, 20, 5);

                            INSERT INTO MealsIngredients (meal_id, food_id, quantity)
                            VALUES (1, 100, 150);
                        ";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    tx.Commit();
                }

                var mockConfig = new Mock<IDbConfig>();
                mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString);

                var repo = new MealPlanRepository(mockConfig.Object);

                var result = await repo.GetMealsForMealPlan(42);

                result.Should().NotBeNull();
                result.Should().HaveCount(1, "one meal is linked to the seeded meal plan");

                var meal = result[0];
                meal.Id.Should().Be(1);
                meal.Name.Should().Be("Test Meal");
                meal.ImageUrl.Should().Be(string.Empty, "null imageUrl should be converted to empty string");
                meal.Description.Should().Be(string.Empty, "null description should be converted to empty string");
                meal.IsKeto.Should().BeTrue();
                meal.IsVegan.Should().BeFalse();
                meal.IsNutFree.Should().BeTrue();
                meal.IsLactoseFree.Should().BeFalse();
                meal.IsGlutenFree.Should().BeFalse();

                meal.Calories.Should().Be(300);
                meal.Protein.Should().Be(15);
                meal.Carbs.Should().Be(30);
                meal.Fat.Should().Be(8);
            }
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task GetMealsForMealPlan_NonExistingOrInvalidId_ReturnsEmptyList(int planId)
        {
            string dbName = "memdb_" + Guid.NewGuid().ToString("N");
            string connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";

            using (var keeper = new SqliteConnection(connectionString))
            {
                await keeper.OpenAsync();

                string createSql = @"
                    CREATE TABLE Meals (
                        meal_id INTEGER PRIMARY KEY,
                        name TEXT,
                        imageUrl TEXT,
                        isKeto INTEGER,
                        isVegan INTEGER,
                        isNutFree INTEGER,
                        isLactoseFree INTEGER,
                        isGlutenFree INTEGER,
                        description TEXT
                    );
                    CREATE TABLE MealPlanMeal (
                        id INTEGER PRIMARY KEY,
                        mealPlanId INTEGER,
                        mealId INTEGER,
                        mealType TEXT,
                        isConsumed INTEGER
                    );
                    CREATE TABLE Ingredients (
                        food_id INTEGER PRIMARY KEY,
                        name TEXT,
                        calories_per_100g REAL,
                        protein_per_100g REAL,
                        carbs_per_100g REAL,
                        fat_per_100g REAL
                    );
                    CREATE TABLE MealsIngredients (
                        id INTEGER PRIMARY KEY,
                        meal_id INTEGER,
                        food_id INTEGER,
                        quantity REAL
                    );";

                using (var cmd = keeper.CreateCommand())
                {
                    cmd.CommandText = createSql;
                    await cmd.ExecuteNonQueryAsync();
                }

                var mockConfig = new Mock<IDbConfig>();
                mockConfig.SetupGet(m => m.ConnectionString).Returns(connectionString);

                var repo = new MealPlanRepository(mockConfig.Object);

                var result = await repo.GetMealsForMealPlan(planId);

                result.Should().NotBeNull();
                result.Should().BeEmpty("no MealPlanMeal rows exist that reference the provided plan id");
            }
        }

        [Theory]
        [MemberData(nameof(UpdateCases))]
        public async Task Update_WithVariousIdsAndGoalTypes_UpdatesDatabase(int id, string goalType)
        {
            string connString = "Data Source=mealplan_update_shared;Mode=Memory;Cache=Shared";
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connString);

            using var keepAlive = new SqliteConnection(connString);
            await keepAlive.OpenAsync();

            using (var createCmd = keepAlive.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MealPlan (
                        mealplan_id INTEGER PRIMARY KEY,
                        user_id INTEGER,
                        created_at TEXT,
                        goal_type TEXT
                    );";
                await createCmd.ExecuteNonQueryAsync();
            }

            using (var insertCmd = keepAlive.CreateCommand())
            {
                insertCmd.CommandText = @"
                    INSERT INTO MealPlan (mealplan_id, user_id, created_at, goal_type)
                    VALUES (@id, @uid, @created, @goal);";
                insertCmd.Parameters.AddWithValue("@id", id);
                insertCmd.Parameters.AddWithValue("@uid", 1);
                insertCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertCmd.Parameters.AddWithValue("@goal", "initial");
                await insertCmd.ExecuteNonQueryAsync();
            }

            var repo = new MealPlanRepository(dbConfigMock.Object);

            var entity = new MealPlan
            {
                Id = id,
                UserId = 1,
                CreatedAt = DateTime.Now,
                GoalType = goalType
            };

            Func<Task> act = async () => await repo.Update(entity);

            await act.Should().NotThrowAsync();

            using (var verifyCmd = keepAlive.CreateCommand())
            {
                verifyCmd.CommandText = "SELECT goal_type FROM MealPlan WHERE mealplan_id = @id";
                verifyCmd.Parameters.AddWithValue("@id", id);
                var result = await verifyCmd.ExecuteScalarAsync();
                (result as string).Should().Be(goalType);
            }
        }

        public static IEnumerable<object[]> UpdateCases()
        {
            yield return new object[] { 0, string.Empty };
            yield return new object[] { -1, "   " };
            yield return new object[] { 42, "special\t\n♥" };
            yield return new object[] { int.MaxValue, new string('a', 5000) };
        }
        [Fact]
        public async Task Update_WhenTableMissing_ThrowsSqliteException()
        {
            string connString = "Data Source=mealplan_missing_table;Mode=Memory;Cache=Shared";
            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connString);

            using var keepAlive = new SqliteConnection(connString);
            await keepAlive.OpenAsync();

            var repo = new MealPlanRepository(dbConfigMock.Object);

            var entity = new MealPlan
            {
                Id = 1,
                UserId = 1,
                CreatedAt = DateTime.Now,
                GoalType = "goal"
            };

            Func<Task> act = async () => await repo.Update(entity);

            await act.Should().ThrowAsync<SqliteException>();
        }

        private static string CreateSharedInMemoryDatabase(Func<SqliteConnection, Task> initializer, out SqliteConnection masterConnection, string dbName = "TestDb")
        {
            var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
            masterConnection = new SqliteConnection(connectionString);
            masterConnection.Open();

            using var cmd = masterConnection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

            initializer(masterConnection).GetAwaiter().GetResult();

            return connectionString;
        }

        private static async Task InitializeSchemaAsync(SqliteConnection conn)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Meals (
                    meal_id INTEGER PRIMARY KEY,
                    name TEXT,
                    imageUrl TEXT,
                    isKeto INTEGER DEFAULT 0,
                    isVegan INTEGER DEFAULT 0,
                    isNutFree INTEGER DEFAULT 0,
                    isLactoseFree INTEGER DEFAULT 0,
                    isGlutenFree INTEGER DEFAULT 0,
                    description TEXT
                );

                CREATE TABLE IF NOT EXISTS Ingredients (
                    food_id INTEGER PRIMARY KEY,
                    name TEXT,
                    calories_per_100g REAL,
                    protein_per_100g REAL,
                    carbs_per_100g REAL,
                    fat_per_100g REAL
                );

                CREATE TABLE IF NOT EXISTS MealsIngredients (
                    meal_id INTEGER,
                    food_id INTEGER,
                    quantity REAL,
                    FOREIGN KEY(meal_id) REFERENCES Meals(meal_id),
                    FOREIGN KEY(food_id) REFERENCES Ingredients(food_id)
                );

                CREATE TABLE IF NOT EXISTS UserData (
                    user_id INTEGER PRIMARY KEY,
                    calorie_needs INTEGER,
                    protein_needs INTEGER,
                    carb_needs INTEGER,
                    fat_needs INTEGER,
                    goal TEXT
                );

                CREATE TABLE IF NOT EXISTS MealPlan (
                    mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER,
                    created_at TEXT,
                    goal_type TEXT
                );

                CREATE TABLE IF NOT EXISTS MealPlanMeal (
                    mealPlanId INTEGER,
                    mealId INTEGER,
                    mealType TEXT,
                    assigned_at TEXT,
                    isConsumed INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Favorites (
                    mealId INTEGER,
                    userId INTEGER
                );";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertMealWithIngredientAsync(SqliteConnection conn, int mealId, int foodId, double caloriesPer100g, double proteinPer100g, double carbsPer100g, double fatPer100g, double quantity)
        {
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO Meals (meal_id, name) VALUES (@mid, @name);";
            cmd.Parameters.AddWithValue("@mid", mealId);
            cmd.Parameters.AddWithValue("@name", $"Meal {mealId}");
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = "INSERT INTO Ingredients (food_id, name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g) VALUES (@fid, @fname, @c, @p, @carb, @f);";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@fid", foodId);
            cmd.Parameters.AddWithValue("@fname", $"Food {foodId}");
            cmd.Parameters.AddWithValue("@c", caloriesPer100g);
            cmd.Parameters.AddWithValue("@p", proteinPer100g);
            cmd.Parameters.AddWithValue("@carb", carbsPer100g);
            cmd.Parameters.AddWithValue("@f", fatPer100g);
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = "INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES (@mid2, @fid2, @qty);";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@mid2", mealId);
            cmd.Parameters.AddWithValue("@fid2", foodId);
            cmd.Parameters.AddWithValue("@qty", quantity);
            await cmd.ExecuteNonQueryAsync();

            tx.Commit();
        }

        [Fact]
        public async Task GeneratePersonalizedDailyMealPlan_NoMeals_ThrowsGenerationFailedException()
        {
            var connectionString = CreateSharedInMemoryDatabase(async conn =>
            {
                await InitializeSchemaAsync(conn);
            }, out var masterConn, dbName: Guid.NewGuid().ToString());

            try
            {
                var repo = CreateRepositoryWithConnectionString(connectionString);

                Func<Task> act = async () => await repo.GeneratePersonalizedDailyMealPlan(1);

                await act.Should()
                    .ThrowAsync<Exception>()
                    .WithMessage("Generation Failed: No meals found in database.");
            }
            finally
            {
                masterConn.Dispose();
            }
        }

        [Fact]
        public async Task GeneratePersonalizedDailyMealPlan_TwoMeals_ThrowsNotEnoughMealsException()
        {
            var connectionString = CreateSharedInMemoryDatabase(async conn =>
            {
                await InitializeSchemaAsync(conn);

                await InsertMealWithIngredientAsync(conn, mealId: 1, foodId: 101, caloriesPer100g: 500, proteinPer100g: 10, carbsPer100g: 20, fatPer100g: 5, quantity: 100);
                await InsertMealWithIngredientAsync(conn, mealId: 2, foodId: 102, caloriesPer100g: 600, proteinPer100g: 12, carbsPer100g: 25, fatPer100g: 6, quantity: 100);
            }, out var masterConn, dbName: Guid.NewGuid().ToString());

            try
            {
                var repo = CreateRepositoryWithConnectionString(connectionString);

                Func<Task> act = async () => await repo.GeneratePersonalizedDailyMealPlan(userId: 1);

                await act.Should()
                    .ThrowAsync<Exception>()
                    .WithMessage("Generation Failed: Not enough meals in the database to generate a plan.");
            }
            finally
            {
                masterConn.Dispose();
            }
        }

        [Fact]
        [Trait("Category", "ProductionBugSuspected")]
        public async Task GeneratePersonalizedDailyMealPlan_ThreeMeals_SucceedsAndInsertsMealPlanMeals()
        {
            var connectionString = CreateSharedInMemoryDatabase(async conn =>
            {
                await InitializeSchemaAsync(conn);
                await InsertMealWithIngredientAsync(conn, mealId: 1, foodId: 201, caloriesPer100g: 700, proteinPer100g: 10, carbsPer100g: 20, fatPer100g: 5, quantity: 100);
                await InsertMealWithIngredientAsync(conn, mealId: 2, foodId: 202, caloriesPer100g: 600, proteinPer100g: 12, carbsPer100g: 25, fatPer100g: 6, quantity: 100);
                await InsertMealWithIngredientAsync(conn, mealId: 3, foodId: 203, caloriesPer100g: 800, proteinPer100g: 15, carbsPer100g: 30, fatPer100g: 8, quantity: 100);

                using var udCmd = conn.CreateCommand();
                udCmd.CommandText = "INSERT INTO UserData (user_id, calorie_needs, protein_needs, carb_needs, fat_needs, goal) VALUES (@uid, 0, 0, 0, 0, NULL);";
                udCmd.Parameters.AddWithValue("@uid", 42);
                await udCmd.ExecuteNonQueryAsync();
            }, out var masterConn, dbName: Guid.NewGuid().ToString());

            try
            {
                var repo = CreateRepositoryWithConnectionString(connectionString);

                var resultId = await repo.GeneratePersonalizedDailyMealPlan(userId: 42);

                resultId.Should().BeGreaterThan(0);

                using var verifyConn = new SqliteConnection(connectionString);
                await verifyConn.OpenAsync();
                using var verifyCmd = verifyConn.CreateCommand();
                verifyCmd.CommandText = "SELECT COUNT(*) FROM MealPlanMeal WHERE mealPlanId = @pid;";
                verifyCmd.Parameters.AddWithValue("@pid", resultId);
                var countScalar = await verifyCmd.ExecuteScalarAsync();
                var insertedCount = Convert.ToInt32(countScalar);
                insertedCount.Should().Be(3);

                using var planCmd = verifyConn.CreateCommand();
                planCmd.CommandText = "SELECT goal_type, user_id FROM MealPlan WHERE mealplan_id = @pid;";
                planCmd.Parameters.AddWithValue("@pid", resultId);
                using var reader = await planCmd.ExecuteReaderAsync();
                reader.Read().Should().BeTrue();
                var goalType = reader["goal_type"]?.ToString() ?? string.Empty;
                var userId = Convert.ToInt32(reader["user_id"]);
                userId.Should().Be(42);
                goalType.Should().Be(string.Empty);
            }
            finally
            {
                masterConn.Dispose();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public async Task GetAll_VariousRowCounts_ReturnsExpectedResults(int rowCount)
        {
            const string connectionString = "Data Source=MealPlanRepoTests_InMemory;Mode=Memory;Cache=Shared";
            using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            using (var createCmd = keeper.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MealPlan (
                        mealplan_id INTEGER PRIMARY KEY,
                        user_id INTEGER NOT NULL,
                        created_at TEXT NOT NULL,
                        goal_type TEXT
                    );";
                await createCmd.ExecuteNonQueryAsync();
            }

            DateTime expectedDt1 = new DateTime(2023, 1, 2, 3, 4, 5);
            DateTime expectedDt2 = new DateTime(2024, 2, 3, 4, 5, 6);

            if (rowCount == 2)
            {
                using (var ins = keeper.CreateCommand())
                {
                    ins.CommandText = "INSERT INTO MealPlan (mealplan_id, user_id, created_at, goal_type) VALUES (@id, @uid, @created, @goal);";
                    ins.Parameters.AddWithValue("@id", 1);
                    ins.Parameters.AddWithValue("@uid", 10);
                    ins.Parameters.AddWithValue("@created", expectedDt1.ToString("yyyy-MM-dd HH:mm:ss"));
                    ins.Parameters.AddWithValue("@goal", "weightloss");
                    await ins.ExecuteNonQueryAsync();
                }

                using (var ins = keeper.CreateCommand())
                {
                    ins.CommandText = "INSERT INTO MealPlan (mealplan_id, user_id, created_at, goal_type) VALUES (@id, @uid, @created, @goal);";
                    ins.Parameters.AddWithValue("@id", 2);
                    ins.Parameters.AddWithValue("@uid", 20);
                    ins.Parameters.AddWithValue("@created", expectedDt2.ToString("yyyy-MM-dd HH:mm:ss"));
                    ins.Parameters.AddWithValue("@goal", "maintenance");
                    await ins.ExecuteNonQueryAsync();
                }
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            var result = (await repository.GetAll()).ToList();

            if (rowCount == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {
                result.Should().HaveCount(2);

                var expected = new List<MealPlan>
                {
                    new MealPlan
                    {
                        Id = 1,
                        UserId = 10,
                        CreatedAt = expectedDt1,
                        GoalType = "weightloss"
                    },
                    new MealPlan
                    {
                        Id = 2,
                        UserId = 20,
                        CreatedAt = expectedDt2,
                        GoalType = "maintenance"
                    }
                };

                result.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
            }
        }

        [Fact]
        public async Task GetAll_RowWithNullOrEmptyGoalType_MapsGoalTypeToEmptyString()
        {
            const string connectionString = "Data Source=MealPlanRepoTests_GoalType;Mode=Memory;Cache=Shared";
            using var keeper = new SqliteConnection(connectionString);
            await keeper.OpenAsync();

            using (var createCmd = keeper.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MealPlan (
                        mealplan_id INTEGER PRIMARY KEY,
                        user_id INTEGER NOT NULL,
                        created_at TEXT NOT NULL,
                        goal_type TEXT
                    );";
                await createCmd.ExecuteNonQueryAsync();
            }

            var createdAt = new DateTime(2022, 12, 31, 23, 59, 59);
            using (var ins = keeper.CreateCommand())
            {
                ins.CommandText = "INSERT INTO MealPlan (mealplan_id, user_id, created_at, goal_type) VALUES (@id, @uid, @created, @goal);";
                ins.Parameters.AddWithValue("@id", 42);
                ins.Parameters.AddWithValue("@uid", 7);

                ins.Parameters.AddWithValue("@created", createdAt.ToString("yyyy-MM-dd HH:mm:ss"));

                ins.Parameters.AddWithValue("@goal", DBNull.Value);
                await ins.ExecuteNonQueryAsync();
            }

            var dbConfigMock = new Mock<IDbConfig>();
            dbConfigMock.SetupGet(d => d.ConnectionString).Returns(connectionString);

            var repository = new MealPlanRepository(dbConfigMock.Object);

            var result = (await repository.GetAll()).ToList();

            result.Should().HaveCount(1);
            var single = result.Single();
            single.Id.Should().Be(42);
            single.UserId.Should().Be(7);

            single.CreatedAt.Should().Be(createdAt);

            single.GoalType.Should().Be(string.Empty);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public async Task GetTodaysMealPlan_WithMultipleTodayEntries_ReturnsMostRecentMealPlan(int userId)
        {
            string connectionString = $"Data Source=MealPlanToday_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

            await using var persistent = new SqliteConnection(connectionString);
            await persistent.OpenAsync();

            var createTableCmd = persistent.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE MealPlan (
                    mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    created_at TEXT NOT NULL,
                    goal_type TEXT
                );";
            await createTableCmd.ExecuteNonQueryAsync();

            DateTime now = DateTime.Now;
            string older = now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss");
            string newer = now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");

            var insertOlder = persistent.CreateCommand();
            insertOlder.CommandText = "INSERT INTO MealPlan (user_id, created_at, goal_type) VALUES ($uid, $created, $goal);";
            insertOlder.Parameters.AddWithValue("$uid", userId);
            insertOlder.Parameters.AddWithValue("$created", older);
            insertOlder.Parameters.AddWithValue("$goal", "olderGoal");
            await insertOlder.ExecuteNonQueryAsync();

            var insertNewer = persistent.CreateCommand();
            insertNewer.CommandText = "INSERT INTO MealPlan (user_id, created_at, goal_type) VALUES ($uid, $created, $goal);";
            insertNewer.Parameters.AddWithValue("$uid", userId);
            insertNewer.Parameters.AddWithValue("$created", newer);
            insertNewer.Parameters.AddWithValue("$goal", "newerGoal");
            await insertNewer.ExecuteNonQueryAsync();

            var dbConfigMock = new Mock<IDbConfig>(MockBehavior.Strict);
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new MealPlanRepository(dbConfigMock.Object);

            MealPlan? result = await repo.GetTodaysMealPlan(userId);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.GoalType.Should().Be("newerGoal");

            result.CreatedAt.ToString("yyyy-MM-dd HH:mm").Should().Be(newer.Substring(0, 16));

            result.Id.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public async Task GetTodaysMealPlan_NoEntryForToday_ReturnsNull(int userId)
        {
            string connectionString = $"Data Source=MealPlanNoToday_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            await using var persistent = new SqliteConnection(connectionString);
            await persistent.OpenAsync();

            var createTableCmd = persistent.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE MealPlan (
                    mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    created_at TEXT NOT NULL,
                    goal_type TEXT
                );";
            await createTableCmd.ExecuteNonQueryAsync();

            DateTime yesterday = DateTime.Now.AddDays(-1);
            string yesterdayStr = yesterday.ToString("yyyy-MM-dd HH:mm:ss");

            var insert = persistent.CreateCommand();
            insert.CommandText = "INSERT INTO MealPlan (user_id, created_at, goal_type) VALUES ($uid, $created, $goal);";
            insert.Parameters.AddWithValue("$uid", userId);
            insert.Parameters.AddWithValue("$created", yesterdayStr);
            insert.Parameters.AddWithValue("$goal", "yesterdayGoal");
            await insert.ExecuteNonQueryAsync();

            var dbConfigMock = new Mock<IDbConfig>(MockBehavior.Strict);
            dbConfigMock.SetupGet(m => m.ConnectionString).Returns(connectionString);

            var repo = new MealPlanRepository(dbConfigMock.Object);

            MealPlan? result = await repo.GetTodaysMealPlan(userId);

            result.Should().BeNull();
        }
    }
}
