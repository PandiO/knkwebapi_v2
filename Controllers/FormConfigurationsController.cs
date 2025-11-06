using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Services;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormConfigurationsController : ControllerBase
    {
        private readonly IFormConfigurationService _service;

        public FormConfigurationsController(IFormConfigurationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _service.GetAllAsync();
            return Ok(configs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var config = await _service.GetByIdAsync(id);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpGet("{entityName}")]
        public async Task<IActionResult> GetByEntityName(string entityName, [FromQuery] bool defaultOnly = false)
        {
            var config = await _service.GetByEntityNameAsync(entityName, defaultOnly);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpGet("{entityName}/all")]
        public async Task<IActionResult> GetByEntityNameAll(string entityName)
        {
            var configs = await _service.GetByEntityNameAllAsync(entityName);
            return Ok(configs);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormConfigurationDto configDto)
        {
            if (configDto == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(configDto);
                return CreatedAtAction(nameof(GetById), new { id = int.Parse(created.Id!) }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormConfigurationDto configDto)
        {
            if (configDto == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, configDto);
                // Return the updated resource instead of 204
                var updated = await _service.GetByIdAsync(id);
                return Ok(updated);
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
