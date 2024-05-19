using RestSharp;

using System.Text;

namespace ShareInvest.Crypto;

public abstract class ShareQuotation : RestClient
{
    public ShareQuotation(string baseUrl) : base(baseUrl)
    {

    }

    public ShareQuotation(string baseUrl, string authorizationToken) : base(baseUrl, configureDefaultHeaders: headers =>
    {
        headers.Add(KnownHeaders.Authorization, $"Bearer {authorizationToken}");
    })
    {

    }

    /// <summary>
    /// UPbit
    /// bithumb
    /// </summary>
    public virtual async Task<RestResponse> GetTickerAsync(params string[] codeArr)
    {
        StringBuilder sb = new();

        foreach (string code in codeArr)
        {
            sb.Append(code);
            sb.Append(',');
        }
        var request = new RestRequest($"ticker?markets={sb.Remove(sb.Length - 1, 1)}");

        return await ExecuteAsync(request, cts.Token);
    }

    /// <summary>
    /// UPbit
    /// bithumb
    /// </summary>
    public virtual async Task<RestResponse> GetMarketAsync(bool isDetails = true)
    {
        return await ExecuteAsync(new RestRequest($"market/all?isDetails={isDetails}"), cts.Token);
    }

    readonly CancellationTokenSource cts = new();
}