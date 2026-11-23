using Microsoft.JSInterop;

namespace TaskFlow.Web.Services;

// Встроенные диалоги браузера: window.confirm и window.alert
public sealed class BrowserDialogService(IJSRuntime js)
{
    public async Task<bool> ConfirmAsync(string message)
    {
        return await js.InvokeAsync<bool>("confirm", message);
    }

    public async Task AlertAsync(string message)
    {
        await js.InvokeVoidAsync("alert", message);
    }
}
