using UPlant.Domain.Interfaces;

namespace UPlant.Infrastructure.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToAsync<T>(params object[] parameters) where T : Page
    {
        var page = ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
        if (GetCurrentPage().Navigation != null)
            await GetCurrentPage().Navigation.PushAsync(page);
        else
            Application.Current.MainPage = new NavigationPage(page);
    }

    public async Task NavigateBackAsync()
    {
        if (GetCurrentPage().Navigation != null)
            await GetCurrentPage().Navigation.PopAsync();
    }

    public Task SetMainPageAsync<T>(params object[] parameters) where T : Page
    {
        var page = ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
        Application.Current.MainPage = new NavigationPage(page);
        return Task.CompletedTask;
    }

    private Page GetCurrentPage()
    {
        return Application.Current.MainPage is NavigationPage navigationPage
            ? navigationPage.CurrentPage
            : Application.Current.MainPage;
    }
} 