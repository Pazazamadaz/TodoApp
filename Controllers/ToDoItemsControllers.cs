using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodoItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            // Extract the username from the JWT token
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User information is missing.");
            }

            // Retrieve the user based on the username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Filter TodoItems by the UserId
            var userTodoItems = await _context.TodoItems
                                              .Where(item => item.UserId == user.Id)
                                              .ToListAsync();

            return userTodoItems;
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItemNew todoItemNew)
        {
            // Retreive user id
            User user = _context.Users
                .Where(u => u.Username == User.FindFirstValue(ClaimTypes.Name))
                .FirstOrDefault();
            // todoItenNew has no id. Map the props it does have to a ToDoItem type object
            var todoItem = new TodoItem
            {
                UserId = user.Id,
                Title = todoItemNew.Title,
                IsCompleted = todoItemNew.IsCompleted
            };

            // id is Identity column in the database... 
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            // so the success message will be returned with the id
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
        {
            // Check if the ID in the request matches the TodoItem's ID
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            // Extract the username from the JWT token
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User information is missing.");
            }

            // Retrieve the user based on the username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Find the existing TodoItem
            var existingItem = await _context.TodoItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Ensure that the TodoItem belongs to the authenticated user
            if (existingItem.UserId != user.Id)
            {
                return Forbid("You are not allowed to edit this task.");
            }

            // Update the fields that are allowed to be modified
            existingItem.Title = todoItem.Title;
            existingItem.IsCompleted = todoItem.IsCompleted;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(int id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
