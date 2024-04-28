using System.Text.Json;

using StockPriceWatcher.Input;
using StockPriceWatcher.Config;
using StockPriceWatcher.Mail;
using StockPriceWatcher.Api;
using StockPriceWatcher.Observer;
using StockPriceWatcher.State;

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
