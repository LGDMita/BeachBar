namespace BeachBar.Services;

/// <inheritdoc />
public class DashboardEventService : IDashboardEventService
{
    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}
