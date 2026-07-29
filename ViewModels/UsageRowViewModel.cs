using System.ComponentModel;
using Brush = System.Windows.Media.Brush;

namespace ScreenVigil.ViewModels;

public sealed class UsageRowViewModel : INotifyPropertyChanged
{
    private const double MaxBarWidth = 320;

    private TimeSpan _duration;
    private double _widthFraction;

    public UsageRowViewModel(string key, string label, Brush color)
    {
        Key = key;
        Label = label;
        Color = color;
    }

    public string Key { get; }
    public string Label { get; }
    public Brush Color { get; }

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (_duration == value) return;
            _duration = value;
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(DurationText));
        }
    }

    public double WidthFraction
    {
        get => _widthFraction;
        set
        {
            if (Math.Abs(_widthFraction - value) < 0.0001) return;
            _widthFraction = value;
            OnPropertyChanged(nameof(BarWidth));
        }
    }

    public double BarWidth => Math.Max(2, WidthFraction * MaxBarWidth);

    public string DurationText
    {
        get
        {
            var d = _duration;
            if (d.TotalHours >= 1) return $"{(int)d.TotalHours}h {d.Minutes}m";
            if (d.TotalMinutes >= 1) return $"{d.Minutes}m {d.Seconds}s";
            return $"{d.Seconds}s";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
