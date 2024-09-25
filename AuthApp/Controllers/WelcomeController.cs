using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WelcomeController(IConfiguration configuration) : ControllerBase
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWelcomeMessage()
        {
            var username = User.Identity.Name;

            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT IsActive FROM Users u JOIN UserStatus us ON u.Id = us.UserId WHERE u.Username = @Username";
                var isActive = await connection.QuerySingleOrDefaultAsync<bool>(sql, new { Username = username });

                if (!isActive)
                {
                    return Unauthorized(new { message = "User is inactive, please contact admin." });
                }
            }


            return Ok($"Welcome, {username}! This page is for authenticated users only.");
        }

    }
}
