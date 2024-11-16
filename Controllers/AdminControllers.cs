using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Dtos;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetUsernames()
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User information is missing.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var usernames = await _context.Users
                                          .Select(u => u.Username)
                                          .ToListAsync();

            return usernames;
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] UserDeleteDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username))
            {
                return BadRequest("Username is required.");
            }

            // Retrieve the user and their ID
            var userToDelete = await _context.Users
                .SingleOrDefaultAsync(u => u.Username == request.Username);

            if (userToDelete == null)
            {
                return NotFound("User not found.");
            }

            // Store the user's ID
            var userId = userToDelete.Id;

            // Delete the user
            _context.Users.Remove(userToDelete);

            // Delete the user's todo items
            var todoItemsToDelete = _context.TodoItems.Where(t => t.UserId == userId);
            _context.TodoItems.RemoveRange(todoItemsToDelete);

            // Save changes
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}   
