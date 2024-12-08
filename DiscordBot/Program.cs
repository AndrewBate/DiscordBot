using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;

// Copypasta from Example

namespace DiscordBot;
public class Program {

    private static IConfiguration _configuration;
    private static DiscordSocketClient _client;
    private static IServiceProvider _services;

    private static readonly DiscordSocketConfig _socketConfig = new() {
        GatewayIntents = GatewayIntents.AllUnprivileged,
    };

    private static readonly InteractionServiceConfig _interactionServiceConfig = new() {
        
    };


    public static async Task Main() {
        _configuration = new ConfigurationBuilder().Build();

        _services = new ServiceCollection()
           .AddSingleton(_configuration)
           .AddSingleton(_socketConfig)
           .AddSingleton<DiscordSocketClient>()
           .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig))
           .AddSingleton<InteractionHandler>()
           .BuildServiceProvider();

        _client = _services.GetRequiredService<DiscordSocketClient>();
        _client.Log += Log;


        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

        Console.WriteLine("login");
        var token = "NzgwODM0OTcwMzYxMDA0MDMy.GZaS_1.tO2qaO7f3OFEfkFVw0UcqtnZad5K0CxXhHdmCA";      
        await _client.LoginAsync(TokenType.Bot, token);

        Console.WriteLine("start");
        await _client.StartAsync();

        Console.WriteLine("delay -1");
        await Task.Delay(-1);

    }

    private static Task Log(LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }





}

public class MyInteractionModule : InteractionModuleBase<SocketInteractionContext> {
    [Command("spawner")]
    public async Task Spawn() {
        var builder = new ComponentBuilder()
            .WithButton("label", "custom-id");

        await ReplyAsync("Here is a button!", components: builder.Build());

    }

}
//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices(services =>
//    {
//        services.AddHostedService<Worker>();
//    })
//    .Build();

//await host.RunAsync();
