using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Services;

namespace RoomBooker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var rooms = await _roomService.GetAllAsync(includeInactive);
            return Ok(rooms);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoomDto>> GetById(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> Create(RoomDto dto)
        {
            var created = await _roomService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.RoomId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<RoomDto>> Update(int id, RoomDto dto)
        {
            var updated = await _roomService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _roomService.DeactivateAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
