using Newtonsoft.Json.Linq;

using System.Net.WebSockets;
using System.Text;

namespace ShareInvest.Crypto;

public abstract class ShareWebSocket<Ticker>(string baseUrl) : IDisposable where Ticker : class
{
    public event EventHandler<Ticker>? SendTicker;

    public virtual async Task RequestAsync(string json)
    {
        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, cts.Token);
    }

    public virtual async Task ReceiveAsync()
    {
        while (WebSocketState.Open == socket.State)
        {
            var buffer = new byte[0x400];

            var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

            var str = Encoding.UTF8.GetString(buffer, 0, res.Count);

            if (string.IsNullOrEmpty(str))
            {
                continue;
            }
            var jToken = JToken.Parse(str);

            if (string.IsNullOrEmpty(jToken.Value<string>("status")))
            {
                switch (Activator.CreateInstance(typeof(Ticker), str))
                {
                    case Ticker ticker:
                        SendTicker?.Invoke(this, ticker);
                        continue;
                }
            }
            Console.WriteLine(jToken);
        }
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