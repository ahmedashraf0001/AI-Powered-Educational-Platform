namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
