using System.Net.WebSockets;
using System.Text;

namespace ShareInvest.Crypto;

public abstract class ShareWebSocket<Ticker>(string baseUrl) : IDisposable where Ticker : class
{
    public event EventHandler<Ticker>? SendTicker;

    public abstract Task ReceiveAsync();

    public virtual async Task RequestAsync(string json)
    {
        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, cts.Token);
    }

    /// <summary>
    /// UPbit: api.upbit.com/websocket/v1
    /// bithumb: pubwss.bithumb.com/pub/ws
    /// </summary>
    public virtual async Task ConnectAsync(string? token, TimeSpan? interval = null)
    {
        if (string.IsNullOrEmpty(token) is false)
        {
            socket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
        }
        socket.Options.KeepAliveInterval = interval ?? TimeSpan.MinValue;

        await socket.ConnectAsync(new Uri($"wss://{baseUrl}"), cts.Token);
    }

    public void OnReceiveTicker(string str)
    {
        if (Activator.CreateInstance(typeof(Ticker), str) is Ticker ticker)
        {
            SendTicker?.Invoke(this, ticker);
        }
    }

    public void Dispose()
    {
        socket.Dispose();

        GC.SuppressFinalize(this);
    }

    protected ClientWebSocket Socket
    {
        get => socket;
    }

    readonly string baseUrl = baseUrl;

    readonly ClientWebSocket socket = new();
    readonly CancellationTokenSource cts = new();
}