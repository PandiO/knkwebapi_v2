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
    public class FormSubmissionProgressController : ControllerBase
    {
        private readonly IFormSubmissionProgressService _service;

        public FormSubmissionProgressController(IFormSubmissionProgressService service)
        {
            _service = service;
        }

        [HttpGet("entity")]
        public async Task<IActionResult> GetByEntityTypeName(string entityTypeName, int? userId, bool? isSummary = false)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
            {
                return BadRequest("Entity type name is required.");
            }
            if (isSummary == true)
            {
                var summaries = await _service.GetSummaryByEntityTypeNameAsync(entityTypeName, userId);
                return Ok(summaries);
            }
            else
            {
                var progresses = await _service.GetByEntityTypeNameAsync(entityTypeName, userId);
                return Ok(progresses);
            }
        }

        [HttpGet("user")]
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
        public async Task<IActionResult> SaveProgress([FromBody] FormSubmissionProgressDto progressDto)
        {
            if (progressDto == null) return BadRequest("Request body is required.");
            try
            {
                var created = await _service.SaveProgressAsync(progressDto);
                return CreatedAtAction(nameof(GetById), new { id = int.Parse(created.Id!) }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (add ILogger if needed)
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] FormSubmissionProgressDto progressDto)
        {
            if (progressDto == null) return BadRequest();
            try
            {
                var updated = await _service.UpdateProgressAsync(id, progressDto);
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
