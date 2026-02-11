using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    /// <summary>
    /// Repository implementation for FieldValidationRule data access.
    /// Handles CRUD operations and dependency analysis.
    /// </summary>
    public class FieldValidationRuleRepository : IFieldValidationRuleRepository
    {
        private readonly KnKDbContext _context;

        public FieldValidationRuleRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<FieldValidationRule?> GetByIdAsync(int id)
        {
            return await _context.FieldValidationRules
                .Include(r => r.FormField)
                .Include(r => r.DependsOnField)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<FieldValidationRule>> GetByFormFieldIdAsync(int formFieldId)
        {
            return await _context.FieldValidationRules
                .Include(r => r.FormField)
                .Include(r => r.DependsOnField)
                .Where(r => r.FormFieldId == formFieldId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldValidationRule>> GetByFieldIdsAsync(IEnumerable<int> formFieldIds)
        {
            var fieldIds = formFieldIds?.ToList() ?? new List<int>();
            if (fieldIds.Count == 0)
            {
                return new List<FieldValidationRule>();
            }

            return await _context.FieldValidationRules
                .Include(r => r.FormField)
                .Include(r => r.DependsOnField)
                .Where(r => fieldIds.Contains(r.FormFieldId))
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldValidationRule>> GetByFormConfigurationIdAsync(int formConfigurationId)
        {
            // Get all fields in the configuration first
            var fieldIds = await _context.FormFields
                .Where(f => f.FormStep != null && f.FormStep.FormConfigurationId == formConfigurationId)
                .Select(f => f.Id)
                .ToListAsync();

            // Get all validation rules for those fields
            return await _context.FieldValidationRules
                .Include(r => r.FormField)
                .Include(r => r.DependsOnField)
                .Where(r => fieldIds.Contains(r.FormFieldId))
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldValidationRule>> GetRulesDependingOnFieldAsync(int fieldId)
        {
            return await _context.FieldValidationRules
                .Include(r => r.FormField)
                .Include(r => r.DependsOnField)
                .Where(r => r.DependsOnFieldId == fieldId)
                .ToListAsync();
        }

        public async Task<bool> HasCircularDependencyAsync(int fieldId, int dependsOnFieldId)
        {
            var visited = new HashSet<int>();
            var toCheck = new Queue<int>();
            toCheck.Enqueue(dependsOnFieldId);

            while (toCheck.Count > 0)
            {
                var currentFieldId = toCheck.Dequeue();
                
                // If we've reached the original field, we have a circular dependency
                if (currentFieldId == fieldId)
                {
                    return true;
                }

                // Skip if already visited to prevent infinite loops
                if (visited.Contains(currentFieldId))
                {
                    continue;
                }

                visited.Add(currentFieldId);

                // Get all fields that the current field depends on
                var dependencies = await _context.FieldValidationRules
                    .Where(r => r.FormFieldId == currentFieldId && r.DependsOnFieldId.HasValue)
                    .Select(r => r.DependsOnFieldId!.Value)
                    .Distinct()
                    .ToListAsync();

                // Add them to the queue for checking
                foreach (var dep in dependencies)
                {
                    toCheck.Enqueue(dep);
                }
            }

            return false;
        }

        public async Task<FieldValidationRule> CreateAsync(FieldValidationRule rule)
        {
            await _context.FieldValidationRules.AddAsync(rule);
            await _context.SaveChangesAsync();
            
            // Reload with navigation properties
            return (await GetByIdAsync(rule.Id))!;
        }

        public async Task UpdateAsync(FieldValidationRule rule)
        {
            _context.FieldValidationRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rule = await _context.FieldValidationRules.FindAsync(id);
            if (rule != null)
            {
                _context.FieldValidationRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
    }
}
