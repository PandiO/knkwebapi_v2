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

        public FieldValidationRulesController(IValidationService service)
        {
            _service = service;
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
    }
}