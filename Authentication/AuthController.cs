using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Dtos;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registration)
        {
            if (string.IsNullOrWhiteSpace(registration.Password))
            {
                return BadRequest("Password cannot be empty.");
            }

            var user = await _userService.Register(registration);
            if (user == null)
            {
                return BadRequest("Invalid registration.");
            }

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto login)
        {
            try
            {
                var token = await _userService.Authenticate(login.Username, login.Password);

                if (token != null)
                {
                    return Ok(new { Token = token });
                }
                else
                {
                    return new UnauthorizedResult();
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }

}