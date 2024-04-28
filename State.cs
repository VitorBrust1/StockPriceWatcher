using StockPriceWatcher.Api;
using StockPriceWatcher.Observer;

namespace StockPriceWatcher.State;

internal class StockState {
    private List<StockObserver> _observers = new List<StockObserver>();
    private ApiManager _api;
    
    public string targetStock { get; set; }
    public string currency { get; set; } = string.Empty;
    public decimal price { get; set; } = -1;

    public StockState(string stock, ApiManager stockAPI) {
        targetStock = stock;
        _api = stockAPI;
    }

    public void Attach(StockObserver observer) => _observers.Add(observer);

    public bool Remove(StockObserver observer) => _observers.Remove(observer);

    public async Task UpdateAndNotify() {
        (decimal fetchedPrice, string fetchedCurrency) = await _api.fetch_from_api(targetStock);
        
        price = fetchedPrice;
        currency = fetchedCurrency;

        foreach (StockObserver observer in _observers) {
            observer.Update(this);
        }
    }

}
