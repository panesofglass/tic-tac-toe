using Microsoft.AspNetCore.Identity;

namespace TicTacToe.Web.Infrastructure;

public class PasswordHasher
{
    private readonly IPasswordHasher<Player> _hasher = new PasswordHasher<Player>();

    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    public record PasswordValidationResult(bool IsValid, string? Error);

    public PasswordValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return new(false, "Password is required.");

        if (password.Length < MinPasswordLength)
            return new(false, $"Password must be at least {MinPasswordLength} characters long.");

        if (password.Length > MaxPasswordLength)
            return new(false, $"Password must not exceed {MaxPasswordLength} characters.");

        if (!password.Any(char.IsUpper))
            return new(false, "Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            return new(false, "Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            return new(false, "Password must contain at least one number.");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return new(false, "Password must contain at least one special character.");

        return new(true, null);
    }

    public string HashPassword(Player player, string password)
    {
        return _hasher.HashPassword(player, password);
    }

    public bool VerifyPassword(Player player, string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(player, hash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
