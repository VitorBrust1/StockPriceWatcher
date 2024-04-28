using System.Text.Json.Nodes;    

namespace StockPriceWatcher.Api;

class ApiManager {
    string? _key;
    int _delay;
    HttpClient? _http_client;

    public ApiManager(string key, int delay) {
        _key = key;
        _http_client = new HttpClient();
        _delay = delay;
    }

    public async Task<(decimal, string)> fetch_from_api(string stock) {
        string content = string.Empty;
        string url = parse_url(stock);

        try {
            var response = await _http_client!.GetAsync(url);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        } catch (Exception ex) {
            Console.WriteLine($"Failed to fetch data from {stock}: {ex.Message}");
        }

        var parsed = JsonNode.Parse(content)!["results"]![0]!;
        decimal price = parsed["regularMarketPrice"]!.GetValue<decimal>();
        string currency = parsed["currency"]!.GetValue<string>();
        return (price, currency);
    }

    private string parse_url(string stock) => String.Format("https://brapi.dev/api/quote/{0}?token={1}", stock, _key);
}
