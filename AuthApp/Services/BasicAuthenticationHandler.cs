using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace AuthApp.Services
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserService _userService;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                            ILoggerFactory logger,
                                            UrlEncoder encoder,
                                            ISystemClock clock, IUserService userService) : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (!authHeader.StartsWith("Basic "))
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                var authHeaderValue = authHeader.Substring("Basic ".Length).Trim();
                var authValueBytes = Convert.FromBase64String(authHeaderValue);
                var authValue = Encoding.UTF8.GetString(authValueBytes);
                var usernamePassword = authValue.Split(':', 2);

                if (usernamePassword.Length != 2)
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                var username = usernamePassword[0];
                var password = usernamePassword[1];

                var user = await _userService.ValidateUserAsync(username, password);
                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }

                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (FormatException)
            {
                return AuthenticateResult.Fail("Invalid Authorization Header format");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Authentication failed");
                return AuthenticateResult.Fail("Authentication failed");
            }
        }
    }
}