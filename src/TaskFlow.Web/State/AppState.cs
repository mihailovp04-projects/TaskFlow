using TaskFlow.Web.Services;

namespace TaskFlow.Web.State;

// Состояние интерфейса, общее для страниц и меню. В WASM один экземпляр на вкладку
public sealed class AppState(LocalStorageService storage)
{
    private const string HideCompletedKey = "taskflow.hideCompleted";
    private bool _initialized;

    // Сохраняется в localStorage и переживает F5
    public bool HideCompleted { get; private set; }

    // Только в памяти: пересчитывается страницей задач после загрузки списка
    public int OpenTaskCount { get; private set; }

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        HideCompleted = await storage.GetItemAsync<bool>(HideCompletedKey);
        NotifyStateChanged();
    }

    public async Task SetHideCompletedAsync(bool value)
    {
        if (HideCompleted == value)
        {
            return;
        }

        HideCompleted = value;
        await storage.SetItemAsync(HideCompletedKey, value);
        NotifyStateChanged();
    }

    public void SetOpenTaskCount(int count)
    {
        if (OpenTaskCount == count)
        {
            return;
        }

        OpenTaskCount = count;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
