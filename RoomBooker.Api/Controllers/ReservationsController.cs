using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Services;

namespace RoomBooker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        // GET: api/reservations/room/1
        [HttpGet("room/{roomId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetForRoom(int roomId)
        {
            var result = await _service.GetForRoomAsync(roomId);
            return Ok(result);
        }

        // GET: api/reservations/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationDto>> Get(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/reservations
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.ReservationId }, created);
        }

        // POST: api/reservations/5/approve?adminUserId=1
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, [FromQuery] int adminUserId)
        {
            var ok = await _service.ApproveAsync(id, adminUserId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST: api/reservations/5/reject?adminUserId=1&reason=...
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromQuery] int adminUserId, [FromQuery] string? reason)
        {
            var ok = await _service.RejectAsync(id, adminUserId, reason);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST: api/reservations/5/cancel?userId=2
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromQuery] int userId)
        {
            var ok = await _service.CancelAsync(id, userId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
