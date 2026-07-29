using System.Windows;
using ScreenVigil.Services;
using ScreenVigil.Views;

namespace ScreenVigil;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstanceMutex;
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private TrackingCoordinator? _coordinator;
    private UsageAggregator? _aggregator;
    private DashboardWindow? _dashboard;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "Global\\ScreenVigil-SingleInstance", out var isNew);
        if (!isNew)
        {
            Shutdown();
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _aggregator = new UsageAggregator();
        _coordinator = new TrackingCoordinator(_aggregator);
        _coordinator.Start();

        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "ScreenVigil",
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Open Dashboard", null, (_, _) => ShowDashboard());

        var autoStartItem = new System.Windows.Forms.ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = AutoStart.IsEnabled(),
        };
        autoStartItem.CheckedChanged += (_, _) =>
        {
            if (autoStartItem.Checked) AutoStart.Enable();
            else AutoStart.Disable();
        };
        menu.Items.Add(autoStartItem);

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Shutdown());

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowDashboard();
    }

    private void ShowDashboard()
    {
        if (_dashboard is null)
        {
            _dashboard = new DashboardWindow(_aggregator!);
            _dashboard.Closing += (_, args) =>
            {
                args.Cancel = true;
                _dashboard.Hide();
            };
        }

        _dashboard.Show();
        _dashboard.WindowState = WindowState.Normal;
        _dashboard.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _coordinator?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
