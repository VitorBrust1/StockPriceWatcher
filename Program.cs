// using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Mail;

namespace StockQuoteALert;

internal class StockObserver {
    private decimal _lowerbound;
    private decimal _upperbound;
    private MailManager _mail_manager;

    public StockObserver(MailManager mail_manager, decimal lowerbound, decimal upperbound) {
        _lowerbound = lowerbound;
        _upperbound = upperbound;
        _mail_manager = mail_manager;
    }

    public void Update(StockState stock) {
        if (stock.price == -1 || stock.currency == string.Empty) {
            throw new InvalidOperationException("Received state with no information.");
        }
        if (stock.price < _lowerbound) {
            Console.WriteLine(@$"Detected price for {stock.targetStock} lower than the specified threshold. Sending a notification e-mail...");
            _mail_manager.SendEmails(stock, true);
        } else if (stock.price > _upperbound) {
            Console.WriteLine(@$"Detected price for {stock.targetStock} greater than the specified threshold. Sending a notification e-mail...");
            _mail_manager.SendEmails(stock, false);
        }
    }
}

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

class MailManager {
    private SmtpClient _smtp;
    private HashSet<MailAddress> _senderSet;

    public MailManager(string host, string username, string password) {
        _senderSet = new HashSet<MailAddress>();
        _smtp = new SmtpClient(host) {
            Port = 587,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
        };
    } 
    
    public void SendEmails(StockState stock, bool buyStock) {
        var message = buyStock ? CreateBuyMessage(stock) : CreateSellMessage(stock);

        foreach (MailAddress address in _senderSet) {
            message.To.Add(address);
        }
        _smtp.Send(message);
    }

    public void AddSender(string sender_email) => _senderSet!.Add(new MailAddress(sender_email));

    public void AddSender(List<string> sender_emails) {
        foreach (string email in sender_emails) {
            _senderSet!.Add(new MailAddress(email));
        }
    }

    // Trocar as mensagens
    private MailMessage CreateSellMessage(StockState stock) => new() {
        From = new MailAddress("noreply@stockalert.com"),
        Subject = $"Selling advice regarding one of your assets: {stock.targetStock}",
        Body = @$"One of your assets that's being tracked, {stock.targetStock}, currently has price of
                  {stock.price} {stock.currency}, which is greater than your upperbound threshold. It's
                  a good time to sell these assets!",
        IsBodyHtml = true,
    };

    private MailMessage CreateBuyMessage(StockState stock) => new() {
        From = new MailAddress("noreply@stockalert.com"),
        Subject = $"Buying advice regarding one of your assets: {stock.targetStock}",
        Body = @$"One of your assets that's being tracked, {stock.targetStock}, currently has price of
                  {stock.price} {stock.currency}, which is lesser than your lowerbound threshold. It's
                  a good time to buy some of these assets!",
        IsBodyHtml = true,
    };
}

class JSONConfig {
    public class ApiConfig {
        public string key {get; set;} = string.Empty;
        public int delay {get; set;}
    }

    public class MailConfig {
        public string smtp_server {get; set;} = string.Empty;
        // public string smtp_port {get; set;} = string.Empty;
        public string smtp_user {get; set;} = string.Empty;
        public string smtp_password {get; set;} = string.Empty;
        public List<string> sender_list {get; set;} = new List<string>();
    }

    public ApiConfig? api {get; set;}
    public MailConfig? email {get; set;}
}

public class UserInput {
    public string? stock {get ; set;}
    public decimal sell_price {get ; set;}
    public decimal buy_price {get ; set;}

    public UserInput(string[] args) {
        // TODO: better input checking
        if (args.Length < 3) {
            throw new Exception("Not enough Arguments (Needed: 3)\nUsage: program.exe <Stock Symbol> <Sell Price> <Buy Price>");
        } else if (args.Length > 3) {
            throw new Exception("Too many Arguments (Needed: 3)\nUsage: program.exe <Stock Symbol> <Sell Price> <Buy Price>");
        } else {
            try {
                this.stock = args[0];
                this.sell_price = Decimal.Parse(args[1]);
                this.buy_price = Decimal.Parse(args[2]);
            } catch {
                throw new Exception("Arguments could not be parsed correctly, try again\n");
            }
        }
        if (buy_price >= sell_price) {
            throw new Exception("Buy price must be smaller than the Sell price, try again\n");
        }
    }
}

static class Program {
    static async Task Main(string[] args) {
        var user_input = new UserInput(args);
        string file_text = File.ReadAllText("appConfig.json");
        JSONConfig? parsed_config = JsonSerializer.Deserialize<JSONConfig>(file_text);
        Console.WriteLine($"Initializing Stock Alert for {user_input!.stock}");

        var api_handler = new ApiManager(parsed_config!.api!.key, parsed_config!.api.delay);
        var email_handler = new MailManager(parsed_config!.email!.smtp_server, parsed_config!.email!.smtp_user, parsed_config!.email!.smtp_password);
        email_handler.AddSender(parsed_config!.email.sender_list);

        var observer = new StockObserver(email_handler, user_input.buy_price, user_input.sell_price);
        var state = new StockState(user_input.stock!, api_handler);
        state.Attach(observer);
        
        while (true) {
            await state.UpdateAndNotify();
            Thread.Sleep(10000);
        }
    }
}
