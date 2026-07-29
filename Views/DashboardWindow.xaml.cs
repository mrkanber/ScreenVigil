using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ScreenVigil.Services;
using ScreenVigil.ViewModels;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace ScreenVigil.Views;

public partial class DashboardWindow : Window
{
    private static readonly Color[] Palette =
    {
        (Color)ColorConverter.ConvertFromString("#3987E5")!,
        (Color)ColorConverter.ConvertFromString("#D95926")!,
        (Color)ColorConverter.ConvertFromString("#199E70")!,
        (Color)ColorConverter.ConvertFromString("#C98500")!,
        (Color)ColorConverter.ConvertFromString("#D55181")!,
        (Color)ColorConverter.ConvertFromString("#008300")!,
        (Color)ColorConverter.ConvertFromString("#9085E9")!,
        (Color)ColorConverter.ConvertFromString("#E66767")!,
    };

    private static readonly Brush MutedBrush =
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B6A64")!);

    private readonly UsageAggregator _aggregator;
    private readonly ObservableCollection<UsageRowViewModel> _rows = new();
    private readonly Dictionary<string, Brush> _colorByKey = new();
    private readonly DispatcherTimer _timer;
    private int _nextColorIndex;

    public DashboardWindow(UsageAggregator aggregator)
    {
        InitializeComponent();
        _aggregator = aggregator;

        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_rows);
        view.SortDescriptions.Add(new SortDescription(nameof(UsageRowViewModel.Duration), ListSortDirection.Descending));
        view.IsLiveSorting = true;
        view.LiveSortingProperties.Add(nameof(UsageRowViewModel.Duration));
        RowsItemsControl.ItemsSource = view;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => Refresh();

        Loaded += (_, _) =>
        {
            Refresh();
            _timer.Start();
        };
        Closing += (_, _) => _timer.Stop();
    }

    private void Refresh()
    {
        var snapshot = _aggregator.Snapshot();
        if (snapshot.Count == 0) return;

        var maxSeconds = Math.Max(1, snapshot[0].Duration.TotalSeconds);

        foreach (var entry in snapshot)
        {
            var row = _rows.FirstOrDefault(r => r.Key == entry.Key);
            if (row is null)
            {
                row = new UsageRowViewModel(entry.Key, entry.Label, GetColorFor(entry.Key));
                _rows.Add(row);
            }

            row.Duration = entry.Duration;
            row.WidthFraction = entry.Duration.TotalSeconds / maxSeconds;
        }
    }

    private Brush GetColorFor(string key)
    {
        if (_colorByKey.TryGetValue(key, out var existing)) return existing;

        Brush brush = _nextColorIndex < Palette.Length
            ? new SolidColorBrush(Palette[_nextColorIndex])
            : MutedBrush;
        _nextColorIndex++;
        _colorByKey[key] = brush;
        return brush;
    }
}
