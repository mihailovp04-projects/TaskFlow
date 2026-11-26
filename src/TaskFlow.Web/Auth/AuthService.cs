using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TaskFlow.Web.Auth;

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

    public Task LogoutAsync() => authState.SignOutAsync();

    public async Task<ProfileResult> GetProfileAsync(CancellationToken ct = default)
    {
        var session = await authState.GetSessionAsync();
        if (session is null)
        {
            return new ProfileResult(null, true, null);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/manage/info");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
            using var response = await http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ProfileResult(null, true, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return new ProfileResult(null, false, $"HTTP {(int)response.StatusCode}");
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileInfo>(ct);
            return new ProfileResult(profile, false, null);
        }
        catch (HttpRequestException)
        {
            return new ProfileResult(null, false, ApiUnavailable);
        }
    }

    private async Task<HttpResponseMessage?> PostAsync<T>(string url, T body, CancellationToken ct)
    {
        try
        {
            return await http.PostAsJsonAsync(url, body, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}