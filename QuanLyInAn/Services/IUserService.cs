using QuanLyInAn.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyInAn.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string userId); 
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
    }
}
