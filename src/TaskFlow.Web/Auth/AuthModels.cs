namespace TaskFlow.Web.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Email, string Password);

public sealed record AccessTokenResponse(string TokenType, string AccessToken, long ExpiresIn, string RefreshToken);

public sealed record AuthSession(string Email, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public sealed class ValidationProblemResponse
{
    public string? Title { get; set; }

    public Dictionary<string, string[]> Errors { get; set; } = new();
}

public sealed record AuthResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static AuthResult Success() => new(true, []);

    public static AuthResult Failure(params string[] errors) => new(false, errors);
}

public sealed record ProfileInfo(string Email, bool IsEmailConfirmed);

public sealed record ProfileResult(ProfileInfo? Profile, bool Unauthorized, string? Error);