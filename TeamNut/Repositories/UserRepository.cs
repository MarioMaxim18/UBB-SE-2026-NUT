using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamNut;
using TeamNut.Models;
namespace TeamNut.Repositories;



public class UserRepository : IRepository<User>
{
    private readonly string _connectionString = DbConfig.ConnectionString;
    public async Task<User> GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT Id, Username, Password, Role FROM Users WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                Role = reader.GetString(3)
            };
        }
        return null;
    }

    public async Task Add(User entity)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("INSERT INTO Users (Username, Password, Role) VALUES (@u, @p, @r)", conn);

        cmd.Parameters.AddWithValue("@u", entity.Username);
        cmd.Parameters.AddWithValue("@p", entity.Password);
        cmd.Parameters.AddWithValue("@r", entity.Role);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task Update(User entity)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("UPDATE Users SET Username=@u, Password=@p, Role=@r WHERE Id=@id", conn);

        cmd.Parameters.AddWithValue("@u", entity.Username);
        cmd.Parameters.AddWithValue("@p", entity.Password);
        cmd.Parameters.AddWithValue("@r", entity.Role);
        cmd.Parameters.AddWithValue("@id", entity.Id);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not initialized.");
        }

        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using var command = new SqlCommand("SELECT Id, Username, Password, Role FROM Users", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }
        }
        return users;
    }
}
