using QuanLyInAn.Data;
using QuanLyInAn.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuanLyInAn.Repositories
{
    public class ConfirmEmailRepository
    {
        private readonly AppDbContext _context;

        public ConfirmEmailRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConfirmEmail> GetConfirmEmailByCodeAsync(string code)
        {
            return await _context.ConfirmEmails.SingleOrDefaultAsync(c => c.ConfirmCode == code);
        }

        public async Task AddConfirmEmailAsync(ConfirmEmail confirmEmail)
        {
            _context.ConfirmEmails.Add(confirmEmail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConfirmEmailAsync(ConfirmEmail confirmEmail)
        {
            _context.ConfirmEmails.Update(confirmEmail);
            await _context.SaveChangesAsync();
        }
    }
}
