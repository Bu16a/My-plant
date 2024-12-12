using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using UPlant.Domain.Interfaces;

namespace UPlant.Infrastructure.Services;

public class FirebaseAuthService : IAuthService
{
    private readonly IFirebaseAuth _auth;
    private readonly IFirebaseCloudMessaging _messaging;
    private readonly IFirebaseDatabase _database;

    public event EventHandler<bool> AuthStateChanged;

    public FirebaseAuthService(
        IFirebaseAuth auth,
        IFirebaseCloudMessaging messaging,
        IFirebaseDatabase database)
    {
        _auth = auth;
        _messaging = messaging;
        _database = database;
    }

    public bool IsAuthenticated => _auth.CurrentUser != null;

    public async Task SignUpAsync(string email, string password)
    {
        await _auth.CreateUserAsync(email, password);
        await _database.WriteDatabaseAsync("fcm_token", await GetFcmTokenAsync());
        AuthStateChanged?.Invoke(this, true);
    }

    public async Task SignInAsync(string email, string password)
    {
        await _auth.SignInWithEmailAndPasswordAsync(email, password);
        await _database.WriteDatabaseAsync("fcm_token", await GetFcmTokenAsync());
        AuthStateChanged?.Invoke(this, true);
    }

    public async Task LogoutAsync()
    {
        await _database.WriteDatabaseAsync("fcm_token", null);
        await _auth.SignOutAsync();
        AuthStateChanged?.Invoke(this, false);
    }

    public async Task<string> GetFcmTokenAsync()
    {
        return await _messaging.GetTokenAsync();
    }
} 