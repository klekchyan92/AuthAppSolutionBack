using Dapper;
using Microsoft.Data.SqlClient;

namespace AuthApp.Middlewares
{
    public class CheckUserActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _connectionString;

        public CheckUserActivityMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var username = context.User.Identity.Name;

                using (var connection = new SqlConnection(_connectionString))
                {
                    var sql = "SELECT IsActive FROM Users u JOIN UserStatus us ON u.Id = us.UserId WHERE u.Username = @Username";
                    var isActive = await connection.QuerySingleOrDefaultAsync<bool>(sql, new { Username = username });

                    if (!isActive)
                    {
                        context.Response.Redirect("/login");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}
