namespace UPlant.Domain.Interfaces;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    event EventHandler<bool> AuthStateChanged;
    Task SignUpAsync(string email, string password);
    Task SignInAsync(string email, string password);
    Task LogoutAsync();
    Task<string> GetFcmTokenAsync();
} 