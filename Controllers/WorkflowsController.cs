using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _service;

        public WorkflowsController(IWorkflowService service)
        {
            _service = service;
        }

        // Health endpoint to confirm orchestration is wired
        [HttpGet("health")] 
        public IActionResult Health() => Ok(new { status = "ok", component = "workflow" });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkflowSessionCreateDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.StartSessionAsync(dto);
            return CreatedAtRoute("GetWorkflowById", new { id = created.Id }, created);
        }

        [HttpGet("{id:int}", Name = "GetWorkflowById")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetSessionAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("by-guid/{guid:guid}")]
        public async Task<IActionResult> GetByGuid(Guid guid)
        {
            var item = await _service.GetSessionByGuidAsync(guid);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("{id:int}/resume")]
        public async Task<IActionResult> Resume(int id)
        {
            var item = await _service.ResumeSessionAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("{id:int}/progress")]
        public async Task<IActionResult> GetProgress(int id)
        {
            var steps = await _service.GetProgressAsync(id);
            return Ok(steps);
        }

        [HttpPost("{id:int}/steps/{stepKey}/complete")]
        public async Task<IActionResult> CompleteStep(int id, string stepKey, [FromQuery] int? stepIndex = null)
        {
            try
            {
                var step = await _service.SetStepCompletedAsync(id, stepKey, stepIndex);
                return Ok(step);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteSessionAsync(id);
            return NoContent();
        }
    }
}
