using Plugin.Firebase.Core.Exceptions;
using UPlant.Domain.Interfaces;

namespace UPlant;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

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
            ShowError("Email и пароль обязательны!");
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
            ShowError(GetLocalizedErrorMessage(ex));
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

    private string GetLocalizedErrorMessage(FirebaseAuthException ex)
    {
        return ex.Reason switch // TODO: BAD LOCALIZATION
        {
            FIRAuthError.InvalidEmail => "Неверный формат email",
            FIRAuthError.WrongPassword => "Неверный пароль",
            FIRAuthError.WeakPassword => "Слишком простой пароль. Пароль должен содержать как минимум 6 символов",
            FIRAuthError.EmailAlreadyInUse => "Email уже используется",
            FIRAuthError.UserNotFound => "Пользователь не найден",
            FIRAuthError.UserTokenExpired => "Сессия истекла. Пожалуйста, войдите снова",
            FIRAuthError.InvalidCredential => "Неверные учетные данные",
            FIRAuthError.UserDisabled => "Аккаунт отключен",
            FIRAuthError.AccountExistsWithDifferentCredential => "Аккаунт уже существует с другими учетными данными",
            FIRAuthError.Undefined => "Неизвестная ошибка",
            _ => $"Ошибка: {ex.Reason}"
        };
    }
}