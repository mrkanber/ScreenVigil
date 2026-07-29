using ScreenVigil.Models;

namespace ScreenVigil.Services;

public sealed class UsageAggregator
{
    private readonly object _lock = new();
    private readonly Dictionary<string, TimeSpan> _completed = new();
    private readonly Dictionary<string, string> _labels = new();
    private string? _currentKey;
    private DateTime _currentStart = DateTime.UtcNow;

    public void SwitchTo(string key, string displayLabel)
    {
        lock (_lock)
        {
            if (key == _currentKey) return;

            var now = DateTime.UtcNow;
            if (_currentKey is not null)
            {
                _completed[_currentKey] = _completed.GetValueOrDefault(_currentKey) + (now - _currentStart);
            }

            _currentKey = key;
            _labels[key] = displayLabel;
            _currentStart = now;
        }
    }

    public IReadOnlyList<UsageEntry> Snapshot()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var rows = new List<UsageEntry>();

            foreach (var (key, duration) in _completed)
            {
                var total = key == _currentKey ? duration + (now - _currentStart) : duration;
                rows.Add(new UsageEntry(key, _labels.GetValueOrDefault(key, key), total));
            }

            if (_currentKey is not null && !_completed.ContainsKey(_currentKey))
            {
                rows.Add(new UsageEntry(_currentKey, _labels.GetValueOrDefault(_currentKey, _currentKey), now - _currentStart));
            }

            rows.Sort((a, b) => b.Duration.CompareTo(a.Duration));
            return rows;
        }
    }
}
