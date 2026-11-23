using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TaskFlow.Web.Services;

namespace TaskFlow.Web.Auth;

// Источник информации о пользователе для AuthorizeView, AuthorizeRouteView и [Authorize]
public sealed class TokenAuthStateProvider(LocalStorageService storage) : AuthenticationStateProvider
{
    private const string StorageKey = "taskflow.auth";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await GetSessionAsync();
        return session is null ? Anonymous : new AuthenticationState(CreatePrincipal(session));
    }

    // Используется также на неделе 8 обработчиком, который добавляет заголовок Authorization
    public async Task<AuthSession?> GetSessionAsync()
    {
        var session = await storage.GetItemAsync<AuthSession>(StorageKey);
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            // Токен истёк: забываем сессию
            await storage.RemoveItemAsync(StorageKey);
            return null;
        }

        return session;
    }

    public async Task SignInAsync(AuthSession session)
    {
        await storage.SetItemAsync(StorageKey, session);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(session))));
    }

    public async Task SignOutAsync()
    {
        await storage.RemoveItemAsync(StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthSession session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, session.Email),
            new(ClaimTypes.Email, session.Email),
            new("expires_at", session.ExpiresAt.ToString("O"))
        };

        // Непустой authenticationType делает IsAuthenticated == true
        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        return new ClaimsPrincipal(identity);
    }
}
