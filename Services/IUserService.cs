using System.Threading.Tasks;
using TodoApp.Dtos;
using TodoApp.Models;

namespace TodoApp.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<User> Register(UserRegisterDto registrationDto);
    }

}
