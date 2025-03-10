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

namespace PizzaBot;

public class Program
{
    public static async Task Main()
    {
        const string BOT_TOKEN = "7648532936:AAGOOcreRc4Czf9HKCY_EZHK2gFG13bnpxY";

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
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
                services.AddSingleton<ICommandHandler, StartCommandHandler>();
                services.AddSingleton<ICommandHandler, HelpCommandHandler>();
                services.AddSingleton<ICommandHandler, AdminCommandHandler>();
                services.AddSingleton<ICommandHandler, CatalogCommandHandler>();
                services.AddSingleton<ICommandHandler, CheckIngredientsCommandHandler>();
                services.AddSingleton<ICommandHandler, AddProductCommandHandler>();
                services.AddSingleton<ICommandHandler, AddIngridientsCommandHandler>();
                services.AddSingleton<PostgresContext>();
                services.AddSingleton<DBService>();
                services.AddSingleton<MapApiService>();
                services.AddHttpClient<MapApiService>();
                services.AddHostedService<BotService>();
            })
            .Build();

        await builder.RunAsync();
    }
}