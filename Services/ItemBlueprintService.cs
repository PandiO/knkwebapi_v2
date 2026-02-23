using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class ItemBlueprintService : IItemBlueprintService
    {
        private readonly IItemBlueprintRepository _repo;
        private readonly IMinecraftMaterialRefRepository _materialRepo;
        private readonly IMinecraftMaterialCatalogService _materialCatalog;
        private readonly IEnchantmentDefinitionRepository _enchantmentRepo;
        private readonly IMapper _mapper;

        public ItemBlueprintService(
            IItemBlueprintRepository repo,
            IMinecraftMaterialRefRepository materialRepo,
            IMinecraftMaterialCatalogService materialCatalog,
            IEnchantmentDefinitionRepository enchantmentRepo,
            IMapper mapper)
        {
            _repo = repo;
            _materialRepo = materialRepo;
            _materialCatalog = materialCatalog;
            _enchantmentRepo = enchantmentRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ItemBlueprintReadDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ItemBlueprintReadDto>>(items);
        }

        public async Task<ItemBlueprintReadDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<ItemBlueprintReadDto>(entity);
        }

        public async Task<ItemBlueprintReadDto> CreateAsync(ItemBlueprintCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.", nameof(dto));

            dto.Description ??= string.Empty;
            dto.DefaultDisplayName ??= string.Empty;
            dto.DefaultDisplayDescription ??= string.Empty;

            // Ensure icon material ref exists
            var iconMaterialRefId = await EnsureIconMaterialRefAsync(dto.IconMaterialRefId, dto.IconNamespaceKey);

            var entity = _mapper.Map<ItemBlueprint>(dto);
            entity.IconMaterialRefId = iconMaterialRefId;

            // Handle default enchantments (Many-to-Many)
            if (dto.DefaultEnchantments != null && dto.DefaultEnchantments.Any())
            {
                entity.DefaultEnchantments = new List<ItemBlueprintDefaultEnchantment>();
                foreach (var enchDto in dto.DefaultEnchantments)
                {
                    // Validate enchantment definition exists
                    var enchDef = await _enchantmentRepo.GetByIdAsync(enchDto.EnchantmentDefinitionId);
                    if (enchDef == null)
                        throw new ArgumentException($"EnchantmentDefinition with id {enchDto.EnchantmentDefinitionId} not found.");

                    entity.DefaultEnchantments.Add(new ItemBlueprintDefaultEnchantment
                    {
                        EnchantmentDefinitionId = enchDto.EnchantmentDefinitionId,
                        Level = enchDto.Level
                    });
                }
            }

            await _repo.AddAsync(entity);
            return _mapper.Map<ItemBlueprintReadDto>(entity);
        }

        public async Task UpdateAsync(int id, ItemBlueprintUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.", nameof(dto));

            dto.Description ??= string.Empty;
            dto.DefaultDisplayName ??= string.Empty;
            dto.DefaultDisplayDescription ??= string.Empty;

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"ItemBlueprint with id {id} not found.");

            // Ensure icon material ref exists
            var iconMaterialRefId = await EnsureIconMaterialRefAsync(dto.IconMaterialRefId, dto.IconNamespaceKey, existing.IconMaterialRefId);

            // Update scalar properties
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IconMaterialRefId = iconMaterialRefId;
            existing.DefaultDisplayName = dto.DefaultDisplayName;
            existing.DefaultDisplayDescription = dto.DefaultDisplayDescription;
            existing.DefaultQuantity = dto.DefaultQuantity;
            existing.MaxStackSize = dto.MaxStackSize;

            // Update default enchantments (cascade M2M)
            existing.DefaultEnchantments.Clear();
            if (dto.DefaultEnchantments != null && dto.DefaultEnchantments.Any())
            {
                foreach (var enchDto in dto.DefaultEnchantments)
                {
                    var enchDef = await _enchantmentRepo.GetByIdAsync(enchDto.EnchantmentDefinitionId);
                    if (enchDef == null)
                        throw new ArgumentException($"EnchantmentDefinition with id {enchDto.EnchantmentDefinitionId} not found.");

                    existing.DefaultEnchantments.Add(new ItemBlueprintDefaultEnchantment
                    {
                        ItemBlueprintId = existing.Id,
                        EnchantmentDefinitionId = enchDto.EnchantmentDefinitionId,
                        Level = enchDto.Level
                    });
                }
            }

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"ItemBlueprint with id {id} not found.");

            await _repo.DeleteAsync(id);
        }

        public async Task<PagedResultDto<ItemBlueprintListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            return _mapper.Map<PagedResultDto<ItemBlueprintListDto>>(result);
        }

        private async Task<int?> EnsureIconMaterialRefAsync(int? explicitId, string? namespaceKey, int? currentId = null)
        {
            // If explicit ID provided, validate and return
            if (explicitId.HasValue && explicitId > 0)
            {
                var material = await _materialRepo.GetByIdAsync(explicitId.Value);
                if (material == null)
                    throw new ArgumentException($"MinecraftMaterialRef with id {explicitId} not found.");
                return explicitId;
            }

            // If no namespace key, keep existing or null
            if (string.IsNullOrWhiteSpace(namespaceKey))
                return currentId;

            var key = namespaceKey.Trim();

            // Check if material already exists
            var existing = await _materialRepo.GetByNamespaceKeyAsync(key);
            if (existing != null)
                return existing.Id;

            // Look up catalog info
            var catalogEntry = _materialCatalog.Search(key, null)
                .FirstOrDefault(c => c.NamespaceKey.Equals(key, StringComparison.OrdinalIgnoreCase));

            var newMaterial = new MinecraftMaterialRef
            {
                NamespaceKey = key,
                Category = catalogEntry?.Category ?? "Uncategorized",
                LegacyName = catalogEntry?.LegacyName,
                IconUrl = catalogEntry?.IconUrl
            };

            await _materialRepo.AddAsync(newMaterial);
            return newMaterial.Id;
        }
    }
}
