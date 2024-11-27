namespace UPlant;

public partial class RegisterPage : ContentPage
{

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StatusLabel.Text = "Email и пароль обязательны!";
            return;
        }

        try
        {
            await FirebaseApi.SignUpAsync(email, password);
            Application.Current.MainPage = new NavigationPage(new MainPage());
            PlantDB.LoadPlantData();
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Ошибка: {ex}";
        }
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StatusLabel.Text = "Email и пароль обязательны!";
            return;
        }

        try
        {
            await FirebaseApi.SignInAsync(email, password);
            Application.Current.MainPage = new NavigationPage(new MainPage());
            PlantDB.LoadPlantData();
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Ошибка: {ex}";
        }
    }
}
