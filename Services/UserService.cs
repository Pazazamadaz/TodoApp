using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Dtos;
using TodoApp.Models;
using TodoApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> Authenticate(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);

        if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            return null; // Authentication failed
        }

        return GenerateJwtToken(user);
    }

    public async Task<User> Register(UserRegisterDto registrationDto)
    {
        if (await _context.Users.AnyAsync(x => x.Username == registrationDto.Username))
        {
            return null; // Username already taken
        }

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

        return user; // Return the newly created user
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.Id.ToString())
        };

        // Add "Administrator" role if the user is an admin
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

        // Add user's default colour theme
        ColourTheme DefaultColourTheme = GetDefaultColourTheme(user);
        if (DefaultColourTheme != null)
        {
            // Add the DefaultColourTheme as individual claims to avoid double encoding
            claims.Add(new Claim("DefaultColourTheme.Name", DefaultColourTheme.Name ?? string.Empty));
            claims.Add(new Claim("DefaultColourTheme.IsDefault", DefaultColourTheme.IsDefault.ToString()));
            claims.Add(new Claim("DefaultColourTheme.SysDefined", DefaultColourTheme.SysDefined.ToString()));
            claims.Add(new Claim("DefaultColourTheme.IsActive", DefaultColourTheme.IsActive.ToString()));

            // Add Colours as individual claims (one for each colour)
            foreach (var colour in DefaultColourTheme.Colours)
            {
                claims.Add(new Claim($"DefaultColourTheme.Colour.{colour.ColourProperty}", colour.ColourValue));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30), // Token expiration time
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token); // Return the token as a string
    }

    // Helper method to create the password hash and salt
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key; // Generate a random salt
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash the password with the salt
        }
    }

    // Helper method to verify the password hash
    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
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

    private ColourTheme GetDefaultColourTheme(User user)
    {
        ColourTheme DefaultColourTheme = _context.ColourThemes
            .Include(theme => theme.Colours)
            .Where(theme => theme.UserId == user.Id)
            .Where(theme => theme.IsDefault)
            .Where(theme => theme.IsActive)
            .FirstOrDefault();

        if (DefaultColourTheme == null)
        {
            return _context.ColourThemes
                .Include(theme => theme.Colours)
                .Where(theme => theme.Name == "Default Theme")
                .FirstOrDefault();
        }

        return DefaultColourTheme;
    }
}
