using System.Threading.Tasks;
using TodoApp.Dtos;
using TodoApp.Models;

namespace TodoApp.Services
{
    public interface IUserService
    {
        Task<string> Authenticate(string username, string password);  // To authenticate users and return the user entity if valid
        Task<User> Register(UserRegisterDto registrationDto);       // To register new users
        string GenerateJwtToken(User user);                         // To generate JWT tokens for authenticated users
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt);

    }

}
