using AutoMapper;
using Moq;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services;
using knkwebapi_v2.Services.Interfaces;
using Xunit;

namespace knkwebapi_v2.Tests.Services;

public class EnchantmentDefinitionServiceTests
{
    private readonly Mock<IEnchantmentDefinitionRepository> _enchantmentRepo;
    private readonly Mock<IMinecraftEnchantmentRefRepository> _enchantmentRefRepo;
    private readonly Mock<IMinecraftEnchantmentCatalogService> _catalogService;
    private readonly Mock<IMapper> _mapper;
    private readonly EnchantmentDefinitionService _service;

    public EnchantmentDefinitionServiceTests()
    {
        _enchantmentRepo = new Mock<IEnchantmentDefinitionRepository>();
        _enchantmentRefRepo = new Mock<IMinecraftEnchantmentRefRepository>();
        _catalogService = new Mock<IMinecraftEnchantmentCatalogService>();
        _mapper = new Mock<IMapper>();

        _service = new EnchantmentDefinitionService(
            _enchantmentRepo.Object,
            _enchantmentRefRepo.Object,
            _catalogService.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task CreateAsync_WithAbilityDefinitionAndNonCustom_ThrowsArgumentException()
    {
        var dto = new EnchantmentDefinitionCreateDto
        {
            Key = "minecraft:sharpness",
            DisplayName = "Sharpness",
            IsCustom = false,
            AbilityDefinition = new AbilityDefinitionUpsertDto
            {
                AbilityKey = "combat.sharpness"
            }
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));

        Assert.Contains("IsCustom", ex.Message);
        _enchantmentRepo.Verify(r => r.AddAsync(It.IsAny<EnchantmentDefinition>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithAbilityDefinitionAndNonCustom_ThrowsArgumentException()
    {
        var dto = new EnchantmentDefinitionUpdateDto
        {
            Id = 7,
            Key = "minecraft:sharpness",
            DisplayName = "Sharpness",
            IsCustom = false,
            AbilityDefinition = new AbilityDefinitionUpsertDto
            {
                AbilityKey = "combat.sharpness"
            }
        };

        _enchantmentRepo
            .Setup(r => r.GetByIdAsync(7))
            .ReturnsAsync(new EnchantmentDefinition { Id = 7, Key = "minecraft:sharpness", DisplayName = "Sharpness" });

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(7, dto));

        Assert.Contains("IsCustom", ex.Message);
        _enchantmentRepo.Verify(r => r.UpdateAsync(It.IsAny<EnchantmentDefinition>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithCustomAndAbilityDefinition_PersistsAbilityDefinition()
    {
        var dto = new EnchantmentDefinitionCreateDto
        {
            Key = "knk:lifesteal",
            DisplayName = "Lifesteal",
            IsCustom = true,
            AbilityDefinition = new AbilityDefinitionUpsertDto
            {
                AbilityKey = "combat.lifesteal",
                RuntimeConfigJson = "{\"chance\":0.15}",
                FutureUserAssignmentContract = "UserSkillAssignment:v1"
            }
        };

        var entity = new EnchantmentDefinition
        {
            Id = 0,
            Key = dto.Key,
            DisplayName = dto.DisplayName,
            IsCustom = dto.IsCustom
        };

        var mappedAbility = new AbilityDefinition
        {
            AbilityKey = dto.AbilityDefinition!.AbilityKey,
            RuntimeConfigJson = dto.AbilityDefinition.RuntimeConfigJson,
            FutureUserAssignmentContract = dto.AbilityDefinition.FutureUserAssignmentContract
        };

        _mapper.Setup(m => m.Map<EnchantmentDefinition>(dto)).Returns(entity);
        _mapper.Setup(m => m.Map<AbilityDefinition>(dto.AbilityDefinition)).Returns(mappedAbility);
        _mapper.Setup(m => m.Map<EnchantmentDefinitionReadDto>(It.IsAny<EnchantmentDefinition>()))
            .Returns((EnchantmentDefinition src) => new EnchantmentDefinitionReadDto
            {
                Id = src.Id,
                Key = src.Key,
                DisplayName = src.DisplayName,
                IsCustom = src.IsCustom,
                AbilityDefinition = src.AbilityDefinition == null
                    ? null
                    : new AbilityDefinitionReadDto
                    {
                        AbilityKey = src.AbilityDefinition.AbilityKey,
                        RuntimeConfigJson = src.AbilityDefinition.RuntimeConfigJson,
                        FutureUserAssignmentContract = src.AbilityDefinition.FutureUserAssignmentContract
                    }
            });

        _enchantmentRepo
            .Setup(r => r.AddAsync(It.IsAny<EnchantmentDefinition>()))
            .Callback<EnchantmentDefinition>(added => added.Id = 21)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(21, result.Id);
        Assert.NotNull(result.AbilityDefinition);
        Assert.Equal("combat.lifesteal", result.AbilityDefinition!.AbilityKey);
        _enchantmentRepo.Verify(r => r.AddAsync(It.Is<EnchantmentDefinition>(e => e.AbilityDefinition != null)), Times.Once);
    }
}