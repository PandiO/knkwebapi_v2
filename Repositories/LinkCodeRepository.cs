using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    /// <summary>
    /// Repository implementation for LinkCode entity operations.
    /// </summary>
    public class LinkCodeRepository : ILinkCodeRepository
    {
        private readonly KnKDbContext _context;

        public LinkCodeRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<LinkCode> CreateAsync(LinkCode linkCode)
        {
            await _context.LinkCodes.AddAsync(linkCode);
            await _context.SaveChangesAsync();
            return linkCode;
        }

        public async Task<LinkCode?> GetByCodeAsync(string code)
        {
            return await _context.LinkCodes
                .Include(lc => lc.User)
                .FirstOrDefaultAsync(lc => lc.Code == code);
        }

        public async Task<LinkCode?> GetLinkCodeByCodeAsync(string code)
        {
            return await GetByCodeAsync(code);
        }

        public async Task<LinkCode?> GetByIdAsync(int id)
        {
            return await _context.LinkCodes
                .Include(lc => lc.User)
                .FirstOrDefaultAsync(lc => lc.Id == id);
        }

        public async Task UpdateAsync(LinkCode linkCode)
        {
            _context.LinkCodes.Update(linkCode);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLinkCodeStatusAsync(int id, LinkCodeStatus status)
        {
            var linkCode = await GetByIdAsync(id);
            if (linkCode != null)
            {
                linkCode.Status = status;
                if (status == LinkCodeStatus.Used)
                {
                    linkCode.UsedAt = DateTime.UtcNow;
                }
                await UpdateAsync(linkCode);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var linkCode = await GetByIdAsync(id);
            if (linkCode != null)
            {
                _context.LinkCodes.Remove(linkCode);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<LinkCode>> GetExpiredAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.LinkCodes
                .Where(lc => lc.ExpiresAt < now && lc.Status != LinkCodeStatus.Used)
                .ToListAsync();
        }

        public async Task<IEnumerable<LinkCode>> GetExpiredLinkCodesAsync()
        {
            return await GetExpiredAsync();
        }
    }
}
