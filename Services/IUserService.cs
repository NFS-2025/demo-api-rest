using demo_api_rest.Models;

namespace demo_api_rest.Services
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterUserAsync(RegisterRequest request);
        Task<bool> IsEmailRegisteredAsync(string email);
    }
}