using AuthApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AuthApp.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserService> _logger;
        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var sql = "SELECT u.Id, u.Username, u.Password, s.IsActive FROM Users u LEFT JOIN UserStatus s ON u.Id = s.UserId WHERE Username = @Username";
                    var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });

                    if (user != null && user.Password == password)
                    {
                        return user;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating user {Username}", username);
                throw new Exception("An error occurred while validating the user.");
            }
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var sql = @"
                    SELECT u.Id, u.Username, u.Password, s.IsActive
                    FROM Users u
                    LEFT JOIN UserStatus s ON u.Id = s.UserId";
                    return await connection.QueryAsync<User>(sql);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching users.");
                throw new Exception("An error occurred while fetching users.");
            }
        }

        public async Task UpdateUserActivity(string username, bool isActive)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var sql = @"
                    UPDATE UserStatus
                    SET IsActive = @IsActive
                    FROM UserStatus s
                    INNER JOIN Users u ON s.UserId = u.Id
                    WHERE u.Username = @Username";

                    await connection.ExecuteAsync(sql, new { IsActive = isActive, Username = username });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating user activity for {Username}", username);
                throw new Exception("An error occurred while updating user activity.");
            }
        }

        public async Task CreateUser(User newUser)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var sqlUser = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password); SELECT CAST(SCOPE_IDENTITY() as int)";
                            var userId = await connection.ExecuteScalarAsync<int>(sqlUser, newUser, transaction);

                            var sqlStatus = "INSERT INTO UserStatus (UserId, IsActive) VALUES (@UserId, @IsActive)";
                            await connection.ExecuteAsync(sqlStatus, new { UserId = userId, IsActive = newUser.IsActive }, transaction);

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user {Username}", newUser.Username);
                throw new Exception("An error occurred while creating a new user.");
            }
        }
    }
}
