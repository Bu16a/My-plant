using Plugin.Firebase.Auth;

namespace UPlant
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (CrossFirebaseAuth.Current.CurrentUser == null)
                MainPage = new RegisterPage();
            else
            {
                PlantDB.LoadPlantData();
                MainPage = new NavigationPage(new MainPage());
            }
        }
    }
}
