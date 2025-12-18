using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

namespace knkwebapi_v2.Repositories
{
    public class FormConfigurationRepository : IFormConfigurationRepository
    {
        private readonly KnKDbContext _context;

        public FormConfigurationRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormConfiguration>> GetAllAsync()
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .ToListAsync();
        }

        public async Task<FormConfiguration?> GetByIdAsync(int id)
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .FirstOrDefaultAsync(fc => fc.Id == id);
        }

        public async Task<IEnumerable<FormConfiguration>> GetAllByEntityTypeNameAsync(string entityName, bool defaultOnly = false)
        {
            var query = _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .Where(fc => fc.EntityTypeName == entityName);

            if (defaultOnly)
            {
                query = query.Where(fc => fc.IsDefault);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<FormConfiguration>> GetAllByEntityTypeNameAllAsync(string entityName)
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .Where(fc => fc.EntityTypeName == entityName)
                .ToListAsync();
        }

        public async Task<FormConfiguration?> GetDefaultByEntityTypeNameAsync(string entityName)
        {
            return await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .FirstOrDefaultAsync(fc => fc.EntityTypeName == entityName && fc.IsDefault);
        }

        public async Task AddAsync(FormConfiguration config)
        {
            await _context.FormConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FormConfiguration config)
        {
            // Load existing tracked graph
            var existing = await _context.FormConfigurations
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Validations)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.StepConditions)
                .Include(fc => fc.Steps)
                    .ThenInclude(s => s.ChildFormSteps)
                        .ThenInclude(cs => cs.Fields)
                            .ThenInclude(f => f.Validations)
                .FirstOrDefaultAsync(fc => fc.Id == config.Id);

            if (existing == null)
            {
                return; // or throw if desired
            }

            // Update scalar properties
            existing.Name = config.Name;
            existing.EntityTypeName = config.EntityTypeName;
            existing.Description = config.Description;
            existing.IsDefault = config.IsDefault;
            existing.StepOrderJson = config.StepOrderJson;
            existing.UpdatedAt = config.UpdatedAt;

            // ----- Merge Steps -----
            var incomingStepsById = config.Steps.Where(s => s.Id != 0).ToDictionary(s => s.Id);
            // Remove steps not present
            foreach (var removeStep in existing.Steps.Where(s => !incomingStepsById.ContainsKey(s.Id)).ToList())
            {
                _context.FormSteps.Remove(removeStep);
            }
            // Process incoming steps
            foreach (var step in config.Steps)
            {
                var match = step.Id != 0 ? existing.Steps.FirstOrDefault(s => s.Id == step.Id) : null;
                if (match == null)
                {
                    // New step
                    var newStep = new FormStep
                    {
                        StepName = step.StepName,
                        Description = step.Description,
                        IsReusable = step.IsReusable,
                        SourceStepId = step.SourceStepId,
                        FieldOrderJson = step.FieldOrderJson,
                        IsManyToManyRelationship = step.IsManyToManyRelationship,
                        RelatedEntityPropertyName = step.RelatedEntityPropertyName,
                        JoinEntityType = step.JoinEntityType,
                        ParentStepId = step.ParentStepId,
                        FormConfigurationId = existing.Id,
                        // StepGuid stays auto-generated
                        CreatedAt = DateTime.UtcNow
                    };
                    existing.Steps.Add(newStep);
                    match = newStep;
                }
                else
                {
                    // Update existing step scalars
                    match.StepName = step.StepName;
                    match.Description = step.Description;
                    match.IsReusable = step.IsReusable;
                    match.SourceStepId = step.SourceStepId;
                    match.FieldOrderJson = step.FieldOrderJson;
                    match.IsManyToManyRelationship = step.IsManyToManyRelationship;
                    match.RelatedEntityPropertyName = step.RelatedEntityPropertyName;
                    match.JoinEntityType = step.JoinEntityType;
                    match.ParentStepId = step.ParentStepId;
                }

                // ----- Merge StepConditions -----
                var incomingConditionsIds = step.StepConditions.Where(c => c.Id != 0).Select(c => c.Id).ToHashSet();
                foreach (var removeCond in match.StepConditions.Where(c => !incomingConditionsIds.Contains(c.Id)).ToList())
                    _context.StepConditions.Remove(removeCond);

                foreach (var cond in step.StepConditions)
                {
                    var condMatch = cond.Id != 0 ? match.StepConditions.FirstOrDefault(c => c.Id == cond.Id) : null;
                    if (condMatch == null)
                    {
                        match.StepConditions.Add(new StepCondition
                        {
                            ConditionType = cond.ConditionType,
                            ConditionLogicJson = cond.ConditionLogicJson,
                            ErrorMessage = cond.ErrorMessage,
                            FormStepId = match.Id
                        });
                    }
                    else
                    {
                        condMatch.ConditionType = cond.ConditionType;
                        condMatch.ConditionLogicJson = cond.ConditionLogicJson;
                        condMatch.ErrorMessage = cond.ErrorMessage;
                    }
                }

                // ----- Merge Fields -----
                var incomingFieldIds = step.Fields.Where(f => f.Id != 0).Select(f => f.Id).ToHashSet();
                foreach (var removeField in match.Fields.Where(f => !incomingFieldIds.Contains(f.Id)).ToList())
                    _context.FormFields.Remove(removeField);

                foreach (var field in step.Fields)
                {
                    var fieldMatch = field.Id != 0 ? match.Fields.FirstOrDefault(f => f.Id == field.Id) : null;
                    if (fieldMatch == null)
                    {
                        var newField = new FormField
                        {
                            FieldName = field.FieldName,
                            Label = field.Label,
                            Placeholder = field.Placeholder,
                            Description = field.Description,
                            FieldType = field.FieldType,
                            ElementType = field.ElementType,
                            ObjectType = field.ObjectType,
                            EnumType = field.EnumType,
                            DefaultValue = field.DefaultValue,
                            Required = field.Required,
                            ReadOnly = field.ReadOnly,
                            IsReusable = field.IsReusable,
                            SourceFieldId = field.SourceFieldId,
                            DependsOnFieldId = field.DependsOnFieldId,
                            DependencyConditionJson = field.DependencyConditionJson,
                            SubConfigurationId = field.SubConfigurationId,
                            FormStepId = match.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        match.Fields.Add(newField);
                        fieldMatch = newField;
                    }
                    else
                    {
                        fieldMatch.FieldName = field.FieldName;
                        fieldMatch.Label = field.Label;
                        fieldMatch.Placeholder = field.Placeholder;
                        fieldMatch.Description = field.Description;
                        fieldMatch.FieldType = field.FieldType;
                        fieldMatch.ElementType = field.ElementType;
                        fieldMatch.ObjectType = field.ObjectType;
                        fieldMatch.EnumType = field.EnumType;
                        fieldMatch.DefaultValue = field.DefaultValue;
                        fieldMatch.Required = field.Required;
                        fieldMatch.ReadOnly = field.ReadOnly;
                        fieldMatch.IsReusable = field.IsReusable;
                        fieldMatch.SourceFieldId = field.SourceFieldId;
                        fieldMatch.DependsOnFieldId = field.DependsOnFieldId;
                        fieldMatch.DependencyConditionJson = field.DependencyConditionJson;
                        fieldMatch.SubConfigurationId = field.SubConfigurationId;
                    }

                    // ----- Merge Validations -----
                    var incomingValidationIds = field.Validations.Where(v => v.Id != 0).Select(v => v.Id).ToHashSet();
                    foreach (var removeVal in fieldMatch.Validations.Where(v => !incomingValidationIds.Contains(v.Id)).ToList())
                        _context.FieldValidations.Remove(removeVal);

                    foreach (var val in field.Validations)
                    {
                        var valMatch = val.Id != 0 ? fieldMatch.Validations.FirstOrDefault(v => v.Id == val.Id) : null;
                        if (valMatch == null)
                        {
                            fieldMatch.Validations.Add(new FieldValidation
                            {
                                Type = val.Type,
                                ParametersJson = val.ParametersJson,
                                ErrorMessage = val.ErrorMessage,
                                FormFieldId = fieldMatch.Id
                            });
                        }
                        else
                        {
                            valMatch.Type = val.Type;
                            valMatch.ParametersJson = val.ParametersJson;
                            valMatch.ErrorMessage = val.ErrorMessage;
                        }
                    }
                }

                // ----- Merge ChildFormSteps (for M2M relationships) -----
                var incomingChildStepIds = step.ChildFormSteps.Where(cs => cs.Id != 0).Select(cs => cs.Id).ToHashSet();
                foreach (var removeChild in match.ChildFormSteps.Where(cs => !incomingChildStepIds.Contains(cs.Id)).ToList())
                    _context.FormSteps.Remove(removeChild);

                foreach (var childStep in step.ChildFormSteps)
                {
                    var childMatch = childStep.Id != 0 ? match.ChildFormSteps.FirstOrDefault(cs => cs.Id == childStep.Id) : null;
                    if (childMatch == null)
                    {
                        var newChild = new FormStep
                        {
                            StepName = childStep.StepName,
                            Description = childStep.Description,
                            IsReusable = childStep.IsReusable,
                            SourceStepId = childStep.SourceStepId,
                            FieldOrderJson = childStep.FieldOrderJson,
                            ParentStepId = match.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        match.ChildFormSteps.Add(newChild);
                        childMatch = newChild;
                    }
                    else
                    {
                        childMatch.StepName = childStep.StepName;
                        childMatch.Description = childStep.Description;
                        childMatch.IsReusable = childStep.IsReusable;
                        childMatch.SourceStepId = childStep.SourceStepId;
                        childMatch.FieldOrderJson = childStep.FieldOrderJson;
                    }

                    // Merge child step fields
                    var incomingChildFieldIds = childStep.Fields.Where(f => f.Id != 0).Select(f => f.Id).ToHashSet();
                    foreach (var removeField in childMatch.Fields.Where(f => !incomingChildFieldIds.Contains(f.Id)).ToList())
                        _context.FormFields.Remove(removeField);

                    foreach (var field in childStep.Fields)
                    {
                        var fieldMatch = field.Id != 0 ? childMatch.Fields.FirstOrDefault(f => f.Id == field.Id) : null;
                        if (fieldMatch == null)
                        {
                            var newField = new FormField
                            {
                                FieldName = field.FieldName,
                                Label = field.Label,
                                Placeholder = field.Placeholder,
                                Description = field.Description,
                                FieldType = field.FieldType,
                                ElementType = field.ElementType,
                                ObjectType = field.ObjectType,
                                EnumType = field.EnumType,
                                DefaultValue = field.DefaultValue,
                                Required = field.Required,
                                ReadOnly = field.ReadOnly,
                                IsReusable = field.IsReusable,
                                SourceFieldId = field.SourceFieldId,
                                DependsOnFieldId = field.DependsOnFieldId,
                                DependencyConditionJson = field.DependencyConditionJson,
                                SubConfigurationId = field.SubConfigurationId,
                                FormStepId = childMatch.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            childMatch.Fields.Add(newField);
                            fieldMatch = newField;
                        }
                        else
                        {
                            fieldMatch.FieldName = field.FieldName;
                            fieldMatch.Label = field.Label;
                            fieldMatch.Placeholder = field.Placeholder;
                            fieldMatch.Description = field.Description;
                            fieldMatch.FieldType = field.FieldType;
                            fieldMatch.ElementType = field.ElementType;
                            fieldMatch.ObjectType = field.ObjectType;
                            fieldMatch.EnumType = field.EnumType;
                            fieldMatch.DefaultValue = field.DefaultValue;
                            fieldMatch.Required = field.Required;
                            fieldMatch.ReadOnly = field.ReadOnly;
                            fieldMatch.IsReusable = field.IsReusable;
                            fieldMatch.SourceFieldId = field.SourceFieldId;
                            fieldMatch.DependsOnFieldId = field.DependsOnFieldId;
                            fieldMatch.DependencyConditionJson = field.DependencyConditionJson;
                            fieldMatch.SubConfigurationId = field.SubConfigurationId;
                        }

                        // Merge child field validations
                        var incomingValidationIds = field.Validations.Where(v => v.Id != 0).Select(v => v.Id).ToHashSet();
                        foreach (var removeVal in fieldMatch.Validations.Where(v => !incomingValidationIds.Contains(v.Id)).ToList())
                            _context.FieldValidations.Remove(removeVal);

                        foreach (var val in field.Validations)
                        {
                            var valMatch = val.Id != 0 ? fieldMatch.Validations.FirstOrDefault(v => v.Id == val.Id) : null;
                            if (valMatch == null)
                            {
                                fieldMatch.Validations.Add(new FieldValidation
                                {
                                    Type = val.Type,
                                    ParametersJson = val.ParametersJson,
                                    ErrorMessage = val.ErrorMessage,
                                    FormFieldId = fieldMatch.Id
                                });
                            }
                            else
                            {
                                valMatch.Type = val.Type;
                                valMatch.ParametersJson = val.ParametersJson;
                                valMatch.ErrorMessage = val.ErrorMessage;
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.FormConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.FormConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _context.FormConfigurations
                .Select(fc => fc.EntityTypeName)
                .Distinct()
                .ToListAsync();
        }
    }
}

