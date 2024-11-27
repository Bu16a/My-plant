using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

namespace UPlant;

public static class FirebaseApi
{
    private static readonly IFirebaseAuth _auth = CrossFirebaseAuth.Current;
    private static readonly IFirebaseCloudMessaging _messaging = CrossFirebaseCloudMessaging.Current;

    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    public static async Task SignUpAsync(string email, string password)
    {
        await _auth.CreateUserAsync(email, password);
        await WriteDatabaseAsync("fcm_token", await GetFcmTokenAsync());
    }

    /// <summary>
    /// Авторизация пользователя.
    /// </summary>
    public static async Task SignInAsync(string email, string password)
    {
        await _auth.SignInWithEmailAndPasswordAsync(email, password);
        await WriteDatabaseAsync("fcm_token", await GetFcmTokenAsync());
    }

    /// <summary>
    /// Выход из системы.
    /// </summary>
    public async static Task Logout()
    {
        await WriteDatabaseAsync("fcm_token", null);
        await _auth.SignOutAsync();
        Application.Current.MainPage = new RegisterPage();
    }

    /// <summary>
    /// Получение FCM токена устройства.
    /// </summary>
    public static async Task<string> GetFcmTokenAsync()
    {
        return await _messaging.GetTokenAsync();
    }

    private static readonly HttpClient _httpClient = new();
    public static async Task WriteDatabaseAsync(string path, object data)
    {
        if (_auth.CurrentUser == null) throw new Exception();
        var url = $"https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/Users/{_auth.CurrentUser.Uid}/{path}.json?auth=AIzaSyCeIV8Iap29IkTUB456O0saiHtd2arW10E";
        await _httpClient.PutAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
    }
    public static async Task<string> ReadDatabaseAsync(string path)
    {
        if (_auth.CurrentUser == null) throw new Exception();
        var url = $"https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/Users/{_auth.CurrentUser.Uid}/{path}.json?auth=AIzaSyCeIV8Iap29IkTUB456O0saiHtd2arW10E";
        return await _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync();
    }
}