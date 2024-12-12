using Plugin.Firebase.Auth;
using System.Text.Json;
using System.Text;
using UPlant.Domain.Interfaces;

namespace UPlant.Infrastructure.Services;

public class FirebaseDatabase : IFirebaseDatabase
{
    private readonly string _database = "https://uplant-36fdf-default-rtdb.europe-west1.firebasedatabase.app/";
    private readonly string _apiKey = "AIzaSyCeIV8Iap29IkTUB456O0saiHtd2arW10E";
    private readonly IFirebaseAuth _auth;
    private readonly HttpClient _httpClient;

    public FirebaseDatabase(IFirebaseAuth auth)
    {
        _auth = auth;
        _httpClient = new HttpClient();
    }

    public async Task WriteDatabaseAsync(string path, object data)
    {
        if (_auth.CurrentUser == null) throw new Exception("Пользователь не авторизован");
        var url = $"{_database}Users/{_auth.CurrentUser.Uid}/{path}.json?auth={_apiKey}";
        await _httpClient.PutAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
    }

    public async Task<string> ReadDatabaseAsync(string path)
    {
        if (_auth.CurrentUser == null) throw new Exception("Пользователь не авторизован");
        var url = $"{_database}Users/{_auth.CurrentUser.Uid}/{path}.json?auth={_apiKey}";
        return await _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync();
    }

    public async Task<string> ReadServerURLAsync()
    {
        var url = $"{_database}server.json?auth={_apiKey}";
        return JsonSerializer.Deserialize<string>(await _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync());
    }
} 