using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/form-configurations")]
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

        [HttpGet("by-entity/{entityName}")]
        public async Task<IActionResult> GetByEntityName(string entityName, [FromQuery] bool defaultOnly = false)
        {
            var config = await _service.GetByEntityNameAsync(entityName, defaultOnly);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpGet("by-entity/{entityName}/all")]
        public async Task<IActionResult> GetByEntityNameAll(string entityName)
        {
            var configs = await _service.GetByEntityNameAllAsync(entityName);
            return Ok(configs);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormConfiguration config)
        {
            if (config == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(config);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormConfiguration config)
        {
            if (config == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, config);
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
