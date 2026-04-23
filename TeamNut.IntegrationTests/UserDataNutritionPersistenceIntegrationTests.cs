// <copyright file="UserDataNutritionPersistenceIntegrationTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeamNut.Models;
using TeamNut.Repositories;
using TeamNut.Repositories.Interfaces;
using TeamNut.Services;
using Xunit;

namespace TeamNut.IntegrationTests
{
    public class UserDataNutritionPersistenceIntegrationTests : IDisposable
    {
        private readonly string dbPath;

        private readonly string connectionString;

        public UserDataNutritionPersistenceIntegrationTests()
        {
            dbPath = Path.Combine(
                Path.GetTempPath(),
                $"teamnut-integration-{Guid.NewGuid():N}.db");
            connectionString = $"Data Source={dbPath}";
            CreateSchema();
        }

        [Fact]
        public async Task AddUserDataAsync_ComputesAndPersistsNutritionFields()
        {
            IDbConfig dbConfig = new TestDbConfig(connectionString);
            var userRepository = new UserRepository(dbConfig);
            var nutritionService = new NutritionCalculationService();
            var userService = new UserService(userRepository, nutritionService);

            var user = new User
            {
                Username = $"integration-user-{Guid.NewGuid():N}",
                Password = "Password123",
                Role = "User"
            };
            await userRepository.Add(user);

            var inputData = new UserData
            {
                UserId = user.Id,
                Weight = 80,
                Height = 180,
                Gender = "male",
                Goal = "maintenance"
            };
            var birthDate = new DateTimeOffset(1999, 5, 12, 0, 0, 0, TimeSpan.Zero);

            await userService.AddUserDataAsync(inputData, birthDate);

            var persisted = await userRepository.GetUserDataByUserId(user.Id);

            Assert.NotNull(persisted);
            Assert.Equal(user.Id, persisted!.UserId);
            Assert.Equal(inputData.Weight, persisted.Weight);
            Assert.Equal(inputData.Height, persisted.Height);
            Assert.Equal(inputData.Gender, persisted.Gender);
            Assert.Equal(inputData.Goal, persisted.Goal);
            Assert.Equal(inputData.Age, persisted.Age);
            Assert.Equal(inputData.Bmi, persisted.Bmi);
            Assert.Equal(inputData.CalorieNeeds, persisted.CalorieNeeds);
            Assert.Equal(inputData.ProteinNeeds, persisted.ProteinNeeds);
            Assert.Equal(inputData.CarbNeeds, persisted.CarbNeeds);
            Assert.Equal(inputData.FatNeeds, persisted.FatNeeds);
            Assert.True(persisted.Age > 0);
            Assert.True(persisted.CalorieNeeds > 0);
            Assert.True(persisted.ProteinNeeds > 0);
            Assert.True(persisted.CarbNeeds >= 0);
            Assert.True(persisted.FatNeeds > 0);
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(dbPath))
            {
                try
                {
                    File.Delete(dbPath);
                }
                catch (IOException)
                {
                    
                }
                catch (UnauthorizedAccessException)
                {
                    
                }
            }
        }

        private void CreateSchema()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    role TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS UserData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL UNIQUE,
    weight INTEGER NOT NULL,
    height INTEGER NOT NULL,
    age INTEGER NOT NULL,
    gender TEXT NOT NULL,
    goal TEXT NOT NULL,
    bmi REAL NOT NULL,
    calorie_needs INTEGER NOT NULL,
    protein_needs INTEGER NOT NULL,
    carb_needs INTEGER NOT NULL,
    fat_needs INTEGER NOT NULL,
    FOREIGN KEY(user_id) REFERENCES Users(id) ON DELETE CASCADE
);";
            command.ExecuteNonQuery();
        }

        private sealed class TestDbConfig : IDbConfig
        {
            public TestDbConfig(string value)
            {
                ConnectionString = value;
            }

            public string ConnectionString { get; }
        }
    }
}
