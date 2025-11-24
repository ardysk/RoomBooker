using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Services;

namespace RoomBooker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet("room/{roomId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetForRoom(int roomId)
        {
            var result = await _service.GetForRoomAsync(roomId);
            return Ok(result);
        }

        [HttpGet("equipment/{equipmentId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetForEquipment(int equipmentId)
        {
            var result = await _service.GetForEquipmentAsync(equipmentId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationDto>> Get(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationCreateDto dto)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                dto.UserId = userId;
            }
            else
            {
                return Unauthorized("Błąd tokena.");
            }

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.ReservationId }, created);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
                return Unauthorized();

            var ok = await _service.ApproveAsync(id, adminUserId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromQuery] string? reason)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
                return Unauthorized();

            var ok = await _service.RejectAsync(id, adminUserId, reason);
            if (!ok) return NotFound();
            return NoContent();
        }

        // 👇 TUTAJ BYŁ BŁĄD (Czekał na [FromQuery] int userId, którego nie było)
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            // Poprawka: Bierzemy ID z tokena
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var ok = await _service.CancelAsync(id, userId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}