using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using PizzaBot.Models;
using PizzaBot.Bot.Commands.AdminCommands;
using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.Commands.ClientCommands;
using PizzaBot.Bot.BotService;
using PizzaBot.Services;
using PizzaBot.Bot.BotCommands.AdminCommands;
using PizzaBot.Bot.BotCommands.ClientCommands;
using PizzaBot.Bot.BotCommands.CurierCommands;
using PizzaBot.Bot.Commands.CurierCommands;

namespace PizzaBot;

public class Program
{
    public static async Task Main()
    {
        const string BOT_TOKEN = "7648532936:AAGOOcreRc4Czf9HKCY_EZHK2gFG13bnpxY";

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .Filter.ByExcluding(logEvent =>
                logEvent.Properties.ContainsKey("SourceContext") &&
                logEvent.Properties["SourceContext"].ToString().Contains("System.Net.Http"))
            .CreateLogger();

        var builder = Host.CreateDefaultBuilder()
            .UseSerilog(logger)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<Serilog.ILogger>(provider => logger);
                services.AddSingleton<ITelegramBotClient>(sp => new TelegramBotClient(BOT_TOKEN));
                services.AddSingleton<BotService>();
                services.AddSingleton<UpdateHandler>();
                services.AddSingleton<UserStateMachine>();
                services.AddSingleton<CommandHandlerFactory>();
                services.AddSingleton<PlaceOrderCommandHandler>();
                services.AddSingleton<ICommandHandler, StartCommandHandler>();
                services.AddSingleton<ICommandHandler, HelpCommandHandler>();
                services.AddSingleton<ICommandHandler, CatalogCommandHandler>();
                services.AddSingleton<ICommandHandler, PlaceOrderCommandHandler>();
                services.AddSingleton<ICommandHandler, CheckClientOrderHistoryCommandHandler>();
                services.AddSingleton<ICommandHandler, CourierCommandHandler>();
                services.AddSingleton<ICommandHandler, AdminCommandHandler>();
                services.AddSingleton<ICommandHandler, ProcessOrderCommandHandler>();
                services.AddSingleton<ICommandHandler, TakeOrderCommandHandler>();
                services.AddSingleton<ICommandHandler, CheckActiveOrderCommandHandler>();
                services.AddSingleton<ICommandHandler, CheckCourierOrderHistoryCommandHandler>();
                services.AddSingleton<ICommandHandler, CheckIngredientsCommandHandler>();
                services.AddSingleton<ICommandHandler, AddProductCommandHandler>();
                services.AddSingleton<ICommandHandler, DeleteProductCommandHandler>();
                services.AddSingleton<ICommandHandler, AddIngridientsCommandHandler>();
                services.AddSingleton<ICommandHandler, ChangeBaseCostCommandHandler>();
                services.AddSingleton<ICommandHandler, ChangeCostPerKilometerCommandHandler>();
                services.AddSingleton<PostgresContext>();
                services.AddSingleton<DBService>();
                services.AddSingleton<DeliveryCalculator>();
                services.AddSingleton<MapApiService>();
                services.AddHttpClient<MapApiService>();
                services.AddHostedService<BotService>();
            })
            .Build();

        await builder.RunAsync();
    }
}