using Plugin.Firebase.Core.Exceptions;
using UPlant.Domain.Interfaces;

namespace UPlant;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    private static readonly Dictionary<string, string> localizedErrors = new()
    {
        { "The email address is badly formatted.", "Неправильный email" },
        { "at least 6 characters", "Пароль должен содержать минимум 6 символов" },
        { "INVALID_LOGIN_CREDENTIALS", "Неправильный логин или пароль" },
        { "is already in use", "Данный email уже зарегистрирован" }
    };


    public RegisterPage(IAuthService authService, INavigationService navigationService)
    {
        InitializeComponent();
        _authService = authService;
        _navigationService = navigationService;
    }

    private async void OnSignUpClicked(object sender, EventArgs e) =>
        await AuthenticateAsync(isSignUp: true);

    private async void OnSignInClicked(object sender, EventArgs e) =>
        await AuthenticateAsync(isSignUp: false);

    private async Task AuthenticateAsync(bool isSignUp)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Email и пароль обязательны");
            return;
        }

        try
        {
            if (isSignUp)
                await _authService.SignUpAsync(email, password);
            else
                await _authService.SignInAsync(email, password);

            await _navigationService.SetMainPageAsync<MainPage>();
        }
        catch (FirebaseAuthException ex)
        {
            ShowError(GetLocalizedErrorMessage(ex.Message));
        }
        catch (Exception)
        {
            ShowError(isSignUp ? "Неизвестная ошибка при регистрации" : "Неизвестная ошибка при входе");
        }
    }

    private void ShowError(string message)
    {
        StatusLabel.TextColor = Colors.Red;
        StatusLabel.Text = message;
    }

    private string GetLocalizedErrorMessage(string message)
    {
        foreach (var (sub, error) in localizedErrors)
            if (message.Contains(sub))
                return error;
        return "Неизвестная ошибка";
    }
}