using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/field-validation-rules")]
    public class FieldValidationRulesController : ControllerBase
    {
        private readonly IValidationService _service;
        private readonly IPlaceholderResolutionService _placeholderService;

        public FieldValidationRulesController(
            IValidationService service,
            IPlaceholderResolutionService placeholderService)
        {
            _service = service;
            _placeholderService = placeholderService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rule = await _service.GetByIdAsync(id);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

        [HttpGet("by-field/{fieldId:int}")]
        public async Task<IActionResult> GetByFormField(int fieldId)
        {
            var rules = await _service.GetByFormFieldIdAsync(fieldId);
            return Ok(rules);
        }

        [HttpGet("by-configuration/{configId:int}")]
        public async Task<IActionResult> GetByConfiguration(int configId)
        {
            var rules = await _service.GetByFormConfigurationIdAsync(configId);
            return Ok(rules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFieldValidationRuleDto dto)
        {
            if (dto == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFieldValidationRuleDto dto)
        {
            if (dto == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateField([FromBody] ValidateFieldRequestDto request)
        {
            if (request == null) return BadRequest();
            try
            {
                var result = await _service.ValidateFieldAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Validation execution failed", error = ex.Message });
            }
        }

        [HttpGet("health-check/configuration/{configId:int}")]
        public async Task<IActionResult> ValidateConfigurationHealth(int configId)
        {
            try
            {
                var issues = await _service.ValidateConfigurationHealthAsync(configId);
                return Ok(issues);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("health-check/configuration/draft")]
        public async Task<IActionResult> ValidateDraftConfiguration([FromBody] FormConfigurationDto configDto)
        {
            try
            {
                if (configDto == null)
                {
                    return BadRequest(new { message = "Configuration data is required" });
                }

                var issues = await _service.ValidateDraftConfigurationAsync(configDto);
                return Ok(issues);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Draft validation failed", error = ex.Message });
            }
        }

        [HttpPost("resolve-placeholders")]
        public async Task<IActionResult> ResolvePlaceholders([FromBody] ResolvePlaceholdersRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request data is required" });
            }

            if (string.IsNullOrWhiteSpace(request.CurrentEntityType))
            {
                return BadRequest(new { message = "CurrentEntityType is required" });
            }

            if (request.PlaceholderPaths == null || request.PlaceholderPaths.Count == 0)
            {
                return Ok(new ResolvePlaceholdersResponseDto
                {
                    ResolvedPlaceholders = request.CurrentEntityPlaceholders ?? new Dictionary<string, string>()
                });
            }

            try
            {
                var result = await _placeholderService.ResolvePlaceholdersAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Placeholder resolution failed",
                    error = ex.Message
                });
            }
        }
    }
}