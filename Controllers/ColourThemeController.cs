using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ColourThemeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ColourThemeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ColourTheme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ColourTheme>>> GetColourThemes()
        {
            return await _context.ColourThemes.ToListAsync();
        }

        // GET: api/ColourTheme/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ColourTheme>> GetColourTheme(int id)
        {
            var colourTheme = await _context.ColourThemes.FindAsync(id);

            if (colourTheme == null)
            {
                return NotFound();
            }

            return colourTheme;
        }

        // POST: api/ColourTheme
        [HttpPost]
        public async Task<ActionResult<ColourTheme>> PostColourTheme(ColourTheme colourTheme)
        {
            // Associate the user ID with the theme
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User information is missing.");
            }

            colourTheme.UserId = int.Parse(userId);
            _context.ColourThemes.Add(colourTheme);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetColourTheme", new { id = colourTheme.Id }, colourTheme);
        }

        // PUT: api/ColourTheme
        [HttpPut()]
        public async Task<IActionResult> PutColourTheme([FromBody] ColourTheme colourTheme)
        {

            var existingTheme = await _context.ColourThemes
                .Include(theme => theme.Colours)
                .FirstOrDefaultAsync(theme => theme.Id == colourTheme.Id);

            if (existingTheme == null)
                return NotFound();

            // Only allow updates for themes the user owns
            var userId = User.FindFirstValue("UserId");
            if (existingTheme.UserId.ToString() != userId || existingTheme.SystemDefined)
            {
                return Forbid("Unauthorised");
            }

            existingTheme.Name = colourTheme.Name;
            existingTheme.Colours = colourTheme.Colours;
            existingTheme.IsDefault = colourTheme.IsDefault;
            existingTheme.SystemDefined = colourTheme.SystemDefined;
            existingTheme.UserId = existingTheme.UserId;
            existingTheme.IsActive = colourTheme.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ColourTheme/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColourTheme(int id)
        {
            var colourTheme = await _context.ColourThemes.FindAsync(id);
            if (colourTheme == null)
            {
                return NotFound();
            }

            // Only allow deletion for themes the user owns
            var userId = User.FindFirstValue("UserId");
            if (colourTheme.UserId.ToString() != userId && colourTheme.SystemDefined)
            {
                return Forbid("Cannot delete system-defined themes.");
            }

            _context.ColourThemes.Remove(colourTheme);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
