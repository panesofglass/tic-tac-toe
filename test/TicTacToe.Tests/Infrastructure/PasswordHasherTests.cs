using TicTacToe.Web.Infrastructure;

namespace TicTacToe.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();
    private readonly Player _testPlayer = Player.Default;

    [Theory]
    [InlineData("short", false, "must be at least 8 characters")]
    [InlineData("nogoodpassword", false, "must contain at least one uppercase letter")]
    [InlineData("NOGOODPASSWORD", false, "must contain at least one lowercase letter")]
    [InlineData("NoGoodPassword", false, "must contain at least one number")]
    [InlineData("NoGoodPassword1", false, "must contain at least one special character")]
    [InlineData("NoGoodPassword1!", true, null)]
    [InlineData("Test1234!@#$", true, null)]
    [InlineData("", false, "Password is required")]
    public void ValidatePassword_ReturnsExpectedResult(
        string password,
        bool expectedValid,
        string? expectedError
    )
    {
        // Act
        var result = _hasher.ValidatePassword(password);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        if (expectedError != null)
        {
            Assert.Contains(expectedError.ToLower(), result.Error?.ToLower());
        }
        else
        {
            Assert.Null(result.Error);
        }
    }

    [Fact]
    public void HashPassword_CreatesValidHash()
    {
        // Arrange
        var password = "Test1234!";

        // Act
        var testPlayer = _testPlayer with
        {
            PasswordHash = _hasher.HashPassword(_testPlayer, password),
        };

        // Assert
        Assert.NotNull(testPlayer.PasswordHash);
        Assert.NotEmpty(testPlayer.PasswordHash);
        Assert.True(_hasher.VerifyPassword(testPlayer, password));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        // Arrange
        var password = "Test1234!";
        var testPlayer = _testPlayer with
        {
            PasswordHash = _hasher.HashPassword(_testPlayer, password),
        };

        // Act & Assert
        Assert.False(_hasher.VerifyPassword(testPlayer, "WrongPassword1!"));
    }

    [Fact]
    public void HashPassword_CreatesDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "Test1234!";

        // Act
        var testPlayer1 = _testPlayer with
        {
            PasswordHash = _hasher.HashPassword(_testPlayer, password),
        };
        var testPlayer2 = _testPlayer with
        {
            PasswordHash = _hasher.HashPassword(_testPlayer, password),
        };

        // Assert
        Assert.NotEqual(testPlayer1.PasswordHash, testPlayer2.PasswordHash);
        Assert.True(_hasher.VerifyPassword(testPlayer1, password));
        Assert.True(_hasher.VerifyPassword(testPlayer2, password));
    }
}
