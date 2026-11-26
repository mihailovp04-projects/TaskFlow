using Microsoft.JSInterop;
using TaskFlow.Web.Services;

namespace TaskFlow.Web.State;

public sealed record ThemeInfo(string Name)
{
    public bool IsDark => Name == "dark";
}

public sealed class ThemeState(LocalStorageService storage, IJSRuntime js) : IAsyncDisposable
{
    private const string ThemeKey = "taskflow.theme";
    private IJSObjectReference? _module;
    private bool _initialized;

    public ThemeInfo Current { get; private set; } = new("light");

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        var saved = await storage.GetItemAsync<string>(ThemeKey);
        Current = new ThemeInfo(saved == "dark" ? "dark" : "light");
        await ApplyAsync();
        OnChange?.Invoke();
    }

    public async Task ToggleAsync()
    {
        Current = new ThemeInfo(Current.IsDark ? "light" : "dark");
        await storage.SetItemAsync(ThemeKey, Current.Name);
        await ApplyAsync();
        OnChange?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }

    private async Task ApplyAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
        await _module.InvokeVoidAsync("applyTheme", Current.Name);
    }
}