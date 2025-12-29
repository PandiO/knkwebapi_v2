using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;
using knkwebapi_v2.Dtos;

namespace KnKWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}", Name = nameof(GetUserById))]
        public async Task<IActionResult> GetUserById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("uuid/{uuid}", Name = nameof(GetUserSummaryByUuid))]
        public async Task<IActionResult> GetUserSummaryByUuid(string uuid)
        {
            var item = await _service.GetByUuidAsync(uuid);
            if (item == null) return NotFound();
            var dto = new UserSummaryDto
            {
                Id = item.Id,
                Username = item.Username,
                Coins = item.Coins,
                Uuid = item.Uuid
            };
            return Ok(dto);
        }

        [HttpGet("username/{username}", Name = nameof(GetUserSummaryByUsername))]
        public async Task<IActionResult> GetUserSummaryByUsername(string username)
        {
            var item = await _service.GetByUsernameAsync(username);
            if (item == null) return NotFound();
            var dto = new UserSummaryDto
            {
                Id = item.Id,
                Username = item.Username,
                Coins = item.Coins,
                Uuid = item.Uuid
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDto user)
        {
            if (user == null) return BadRequest();
            try
            {
                var created = await _service.CreateAsync(user);
                return CreatedAtRoute(nameof(GetUserById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDto user)
        {
            if (user == null) return BadRequest();
            try
            {
                await _service.UpdateAsync(id, user);
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

        [HttpPut("{id:int}/coins")]
        public async Task<IActionResult> UpdateCoins(int id, [FromBody] int coins)
        {
            try
            {
                await _service.UpdateCoinsAsync(id, coins);
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

        [HttpPut("{uuid}/coins")]
        public async Task<IActionResult> UpdateCoinsByUuid(string uuid, [FromBody] int coins)
        {
            try
            {
                await _service.UpdateCoinsByUuidAsync(uuid, coins);
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

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<UserListDto>>> SearchUsers([FromBody] PagedQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(result);
        }
    }
}
