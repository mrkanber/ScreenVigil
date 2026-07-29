using System.Runtime.InteropServices;

namespace ScreenVigil.Services;

public sealed class ForegroundWatcher : IDisposable
{
    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint eventThread, uint eventTime);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    private const int OBJID_WINDOW = 0;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

    // Kept as a field: otherwise the GC can collect the delegate and the hook callback turns into a null reference.
    private readonly WinEventDelegate _proc;
    private IntPtr _hookId = IntPtr.Zero;

    public event Action<IntPtr>? ForegroundChanged;

    public ForegroundWatcher()
    {
        _proc = HookCallback;
    }

    public void Start()
    {
        _hookId = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
            _proc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

        var current = GetForegroundWindow();
        if (current != IntPtr.Zero) ForegroundChanged?.Invoke(current);
    }

    private void HookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint eventThread, uint eventTime)
    {
        if (idObject != OBJID_WINDOW || hwnd == IntPtr.Zero) return;
        ForegroundChanged?.Invoke(hwnd);
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWinEvent(_hookId);
            _hookId = IntPtr.Zero;
        }
    }
}
