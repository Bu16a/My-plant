using Plugin.Firebase.Auth;
using System.Net;
using UPlant.Domain.Interfaces;

namespace UPlant
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public App(IAuthService authService, INavigationService navigationService)
        {
            InitializeComponent();
            _authService = authService;
            _navigationService = navigationService;

            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            if (!_authService.IsAuthenticated)
                _navigationService.SetMainPageAsync<RegisterPage>().Wait();
            else
                _navigationService.SetMainPageAsync<MainPage>().Wait();
        }

        //protected override void OnStart()
        //{
        //    if (!IsInternetAvailable())
        //        KillProgram("Нет соединения с интернетом. Приложение будет закрыто").Wait();
        //}

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet)
                await KillProgram("Соединение с интернетом потеряно. Приложение будет закрыто");
        }

        public static bool IsInternetAvailable()
        {
            try
            {
                new HttpClient().GetAsync("http://google.com").Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task KillProgram(string message)
        {
            await MainPage.DisplayAlert("Критическая ошибка", message, "OK");

#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif IOS
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
#endif
        }
    }
}