using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _repo;

        public DomainService(IDomainRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Domain>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Domain?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Domain> CreateAsync(Domain domain)
        {
            if (domain == null) throw new ArgumentNullException(nameof(domain));
            if (string.IsNullOrWhiteSpace(domain.Name)) throw new ArgumentException("Domain name is required.", nameof(domain));

            await _repo.AddDomainAsync(domain);
            return domain;
        }

        public async Task UpdateAsync(int id, Domain domain)
        {
            if (domain == null) throw new ArgumentNullException(nameof(domain));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(domain.Name)) throw new ArgumentException("Domain name is required.", nameof(domain));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Domain with id {id} not found.");

            existing.Name = domain.Name;
            existing.Description = domain.Description;
            existing.AllowEntry = domain.AllowEntry;
            existing.AllowExit = domain.AllowExit;
            existing.LocationId = domain.LocationId;

            await _repo.UpdateDomainAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Domain with id {id} not found.");

            await _repo.DeleteDomainAsync(id);
        }
    }
}
