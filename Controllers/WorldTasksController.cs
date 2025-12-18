using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Services.Interfaces;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldTasksController : ControllerBase
    {
        private readonly IWorldTaskService _service;

        public WorldTasksController(IWorldTaskService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorldTaskCreateDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.CreateAsync(dto);
            return CreatedAtRoute("GetWorldTaskById", new { id = created.Id }, created);
        }

        [HttpGet("{id:int}", Name = "GetWorldTaskById")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // List by session
        [HttpGet("session/{sessionId:int}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var items = await _service.GetBySessionAsync(sessionId);
            return Ok(items);
        }

        // List by user (paginated)
        [HttpPost("user/{userId:int}/search")]
        public async Task<ActionResult<PagedResultDto<WorldTaskReadDto>>> GetByUser(int userId, [FromBody] PagedQueryDto query)
        {
            var result = await _service.GetByUserAsync(userId, query);
            return Ok(result);
        }

        // Search/paginate tasks
        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<WorldTaskReadDto>>> Search([FromBody] PagedQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(result);
        }

        // Status transitions and payload updates
        public class UpdateStatusRequest { public string Status { get; set; } = null!; public string? PayloadJson { get; set; } }

        [HttpPost("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Status)) return BadRequest();
            try
            {
                var updated = await _service.UpdateStatusAsync(id, body.Status, body.PayloadJson);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
