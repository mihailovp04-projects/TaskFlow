using Microsoft.JSInterop;

namespace TaskFlow.Web.Services;

// Показывает уведомления через изолированный JS-модуль и принимает обратные вызовы из JS
public sealed class ToastService(IJSRuntime js) : IAsyncDisposable
{
    private readonly Dictionary<string, Func<Task>> _actions = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<ToastService>? _selfRef;

    public async Task ShowAsync(
        string message,
        ToastKind kind = ToastKind.Info,
        string? actionText = null,
        Func<Task>? onAction = null,
        int durationMs = 4000)
    {
        var module = await GetModuleAsync();
        var id = Guid.NewGuid().ToString("N");

        // Ссылку на себя передаём в JS только если есть действие, которое JS должен вызвать
        DotNetObjectReference<ToastService>? callbackRef = null;
        if (actionText is not null && onAction is not null)
        {
            _actions[id] = onAction;
            _selfRef ??= DotNetObjectReference.Create(this);
            callbackRef = _selfRef;
        }

        var kindName = kind.ToString().ToLowerInvariant();
        await module.InvokeVoidAsync("showToast", id, message, kindName, actionText, durationMs, callbackRef);
    }

    [JSInvokable]
    public async Task OnToastAction(string id)
    {
        if (_actions.Remove(id, out var action))
        {
            await action();
        }
    }

    [JSInvokable]
    public void OnToastExpired(string id)
    {
        _actions.Remove(id);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }

        _selfRef?.Dispose();
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        // Модуль загружается один раз за жизнь вкладки, путь относительно <base href>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/toast.js");
        return _module;
    }
}
