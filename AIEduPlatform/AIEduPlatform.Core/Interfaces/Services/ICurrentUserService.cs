namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }
}
