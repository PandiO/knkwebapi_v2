using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services;
using Microsoft.EntityFrameworkCore;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateStructuresController : ControllerBase
    {
        private readonly IGateStructureService _service;

        public GateStructuresController(IGateStructureService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}", Name = "GetGateStructureById")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GateStructureDto gateStructureDto)
        {
            if (gateStructureDto == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(gateStructureDto);
                return CreatedAtRoute("GetGateStructureById", new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GateStructureDto gateStructureDto)
        {
            if (gateStructureDto == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, gateStructureDto);
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
            catch (DbUpdateException ex)
            {
                return Conflict(new { code = "DbConstraint", message = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<GateStructureListDto>>> SearchGateStructures([FromBody] PagedQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(result);
        }
    }
}
