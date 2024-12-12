namespace UPlant.Domain.Interfaces;

public interface INavigationService
{
    Task NavigateToAsync<T>(params object[] parameters) where T : Page;
    Task NavigateBackAsync();
    Task SetMainPageAsync<T>(params object[] parameters) where T : Page;
} 