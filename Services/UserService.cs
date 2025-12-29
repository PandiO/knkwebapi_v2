using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var user = await _repo.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByUuidAsync(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid)) return null;
            var user = await _repo.GetByUuidAsync(uuid);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var user = await _repo.GetByUsernameAsync(username);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (string.IsNullOrWhiteSpace(userDto.Username)) throw new ArgumentException("Username is required.", nameof(userDto));
            if (string.IsNullOrWhiteSpace(userDto.Email)) throw new ArgumentException("Email is required.", nameof(userDto));

            var user = _mapper.Map<User>(userDto);
            user.CreatedAt = DateTime.UtcNow;
            
            await _repo.AddUserAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateAsync(int id, UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(userDto.Username)) throw new ArgumentException("Username is required.", nameof(userDto));
            if (string.IsNullOrWhiteSpace(userDto.Email)) throw new ArgumentException("Email is required.", nameof(userDto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            existing.Username = userDto.Username;
            existing.Email = userDto.Email;
            existing.Coins = userDto.Coins;

            await _repo.UpdateUserAsync(existing);
        }

        public async Task UpdateCoinsAsync(int id, int coins)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            await _repo.UpdateUserCoinsAsync(id, coins);
        }

        public async Task UpdateCoinsByUuidAsync(string uuid, int coins)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Invalid uuid.", nameof(uuid));
            var existing = await _repo.GetByUuidAsync(uuid);
            if (existing == null) throw new KeyNotFoundException($"User with uuid {uuid} not found.");

            await _repo.UpdateUserCoinsByUuidAsync(uuid, coins);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"User with id {id} not found.");

            await _repo.DeleteUserAsync(id);
        }

        public async Task<PagedResultDto<UserListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            var resultDto = _mapper.Map<PagedResultDto<UserListDto>>(result);

            return resultDto;
        }
    }
}
