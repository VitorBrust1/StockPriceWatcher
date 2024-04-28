using StockPriceWatcher.Mail;
using StockPriceWatcher.State;

namespace StockPriceWatcher.Observer;

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
