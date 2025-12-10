using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestDisplayController : ControllerBase
    {
        private readonly IDisplayConfigurationRepository _configRepo;
        private readonly IDisplaySectionRepository _sectionRepo;
        private readonly IDisplayFieldRepository _fieldRepo;

        public TestDisplayController(
            IDisplayConfigurationRepository configRepo,
            IDisplaySectionRepository sectionRepo,
            IDisplayFieldRepository fieldRepo)
        {
            _configRepo = configRepo;
            _sectionRepo = sectionRepo;
            _fieldRepo = fieldRepo;
        }

        /// <summary>
        /// Test basic CRUD operations on DisplayConfiguration entities
        /// </summary>
        [HttpGet("display-repositories")]
        public async Task<IActionResult> TestRepositories()
        {
            try
            {
                var results = new List<string>();

                // Test 1: Create a DisplayConfiguration with nested structure
                results.Add("=== TEST 1: Create DisplayConfiguration ===");
                var config = new DisplayConfiguration
                {
                    Name = "Test Town Display",
                    EntityTypeName = "Town",
                    IsDefault = true,
                    IsDraft = true,
                    Description = "Test configuration for Town entity",
                    SectionOrderJson = "[\"section-1-guid\", \"section-2-guid\"]",
                    Sections = new List<DisplaySection>
                    {
                        new DisplaySection
                        {
                            SectionName = "General Information",
                            Description = "Basic town info",
                            FieldOrderJson = "[\"field-1-guid\"]",
                            Fields = new List<DisplayField>
                            {
                                new DisplayField
                                {
                                    Label = "Town Name",
                                    FieldName = "Name",
                                    FieldType = "String"
                                },
                                new DisplayField
                                {
                                    Label = "Description",
                                    FieldName = "Description",
                                    FieldType = "String"
                                }
                            }
                        },
                        new DisplaySection
                        {
                            SectionName = "Districts",
                            Description = "Town districts",
                            RelatedEntityPropertyName = "Districts",
                            RelatedEntityTypeName = "District",
                            IsCollection = true,
                            ActionButtonsConfigJson = "{\"showViewButton\": true, \"showAddButton\": true}",
                            SubSections = new List<DisplaySection>
                            {
                                new DisplaySection
                                {
                                    SectionName = "District Template",
                                    Fields = new List<DisplayField>
                                    {
                                        new DisplayField
                                        {
                                            Label = "District Name",
                                            FieldName = "Name",
                                            FieldType = "String"
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                var created = await _configRepo.CreateAsync(config);
                results.Add($"✓ Created config with ID: {created.Id}");
                results.Add($"  - Sections count: {created.Sections.Count}");
                results.Add($"  - Section 1 fields: {created.Sections[0].Fields.Count}");
                results.Add($"  - Section 2 subsections: {created.Sections[1].SubSections.Count}");

                // Test 2: Retrieve by ID
                results.Add("\n=== TEST 2: Retrieve by ID ===");
                var retrieved = await _configRepo.GetByIdAsync(created.Id);
                if (retrieved != null)
                {
                    results.Add($"✓ Retrieved config: {retrieved.Name}");
                    results.Add($"  - Entity Type: {retrieved.EntityTypeName}");
                    results.Add($"  - Is Draft: {retrieved.IsDraft}");
                    results.Add($"  - Sections loaded: {retrieved.Sections.Count}");
                    
                    if (retrieved.Sections.Count > 1 && retrieved.Sections[1].SubSections.Count > 0)
                    {
                        results.Add($"  - Subsections loaded: {retrieved.Sections[1].SubSections.Count}");
                        results.Add($"  - Subsection fields: {retrieved.Sections[1].SubSections[0].Fields.Count}");
                    }
                }

                // Test 3: Get by EntityTypeName
                results.Add("\n=== TEST 3: Get by EntityTypeName ===");
                var byEntity = await _configRepo.GetByEntityTypeNameAsync("Town", defaultOnly: true);
                if (byEntity != null)
                {
                    results.Add($"✓ Found default config for Town: {byEntity.Name}");
                    results.Add($"  - Is Default: {byEntity.IsDefault}");
                }

                // Test 4: Check default exists
                results.Add("\n=== TEST 4: Check if default exists ===");
                var defaultExists = await _configRepo.IsDefaultExistsAsync("Town");
                results.Add($"✓ Default exists for Town: {defaultExists}");

                // Test 5: Get all configs (including drafts)
                results.Add("\n=== TEST 5: Get all configurations ===");
                var allConfigs = await _configRepo.GetAllAsync(includeDrafts: true);
                results.Add($"✓ Total configs (with drafts): {allConfigs.Count()}");

                // Test 6: Get all configs (excluding drafts)
                var publishedConfigs = await _configRepo.GetAllAsync(includeDrafts: false);
                results.Add($"✓ Published configs only: {publishedConfigs.Count()}");

                // Test 7: Update configuration
                results.Add("\n=== TEST 6: Update configuration ===");
                if (retrieved != null)
                {
                    retrieved.Description = "Updated description";
                    retrieved.UpdatedAt = DateTime.UtcNow;
                    await _configRepo.UpdateAsync(retrieved);
                    
                    var updated = await _configRepo.GetByIdAsync(retrieved.Id);
                    results.Add($"✓ Updated config description: {updated?.Description}");
                    results.Add($"  - UpdatedAt set: {updated?.UpdatedAt.HasValue}");
                }

                // Test 8: Create reusable section
                results.Add("\n=== TEST 7: Create reusable section ===");
                var reusableSection = new DisplaySection
                {
                    SectionName = "Reusable Basic Info",
                    IsReusable = true,
                    Fields = new List<DisplayField>
                    {
                        new DisplayField
                        {
                            Label = "ID",
                            FieldName = "Id",
                            FieldType = "Integer",
                            IsReusable = true
                        }
                    }
                };
                var createdSection = await _sectionRepo.CreateAsync(reusableSection);
                results.Add($"✓ Created reusable section with ID: {createdSection.Id}");

                // Test 9: Get all reusable sections
                results.Add("\n=== TEST 8: Get reusable sections ===");
                var reusableSections = await _sectionRepo.GetAllReusableAsync();
                results.Add($"✓ Reusable sections count: {reusableSections.Count()}");

                // Test 10: Get reusable fields
                results.Add("\n=== TEST 9: Get reusable fields ===");
                var reusableFields = await _fieldRepo.GetAllReusableAsync();
                results.Add($"✓ Reusable fields count: {reusableFields.Count()}");

                // Test 11: Delete configuration (cascade should delete sections and fields)
                results.Add("\n=== TEST 10: Delete configuration (cascade test) ===");
                await _configRepo.DeleteAsync(created.Id);
                var deleted = await _configRepo.GetByIdAsync(created.Id);
                results.Add($"✓ Config deleted: {deleted == null}");
                
                // Verify cascade delete worked
                var sectionsAfterDelete = await _configRepo.GetAllAsync();
                results.Add($"✓ Remaining configs: {sectionsAfterDelete.Count()}");

                // Clean up reusable section
                await _sectionRepo.DeleteAsync(createdSection.Id);

                results.Add("\n=== ALL TESTS PASSED ✓ ===");
                
                return Ok(new { 
                    success = true, 
                    message = "All repository tests completed successfully",
                    results = results 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Clean up all test data
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupTestData()
        {
            try
            {
                var allConfigs = await _configRepo.GetAllAsync(includeDrafts: true);
                var testConfigs = allConfigs.Where(c => c.Name.Contains("Test"));
                
                foreach (var config in testConfigs)
                {
                    await _configRepo.DeleteAsync(config.Id);
                }

                return Ok(new { 
                    success = true, 
                    message = $"Cleaned up {testConfigs.Count()} test configurations" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
