using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly KnKDbContext _context;

        public UserRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUuidAsync(string uuid)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Uuid == uuid);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserCoinsAsync(int id, int coins)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Coins = coins;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUserCoinsByUuidAsync(string uuid, int coins)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uuid == uuid);
            if (user != null)
            {
                user.Coins = coins;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<User>> SearchAsync(PagedQuery query)
        {
            var queryable = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchLower = query.SearchTerm.ToLower();
                queryable = queryable.Where(u => u.Username.ToLower().Contains(searchLower) ||
                                                  u.Email.ToLower().Contains(searchLower) ||
                                                  u.Uuid.ToLower().Contains(searchLower));
            }

            if (query.Filters != null)
            {
                if (query.Filters.TryGetValue("id", out var idStr) && int.TryParse(idStr, out var id))
                {
                    queryable = queryable.Where(u => u.Id == id);
                }

                if (query.Filters.TryGetValue("uuid", out var uuid))
                {
                    queryable = queryable.Where(u => u.Uuid.ToLower().Contains(uuid.ToLower()));
                }

                if (query.Filters.TryGetValue("username", out var username))
                {
                    queryable = queryable.Where(u => u.Username.ToLower().Contains(username.ToLower()));
                }

                if (query.Filters.TryGetValue("email", out var email))
                {
                    queryable = queryable.Where(u => u.Email.ToLower().Contains(email.ToLower()));
                }
            }

            queryable = ApplySorting(queryable, query.SortBy, query.SortDescending);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private IQueryable<User> ApplySorting(IQueryable<User> queryable, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return queryable.OrderBy(u => u.Username);

            return sortBy.ToLower() switch
            {
                "username" => sortDescending ? queryable.OrderByDescending(u => u.Username) : queryable.OrderBy(u => u.Username),
                "id" => sortDescending ? queryable.OrderByDescending(u => u.Id) : queryable.OrderBy(u => u.Id),
                "email" => sortDescending ? queryable.OrderByDescending(u => u.Email) : queryable.OrderBy(u => u.Email),
                "uuid" => sortDescending ? queryable.OrderByDescending(u => u.Uuid) : queryable.OrderBy(u => u.Uuid),
                "coins" => sortDescending ? queryable.OrderByDescending(u => u.Coins) : queryable.OrderBy(u => u.Coins),
                "createdat" => sortDescending ? queryable.OrderByDescending(u => u.CreatedAt) : queryable.OrderBy(u => u.CreatedAt),
                _ => queryable.OrderBy(u => u.Username)
            };
        }
    }
}
