using AuthApp.Models;
using AuthApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUserActivity(string username, [FromBody] bool isActive)
        {
            await _userService.UpdateUserActivity(username, isActive);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            await _userService.CreateUser(newUser);
            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, newUser);
        }
    }
}
