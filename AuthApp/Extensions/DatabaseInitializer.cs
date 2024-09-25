using Dapper;
using Microsoft.Data.SqlClient;

namespace AuthApp.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _masterConnectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _masterConnectionString = configuration.GetConnectionString("MasterConnection");
        }

        public async Task InitializeAsync()
        {
            using (var masterConnection = new SqlConnection(_masterConnectionString))
            {
                var dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

                var checkDbQuery = $"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{dbName}') CREATE DATABASE [{dbName}]";
                await masterConnection.ExecuteAsync(checkDbQuery);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var tableCheckQuery = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U') SELECT 0 ELSE SELECT 1";
                var tableExists = await connection.ExecuteScalarAsync<int>(tableCheckQuery);

                if (tableExists == 0)
                {
                    var createUsersTable = @"
                CREATE TABLE Users (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Username NVARCHAR(50) NOT NULL,
                    Password NVARCHAR(50) NOT NULL
                );

                CREATE TABLE UserStatus (
                    UserId INT PRIMARY KEY,
                    IsActive BIT NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );
                ";
                    await connection.ExecuteAsync(createUsersTable);

                    var insertData = @"
                        INSERT INTO Users (Username, Password) VALUES 
                        ('user1', 'password'),
                        ('user2', 'password'),
                        ('user3', 'password'),
                        ('user4', 'password'),
                        ('user5', 'password'),
                        ('user6', 'password'),
                        ('user7', 'password'),
                        ('user8', 'password'),
                        ('user9', 'password');

                        INSERT INTO UserStatus (UserId, IsActive) VALUES 
                        (1, 1),
                        (2, 0),
                        (3, 1),
                        (4, 1),
                        (5, 0),
                        (6, 0),
                        (7, 1),
                        (8, 0),
                        (9, 1);
                        ";
                    await connection.ExecuteAsync(insertData);

                    Console.WriteLine("База данных и таблицы созданы и инициализированы.");
                }
                else
                {
                    Console.WriteLine("Таблицы уже существуют.");
                }
            }
        }
    }
}
