namespace SummitAPI.Dtos
{
    public record RegisterRequest(string Name, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(Guid Id, string Email, string Token);
}
