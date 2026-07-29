using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenVigil.Services;

public static class ProcessResolver
{
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    public static (string Key, string Label) Resolve(IntPtr hwnd)
    {
        try
        {
            GetWindowThreadProcessId(hwnd, out var pid);
            using var process = Process.GetProcessById((int)pid);
            var key = process.ProcessName.ToLowerInvariant() + ".exe";

            string label;
            try
            {
                var description = process.MainModule?.FileVersionInfo.FileDescription;
                label = !string.IsNullOrWhiteSpace(description) ? description! : process.ProcessName;
            }
            catch
            {
                // MainModule can't be read for elevated/inaccessible processes, fall back to ProcessName
                label = process.ProcessName;
            }

            return (key, label);
        }
        catch
        {
            var title = GetWindowTitle(hwnd);
            return (title, title);
        }
    }

    private static string GetWindowTitle(IntPtr hwnd)
    {
        var buffer = new StringBuilder(256);
        GetWindowText(hwnd, buffer, buffer.Capacity);
        var title = buffer.ToString();
        return string.IsNullOrWhiteSpace(title) ? "Unknown" : title;
    }
}
