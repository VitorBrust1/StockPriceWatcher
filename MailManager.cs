using System.Net;
using System.Net.Mail;
using StockPriceWatcher.State;

namespace StockPriceWatcher.Mail;

class MailManager {
    private SmtpClient _smtp;
    private HashSet<MailAddress> _senderSet;

    public MailManager(string host, string username, string password, int smtp_port) {
        _senderSet = new HashSet<MailAddress>();
        _smtp = new SmtpClient(host) {
            Port = smtp_port,
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
        From = new MailAddress("mailtrap@demomailtrap.com"),
        Subject = $"New advancements in regards to the price of the stock: {stock.targetStock}",
        Body = @$"One of your assets that's being tracked, {stock.targetStock}, currently has price of
                  {stock.price} {stock.currency}, which is greater than your upperbound threshold. It's
                  a good time to sell these assets!",
        IsBodyHtml = true,
    };

    private MailMessage CreateBuyMessage(StockState stock) => new() {
        From = new MailAddress("mailtrap@demomailtrap.com"),
        Subject = $"New advancements in regards to the price of the stock: {stock.targetStock}",
        Body = @$"One of your assets that's being tracked, {stock.targetStock}, currently has price of
                  {stock.price} {stock.currency}, which is lesser than your lowerbound threshold. It's
                  a good time to buy some of these assets!",
        IsBodyHtml = true,
    };
}
