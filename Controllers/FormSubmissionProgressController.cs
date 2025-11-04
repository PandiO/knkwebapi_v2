using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/form-submission-progress")]
    public class FormSubmissionProgressController : ControllerBase
    {
        private readonly IFormSubmissionProgressService _service;

        public FormSubmissionProgressController(IFormSubmissionProgressService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var progresses = await _service.GetByUserIdAsync(userId);
            return Ok(progresses);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var progress = await _service.GetByIdAsync(id);
            if (progress == null) return NotFound();
            return Ok(progress);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProgress([FromBody] FormSubmissionProgress progress)
        {
            if (progress == null) return BadRequest();
            try
            {
                var created = await _service.SaveProgressAsync(progress);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] FormSubmissionProgress progress)
        {
            if (progress == null) return BadRequest();
            try
            {
                var updated = await _service.UpdateProgressAsync(id, progress);
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
