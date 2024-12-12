using NUnit.Framework;
using Moq;
using UPlant.Domain.Interfaces;
using UPlant.Infrastructure.Services;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;

namespace UPlant.Tests.Services;

[TestFixture]
public class FirebaseAuthServiceTests
{
    private Mock<IFirebaseAuth> _mockAuth;
    private Mock<IFirebaseCloudMessaging> _mockMessaging;
    private Mock<IFirebaseDatabase> _mockDatabase;
    private FirebaseAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockAuth = new Mock<IFirebaseAuth>();
        _mockMessaging = new Mock<IFirebaseCloudMessaging>();
        _mockDatabase = new Mock<IFirebaseDatabase>();

        _mockMessaging.Setup(x => x.GetTokenAsync())
            .ReturnsAsync("test-fcm-token");

        _authService = new FirebaseAuthService(
            _mockAuth.Object,
            _mockMessaging.Object,
            _mockDatabase.Object);
    }

    [Test]
    public async Task SignUp_ShouldCreateUserAndSaveFcmToken()
    {
        // Arrange
        string email = "test@test.com";
        string password = "password123";

        // Act
        await _authService.SignUpAsync(email, password);

        // Assert
        _mockAuth.Verify(x => x.CreateUserAsync(email, password), Times.Once);
        _mockDatabase.Verify(x => x.WriteDatabaseAsync("fcm_token", "test-fcm-token"), Times.Once);
    }

    [Test]
    public async Task SignIn_ShouldSignInUserAndSaveFcmToken()
    {
        // Arrange
        string email = "test@test.com";
        string password = "password123";

        // Act
        await _authService.SignInAsync(email, password);

        // Assert
        _mockAuth.Verify(x => x.SignInWithEmailAndPasswordAsync(email, password, true), Times.Once);
        _mockDatabase.Verify(x => x.WriteDatabaseAsync("fcm_token", "test-fcm-token"), Times.Once);
    }

    [Test]
    public async Task Logout_ShouldSignOutAndClearFcmToken()
    {
        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockAuth.Verify(x => x.SignOutAsync(), Times.Once);
        _mockDatabase.Verify(x => x.WriteDatabaseAsync("fcm_token", null), Times.Once);
    }

    [Test]
    public void IsAuthenticated_ShouldReturnCorrectValue()
    {
        // Arrange
        _mockAuth.Setup(x => x.CurrentUser).Returns(new Mock<IFirebaseUser>().Object);

        // Assert
        Assert.That(_authService.IsAuthenticated, Is.True);
    }
} 