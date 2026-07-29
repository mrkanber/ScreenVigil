using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ScreenVigil.Services;

public sealed class BrowserBridge : IDisposable
{
    public const int Port = 51823;

    private readonly TcpListener _listener = new(IPAddress.Loopback, Port);
    private readonly CancellationTokenSource _cts = new();

    public event Action<string>? DomainActivated;

    public void Start()
    {
        _listener.Start();
        _ = AcceptLoopAsync(_cts.Token);
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(token);
                _ = HandleClientAsync(client, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            try
            {
                client.NoDelay = true;
                await using var stream = client.GetStream();

                var buffer = new byte[4096];
                var read = await stream.ReadAsync(buffer.AsMemory(), token);
                var text = Encoding.UTF8.GetString(buffer, 0, read);

                var bodyIndex = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                if (bodyIndex >= 0)
                {
                    HandleBody(text[(bodyIndex + 4)..]);
                }

                const string response = "HTTP/1.1 204 No Content\r\nAccess-Control-Allow-Origin: *\r\nConnection: close\r\n\r\n";
                var responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, token);
            }
            catch
            {
                // if a single request fails, the connection just closes silently
            }
        }
    }

    private void HandleBody(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("domain", out var domainProp))
            {
                var domain = domainProp.GetString();
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    DomainActivated?.Invoke(domain);
                }
            }
        }
        catch
        {
            // malformed/missing body is silently ignored
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _cts.Dispose();
    }
}
