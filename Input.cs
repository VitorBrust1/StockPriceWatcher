namespace StockPriceWatcher.Input;

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
