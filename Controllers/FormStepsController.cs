using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/form-steps")]
    public class FormStepsController : ControllerBase
    {
        private readonly IFormStepService _service;

        public FormStepsController(IFormStepService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReusable()
        {
            var steps = await _service.GetAllReusableAsync();
            return Ok(steps);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var step = await _service.GetByIdAsync(id);
            if (step == null) return NotFound();
            return Ok(step);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormStep step)
        {
            if (step == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(step);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormStep step)
        {
            if (step == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, step);
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
