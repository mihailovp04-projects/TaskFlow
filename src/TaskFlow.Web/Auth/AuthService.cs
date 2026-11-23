using System.Net;
using System.Net.Http.Json;

namespace TaskFlow.Web.Auth;

// Регистрация, вход и выход через эндпоинты Identity API (/api/auth/*)
public sealed class AuthService(HttpClient http, TokenAuthStateProvider authState)
{
    private const string ApiUnavailable = "Cannot reach the API. Is TaskFlow.Api running on http://localhost:5080?";

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        using var response = await PostAsync("api/auth/login", new LoginRequest(email, password), ct);
        if (response is null)
        {
            return AuthResult.Failure(ApiUnavailable);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return AuthResult.Failure("Invalid email or password.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return AuthResult.Failure($"Login failed: HTTP {(int)response.StatusCode}.");
        }

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(ct);
        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            return AuthResult.Failure("The API returned an empty token.");
        }

        // Токен Identity API не JWT: срок действия вычисляем сами из expiresIn
        var session = new AuthSession(
            email,
            token.AccessToken,
            token.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));

        await authState.SignInAsync(session);
        return AuthResult.Success();
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        using var response = await PostAsync("api/auth/register", new RegisterRequest(email, password), ct);
        if (response is null)
        {
            return AuthResult.Failure(ApiUnavailable);
        }

        if (response.IsSuccessStatusCode)
        {
            return AuthResult.Success();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(ct);
            var errors = problem?.Errors.SelectMany(pair => pair.Value).ToArray() ?? [];
            if (errors.Length > 0)
            {
                return AuthResult.Failure(errors);
            }
        }

        return AuthResult.Failure($"Registration failed: HTTP {(int)response.StatusCode}.");
    }

    // У bearer-токенов нет серверного выхода: клиент просто забывает токен
    public Task LogoutAsync() => authState.SignOutAsync();

    private async Task<HttpResponseMessage?> PostAsync<T>(string url, T body, CancellationToken ct)
    {
        try
        {
            return await http.PostAsJsonAsync(url, body, ct);
        }
        catch (HttpRequestException)
        {
            // API не запущен или запрос заблокирован CORS
            return null;
        }
    }
}
