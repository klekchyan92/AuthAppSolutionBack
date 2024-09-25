using AuthApp.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginDetails)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT u.Username, u.Password, us.IsActive 
                    FROM Users u 
                    JOIN UserStatus us ON u.Id = us.UserId 
                    WHERE u.Username = @Username AND u.Password = @Password";

                var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = loginDetails.Username, Password = loginDetails.Password });

                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                return Ok(new { message = $"Welcome, {user.Username}!" });
            }
        }
    }
}
