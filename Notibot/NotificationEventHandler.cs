using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Skyra.Grpc.Services;

namespace Notibot
{
	public class NotificationEventHandler
	{
		private readonly IDiscordRestChannelAPI _ChannelApi;

		public NotificationEventHandler(IDiscordRestChannelAPI channelApi)
		{
			_ChannelApi = channelApi;
		}

		public async Task RunAsync()
		{
			var channel = GrpcChannel.ForAddress("http://localhost:5002");
			var client = new YoutubeSubscription.YoutubeSubscriptionClient(channel);

			using var call = client.SubscriptionNotifications(new Empty());

			await foreach (var response in call.ResponseStream.ReadAllAsync())
			{
				foreach (var discordChannel in response.Channels)
				{
					var id = discordChannel.ChannelId;
					var message = discordChannel.Message + $" https://youtube.co.uk/watch?v={response.VideoId}";

					await _ChannelApi.CreateMessageAsync(new Snowflake(ulong.Parse(id)), content: message);
				}
			}
		}
	}
}