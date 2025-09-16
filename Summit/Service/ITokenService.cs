namespace SummitAPI.Service
{
    public interface ITokenService
    {
        string GenerateToken(Guid userId, string email);
    }
}
