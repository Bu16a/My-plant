using Microsoft.Extensions.Logging;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using UPlant.Domain.Interfaces;
using UPlant.Infrastructure.Services;

namespace UPlant;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Регистрация сервисов
        builder.Services.AddSingleton<IFirebaseDatabase, FirebaseDatabase>();
        builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();
        builder.Services.AddSingleton<IPlantRepository>(sp => 
            new PlantRepository(
                sp.GetRequiredService<IFirebaseDatabase>(),
                sp.GetRequiredService<IAuthService>()
            ));
        
        // Регистрация Firebase сервисов
        builder.Services.AddSingleton(CrossFirebaseAuth.Current);
        builder.Services.AddSingleton(CrossFirebaseCloudMessaging.Current);
        
        // Регистрация страниц
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MyPlantsPage>();
        builder.Services.AddTransient<PlantSettingsPage>();
        builder.Services.AddTransient<PlantInfoPage>();
        builder.Services.AddTransient<ChoosePlantPage>();

        // После регистрации сервисов
        builder.Services.AddSingleton<IServiceProvider>(sp => sp);
        builder.Services.AddSingleton<INavigationService, NavigationService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}