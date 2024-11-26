using Discord;
using Discord.WebSocket;
using DiscordBot;

public class Program
{
    private static DiscordSocketClient _client = new DiscordSocketClient();
    public static async Task Main()
    {
        var token = "discord login token";

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();


        await Task.Delay(-1);

    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}

//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices(services =>
//    {
//        services.AddHostedService<Worker>();
//    })
//    .Build();

//await host.RunAsync();
