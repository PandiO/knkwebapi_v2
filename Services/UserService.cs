using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<User> CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username is required.", nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email is required.", nameof(user));

            await _repo.AddUserAsync(user);
            return user;
        }

        public async Task UpdateAsync(int id, User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username is required.", nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email is required.", nameof(user));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            existing.Username = user.Username;
            existing.Email = user.Email;

            await _repo.UpdateUserAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            await _repo.DeleteUserAsync(id);
        }
    }
}
