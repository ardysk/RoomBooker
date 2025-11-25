using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Services;
using System.Security.Claims;

namespace RoomBooker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewsController(IReviewService service)
        {
            _service = service;
        }

        [HttpGet("room/{roomId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetForRoom(int roomId)
        {
            var userId = GetUserId();
            var reviews = await _service.GetForRoomAsync(roomId, userId);
            return Ok(reviews);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var myReviews = await _service.GetByUserAsync(userId);
            return Ok(myReviews);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var created = await _service.AddAsync(dto, userId);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReviewCreateDto dto)
        {
            var userId = GetUserId();
            var success = await _service.UpdateAsync(id, userId, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var success = await _service.DeleteAsync(id, userId, isAdmin);
            if (!success) return NotFound();
            return NoContent();
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
        }
    }
}