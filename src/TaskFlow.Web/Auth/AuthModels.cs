namespace TaskFlow.Web.Auth;

// Тела запросов к /api/auth/login и /api/auth/register
public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Email, string Password);

// Ответ /api/auth/login
public sealed record AccessTokenResponse(string TokenType, string AccessToken, long ExpiresIn, string RefreshToken);

// То, что клиент хранит в localStorage после входа
public sealed record AuthSession(string Email, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

// Ответ 400 от /api/auth/register (формат ValidationProblemDetails)
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
