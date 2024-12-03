using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TodoApp.Models;
using TodoApp.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System;
using TodoApp.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace TodoApp.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secretKey;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            var jwtSettings = configuration.GetSection("JwtSettings");
            _issuer = jwtSettings["Issuer"];
            _audience = jwtSettings["Audience"];
            _secretKey = jwtSettings["SecretKey"];
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registration)
        {
            // Validate username format (A-Z, numbers, hyphen, no other characters)
            if (string.IsNullOrWhiteSpace(registration.Username) ||
                !System.Text.RegularExpressions.Regex.IsMatch(registration.Username, @"^(?=.*[a-zA-Z0-9\-])[a-zA-Z0-9\- ]+$"))
            {
                return BadRequest("Invalid username format.");
            }

            // Validate password is not empty
            if (string.IsNullOrWhiteSpace(registration.Password))
            {
                return BadRequest("Password cannot be empty.");
            }

            // Attempt to register the user
            var user = await _userService.Register(registration);
            if (user == null)
            {
                return BadRequest("Invalid registration.");
            }

            return Ok("User registered successfully");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto login)
        {
            try
            {
                var userToken = await _userService.Authenticate(login.Username, login.Password);

                if (userToken == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                return Ok(new { Token = userToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
