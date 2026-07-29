namespace ScreenVigil.Services;

public sealed class TrackingCoordinator : IDisposable
{
    private static readonly HashSet<string> BrowserExeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome.exe",
        "msedge.exe",
    };

    private readonly ForegroundWatcher _foregroundWatcher = new();
    private readonly BrowserBridge _browserBridge = new();
    private readonly UsageAggregator _aggregator;

    private string? _currentExeKey;

    public TrackingCoordinator(UsageAggregator aggregator)
    {
        _aggregator = aggregator;
        _foregroundWatcher.ForegroundChanged += OnForegroundChanged;
        _browserBridge.DomainActivated += OnDomainActivated;
    }

    public void Start()
    {
        _browserBridge.Start();
        _foregroundWatcher.Start();
    }

    private void OnForegroundChanged(IntPtr hwnd)
    {
        var (key, label) = ProcessResolver.Resolve(hwnd);
        _currentExeKey = key;
        _aggregator.SwitchTo(key, label);
    }

    private void OnDomainActivated(string domain)
    {
        // If the foreground window isn't actually a browser (e.g. a tab changed while
        // Chrome was in the background), the push is dropped entirely — not cached — to avoid mismatched attribution.
        if (_currentExeKey is null || !BrowserExeNames.Contains(_currentExeKey)) return;

        _aggregator.SwitchTo($"{_currentExeKey}|{domain}", domain);
    }

    public void Dispose()
    {
        _foregroundWatcher.Dispose();
        _browserBridge.Dispose();
    }
}
