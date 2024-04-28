# Stock Price Watcher

## Overview

Stock Price Watcher is a C# terminal application created to track stock prices on the [B3](https://www.b3.com.br/en_us/) stock market (The main financial exchange in Brazil) and send email alerts to users when a particular asset's price drops below a set buying threshold or rises above a selling threshold. This text-based utility functions without a visual interface, offering a convenient method to keep up-to-date on market trends.
## Configuration

Prior to utilizing Stock Price Watcher, it is necessary to establish a configuration file in JSON format named **appConfig.json** within the same directory as the **StockPriceWatcher.exe** file. This particular file must contain the subsequent details:
- The Api settings used to acess the stock prices, in this case the key.
- An delay time (in milliseconds) used to check how often the program checks for stock price changes and sends the emails.
- The SMTP server settings including, the host, the username, and the password.
-  A list of destination email addresses for alerts.

Here's an example of the configuration file:

```json
{
    "api": {
      "APIToken": "your_api_key",
      "updateDelay": 1800000
    },
    "email": {
        "smtp_server": "smtp.example.com",
        "smtp_user": "your_username",
        "smtp_password": "your_password",
        "senderList": [
          "user1@email.com",
          "user2@email.com",
          "..."
        ]
    }
}
```

The project relies on the API offered by [Brapi](https://brapi.dev/) under their free license for communication. This necessitates adhering to the API structure for deserializing the response JSON file.

To handle emails, the project utilizes a free testing SMTP server from [MailTrap](https://mailtrap.io/) under their free license.

## Usage

To utilize the Stock Price Watcher, use these instructions:

1. Duplicate this repository to your local device.

2. Compile the application utilizing your C# compiler.

3. Construct the JSON configuration file (as depicted above) and store it as `appConfig.json` in the project folder (the same one as the executable file).

4. Launch a terminal or command prompt and navigate to the project directory.

5. Execute the application with the subsequent command, providing three command-line arguments:

   - The initial argument is the symbol of the asset to observe (e.g., PETR4).
   - The second argument is the selling price.
   - The third argument is the buying price.
   Example usage:

   ```shell
   StockPriceWatcher.exe PETR4 36,4 32,5
   ```

Stock Price Watcher will keep an eye on the stock prices using the API key and update time specified. If the price goes below the buying threshold or above the selling threshold, the system will send email alerts to the chosen recipients.
## Project Structure

The project is organized into multiple classes and utilizes the Observer design pattern to efficiently achieve its goals.
