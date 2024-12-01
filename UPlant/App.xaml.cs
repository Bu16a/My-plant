using Plugin.Firebase.Auth;
using System.Net;

namespace UPlant
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            if (CrossFirebaseAuth.Current.CurrentUser == null)
                MainPage = new RegisterPage();
            else
            {
                PlantDB.LoadPlantData();
                MainPage = new NavigationPage(new MainPage());
            }
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