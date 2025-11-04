using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/form-fields")]
    public class FormFieldsController : ControllerBase
    {
        private readonly IFormFieldService _service;

        public FormFieldsController(IFormFieldService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReusable()
        {
            var fields = await _service.GetAllReusableAsync();
            return Ok(fields);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var field = await _service.GetByIdAsync(id);
            if (field == null) return NotFound();
            return Ok(field);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormField field)
        {
            if (field == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(field);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormField field)
        {
            if (field == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, field);
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
