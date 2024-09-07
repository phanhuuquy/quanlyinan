using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace QuanLyInAn.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetTokenByUserIdAsync(int userId)
        {
            return await _context.RefreshTokens.SingleOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task AddTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
