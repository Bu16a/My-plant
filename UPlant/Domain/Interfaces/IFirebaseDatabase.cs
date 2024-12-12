namespace UPlant.Domain.Interfaces;

public interface IFirebaseDatabase
{
    Task WriteDatabaseAsync(string path, object data);
    Task<string> ReadDatabaseAsync(string path);
    Task<string> ReadServerURLAsync();
} 