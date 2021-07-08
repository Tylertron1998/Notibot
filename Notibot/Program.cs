using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;

namespace Notibot
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("Token not provided, you idiot.");

			var services = new ServiceCollection()
				.AddLogging(c => c.AddConsole())
				.AddDiscordGateway(_ => token)
				.AddDiscordCommands(true)
				.AddCommandGroup<YoutubeCommand>()
				.AddDiscordCaching()
				.AddSingleton<NotificationEventHandler>()
				.BuildServiceProvider(true);

			var log = services.GetRequiredService<ILogger<Program>>();

			var handler = services.GetRequiredService<NotificationEventHandler>();
			
			var slashService = services.GetRequiredService<SlashService>();

			var checkSlashSupport = slashService.SupportsSlashCommands();
			if (!checkSlashSupport.IsSuccess)
			{
				log.LogWarning
				(
					"The registered commands of the bot don't support slash commands: {Reason}",
					checkSlashSupport.Unwrap().Message
				);
			}
			else
			{
				var updateSlash = await slashService.UpdateSlashCommandsAsync();
				if (!updateSlash.IsSuccess)
				{
					log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Unwrap().Message);
				}
			}

			var gatewayClient = services.GetRequiredService<DiscordGatewayClient>();

			Task.Run(handler.RunAsync);
			
			var runResult = await gatewayClient.RunAsync(CancellationToken.None);
		}
	}
}