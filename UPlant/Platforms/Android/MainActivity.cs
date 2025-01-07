using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Firebase;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.ApplicationModel;

namespace UPlant
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Устанавливаем прозрачный статус бар
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                
                // Определяем текущую тему
                var currentTheme = AppInfo.RequestedTheme;
                if (currentTheme == AppTheme.Dark)
                {
                    // Для темной темы убираем флаг LightStatusBar
                    Window.DecorView.SystemUiVisibility &= ~(StatusBarVisibility)SystemUiFlags.LightStatusBar;
                }
                else
                {
                    // Для светлой темы добавляем флаг LightStatusBar
                    Window.DecorView.SystemUiVisibility |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                }
            }

            FirebaseApp.InitializeApp(this);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "default_channel",
                    "Important Notifications",
                    NotificationImportance.High)
                {
                    Description = "Notifications with immediate actions"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}