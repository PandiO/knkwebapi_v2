using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Services;

public class MinecraftEnchantmentRefService : IMinecraftEnchantmentRefService
{
    private readonly IMinecraftEnchantmentRefRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMinecraftEnchantmentCatalogService _catalog;
    private readonly ILogger<MinecraftEnchantmentRefService> _logger;

    public MinecraftEnchantmentRefService(
        IMinecraftEnchantmentRefRepository repo,
        IMapper mapper,
        IMinecraftEnchantmentCatalogService catalog,
        ILogger<MinecraftEnchantmentRefService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<IEnumerable<MinecraftEnchantmentRefDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<MinecraftEnchantmentRefDto>>(items);
    }

    public async Task<MinecraftEnchantmentRefDto?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        var entity = await _repo.GetByIdAsync(id);
        return _mapper.Map<MinecraftEnchantmentRefDto>(entity);
    }

    public async Task<MinecraftEnchantmentRefDto> CreateAsync(MinecraftEnchantmentRefCreateDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.NamespaceKey))
            throw new ArgumentException("NamespaceKey is required.", nameof(dto));

        var entity = _mapper.Map<MinecraftEnchantmentRef>(dto);
        entity.IsCustom = false; // Always false for catalog entries in current implementation
        
        await _repo.AddAsync(entity);
        return _mapper.Map<MinecraftEnchantmentRefDto>(entity);
    }

    public async Task<MinecraftEnchantmentRefDto> GetOrCreateAsync(string namespaceKey, string? category = null, string? legacyName = null)
    {
        if (string.IsNullOrWhiteSpace(namespaceKey))
            throw new ArgumentException("NamespaceKey is required.", nameof(namespaceKey));

        // Check if already persisted
        var existing = await _repo.GetByNamespaceKeyAsync(namespaceKey);
        if (existing != null)
        {
            _logger.LogDebug("Enchantment {NamespaceKey} already persisted with Id {Id}", namespaceKey, existing.Id);
            return _mapper.Map<MinecraftEnchantmentRefDto>(existing);
        }

        // Lookup in catalog
        var catalogEntry = _catalog.GetByNamespaceKey(namespaceKey);
        if (catalogEntry == null)
        {
            _logger.LogWarning("Enchantment {NamespaceKey} not found in catalog", namespaceKey);
            throw new ArgumentException($"Enchantment '{namespaceKey}' not found in catalog");
        }

        // Upsert: try to create
        var entity = new MinecraftEnchantmentRef
        {
            NamespaceKey = namespaceKey,
            LegacyName = legacyName ?? catalogEntry.LegacyName,
            Category = category ?? catalogEntry.Category,
            IconUrl = catalogEntry.IconUrl,
            MaxLevel = catalogEntry.MaxLevel,
            DisplayName = catalogEntry.DisplayName,
            IsCustom = false
        };

        try
        {
            await _repo.AddAsync(entity);
            _logger.LogInformation("Persisted new enchantment {NamespaceKey} with Id {Id}", namespaceKey, entity.Id);
            return _mapper.Map<MinecraftEnchantmentRefDto>(entity);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) == true ||
                                           ex.InnerException?.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Race condition: another request created it; fetch and return
            _logger.LogInformation("Enchantment {NamespaceKey} created by another request; fetching...", namespaceKey);
            var raceWinner = await _repo.GetByNamespaceKeyAsync(namespaceKey);
            if (raceWinner != null)
                return _mapper.Map<MinecraftEnchantmentRefDto>(raceWinner);
            throw;
        }
    }

    public async Task UpdateAsync(int id, MinecraftEnchantmentRefUpdateDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"MinecraftEnchantmentRef with id {id} not found.");

        _mapper.Map(dto, existing);
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"MinecraftEnchantmentRef with id {id} not found.");

        await _repo.DeleteAsync(existing);
    }

    public async Task<List<MinecraftHybridEnchantmentOptionDto>> GetHybridAsync(string? search = null, string? category = null, int? take = null)
    {
        var results = new List<MinecraftHybridEnchantmentOptionDto>();

        // Fetch persisted refs from DB
        var persisted = await _repo.GetAllAsync();
        
        // Apply category filter to persisted if provided
        if (!string.IsNullOrWhiteSpace(category))
        {
            persisted = persisted.Where(e => e.Category != null && e.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var persistedDtos = persisted
            .Select(e => new MinecraftHybridEnchantmentOptionDto
            {
                Type = "PERSISTED",
                NamespaceKey = e.NamespaceKey,
                DisplayName = e.DisplayName,
                LegacyName = e.LegacyName,
                Category = e.Category,
                IconUrl = e.IconUrl,
                MaxLevel = e.MaxLevel,
                Id = e.Id,
                IsCustom = e.IsCustom
            })
            .ToList();

        // Fetch catalog entries not yet persisted
        var catalogEntries = _catalog.GetAll()
            .Where(c => !persisted.Any(p => p.NamespaceKey == c.NamespaceKey));

        // Apply category filter to catalog if provided
        if (!string.IsNullOrWhiteSpace(category))
        {
            catalogEntries = catalogEntries.Where(c => c.Category != null && c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var catalogDtos = catalogEntries
            .Select(c => new MinecraftHybridEnchantmentOptionDto
            {
                Type = "CATALOG",
                NamespaceKey = c.NamespaceKey,
                DisplayName = c.DisplayName,
                LegacyName = c.LegacyName,
                Category = c.Category,
                IconUrl = c.IconUrl,
                MaxLevel = c.MaxLevel,
                Id = null,
                IsCustom = false
            })
            .ToList();

        results.AddRange(persistedDtos);
        results.AddRange(catalogDtos);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLowerInvariant();
            results = results.Where(r =>
                r.NamespaceKey.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                (r.DisplayName != null && r.DisplayName.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase)) ||
                (r.LegacyName != null && r.LegacyName.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase)) ||
                (r.Category != null && r.Category.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Apply limit
        if (take.HasValue && take > 0)
            results = results.Take(take.Value).ToList();

        return results;
    }

    public async Task<PagedResultDto<MinecraftEnchantmentRefListDto>> SearchAsync(PagedQueryDto query)
    {
        var all = await _repo.GetAllAsync();

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var lowerSearch = query.SearchTerm.ToLowerInvariant();
            all = all.Where(e =>
                e.NamespaceKey.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                (e.DisplayName != null && e.DisplayName.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase)) ||
                (e.LegacyName != null && e.LegacyName.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Pagination
        var totalCount = all.Count();
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;
        var skip = (pageNumber - 1) * pageSize;

        var items = all
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var listDtos = _mapper.Map<List<MinecraftEnchantmentRefListDto>>(items);

        return new PagedResultDto<MinecraftEnchantmentRefListDto>
        {
            Items = listDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<MinecraftEnchantmentRefListDto>> SearchHybridAsync(PagedQueryDto queryDto)
    {
        if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

        var search = queryDto.SearchTerm;
        var category = queryDto.Filters?.TryGetValue("category", out var cat) == true ? cat?.ToString() : null;
        
        // Get ALL hybrid results (unfiltered) to calculate total count
        var allHybridResults = (await GetHybridAsync(search, category, null)).ToList();
        var totalCount = allHybridResults.Count;
        
        // Apply pagination
        var skip = (queryDto.PageNumber - 1) * queryDto.PageSize;
        var pagedResults = allHybridResults
            .Skip(skip)
            .Take(queryDto.PageSize)
            .ToList();

        return new PagedResultDto<MinecraftEnchantmentRefListDto>
        {
            Items = pagedResults.Select(h => new MinecraftEnchantmentRefListDto
            {
                Id = h.Id ?? 0,
                NamespaceKey = h.NamespaceKey,
                Category = h.Category,
                LegacyName = h.LegacyName,
                DisplayName = h.DisplayName,
                IsPersisted = h.Id.HasValue,
                MaxLevel = h.MaxLevel,
                IconUrl = h.IconUrl
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = queryDto.PageNumber,
            PageSize = pagedResults.Count
        };
    }
}
