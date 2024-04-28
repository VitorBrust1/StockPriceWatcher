namespace StockPriceWatcher.Config;

class JSONConfig {
    public class ApiConfig {
        public string key {get; set;} = string.Empty;
        public int delay {get; set;}
    }

    public class MailConfig {
        public string smtp_server {get; set;} = string.Empty;
        public string smtp_user {get; set;} = string.Empty;
        public string smtp_password {get; set;} = string.Empty;
        public List<string> sender_list {get; set;} = new List<string>();
    }

    public ApiConfig? api {get; set;}
    public MailConfig? email {get; set;}
}
