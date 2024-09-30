using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using System.Security.Cryptography;
using System.Text;
using TodoApp.Services;
using TodoApp.Dtos;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> Authenticate(string username, string password)
    {
        // Retrieve the user from the database
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);

        // Check if the user exists and verify the password
        if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        // Authentication successful, return the user
        return user;
    }

    public async Task<User> Register(UserRegisterDto registrationDto)
    {
        // Check if the username is already taken
        if (await _context.Users.AnyAsync(x => x.Username == registrationDto.Username))
        {
            return null; // Username already taken
        }

        // Create a new user object
        CreatePasswordHash(registrationDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = registrationDto.Username,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        // Add the user to the database and save changes
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    // Helper method to create the password hash and salt
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key; // Generate a random salt
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash the password with the salt
        }
    }

    // Helper method to verify the password hash
    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i])
                {
                    return false;
                }
            }
        }
        return true;
    }
}
