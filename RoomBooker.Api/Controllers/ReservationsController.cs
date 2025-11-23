using System.Collections.Generic;
using System.Security.Claims; // <--- Ważny using do odczytu Tokena
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // <--- Ważne
using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Services;

namespace RoomBooker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <--- Zabezpieczamy kontroler, żeby User.Claims działało
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationDto>> Get(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationDto dto)
        {
            try
            {
                // 1. Wyciąganie User ID
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Błąd: Nie można zidentyfikować użytkownika z tokena.");
                }
                dto.UserId = userId;

                // 2. Wywołanie serwisu
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.ReservationId }, created);
            }
            catch (Exception ex)
            {
                // 👇 TO JEST KLUCZOWE: Zwracamy treść błędu do frontendu!
                // Wypisujemy też błąd w konsoli serwera
                Console.WriteLine($"🛑 KRYTYCZNY BŁĄD W API: {ex.ToString()}");

                // Zwracamy kod 500, ale z wiadomością
                return StatusCode(500, new { message = "Błąd serwera", details = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id) // <-- Usuń parametr adminUserId z argumentów
        {
            // 1. Pobierz ID z tokena
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
            {
                return Unauthorized();
            }

            // 2. Wywołaj serwis
            var ok = await _service.ApproveAsync(id, adminUserId);
            if (!ok) return NotFound();

            return NoContent();
        }

        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromQuery] string? reason)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
            {
                return Unauthorized();
            }

            var ok = await _service.RejectAsync(id, adminUserId, reason);
            if (!ok) return NotFound();

            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromQuery] int userId)
        {
            var ok = await _service.CancelAsync(id, userId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}