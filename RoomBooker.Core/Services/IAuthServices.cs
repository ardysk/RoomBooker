using System.Threading.Tasks;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(string email, string password);
    }
}
