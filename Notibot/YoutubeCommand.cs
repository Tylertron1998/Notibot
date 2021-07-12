using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Skyra.Grpc.Services;
using Skyra.Grpc.Services.Shared;
using Action = Skyra.Grpc.Services.Action;
using Result = Remora.Results.Result;

namespace Notibot
{
	public class YoutubeCommand : CommandGroup
	{
		private readonly InteractionContext _context;
		private readonly IDiscordRestWebhookAPI _webhookAPI;

		public YoutubeCommand(InteractionContext context, IDiscordRestWebhookAPI webhookApi)
		{
			_context = context;
			_webhookAPI = webhookApi;
		}

		[Command("subscribe")]
		public async Task<IResult> SubscribeToChannelAsync([Description("Channel URL")] string channelUrl, [Description("Notification Message")] string notificationMessage)
		{
			var channel = GrpcChannel.ForAddress("http://localhost:5002");
			var client = new YoutubeSubscription.YoutubeSubscriptionClient(channel);

			try
			{
				var result = await client.ManageSubscriptionAsync(new SubscriptionManageRequest
				{
					Type = Action.Subscribe,
					GuildChannelId = _context.ChannelID.ToString(),
					ChannelUrl = channelUrl,
					GuildId = _context.GuildID.Value.ToString(),
					NotificationMessage = notificationMessage
				});
				if (result.Status == Status.Failed) throw new Exception(); // dirty
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: ":c");
				return Result.FromError(new Result<string>());
			}

			await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: "c:");

			return Result.FromSuccess();
		}
		
		[Command("unsubscribe")]
		public async Task<IResult> UnsubscribeToChannelAsync([Description("Channel URL")] string channelUrl)
		{
			var channel = GrpcChannel.ForAddress("http://localhost:5002");
			var client = new YoutubeSubscription.YoutubeSubscriptionClient(channel);

			try
			{
				var result = await client.ManageSubscriptionAsync(new SubscriptionManageRequest
				{
					Type = Action.Unsubscribe,
					GuildChannelId = _context.ChannelID.ToString(),
					ChannelUrl = channelUrl,
					GuildId = _context.GuildID.Value.ToString(),
					NotificationMessage = "Hello there!"
				});
				if (result.Status == Status.Failed) throw new Exception(); // dirty
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: ":c");
				return Result.FromError(new Result<string>());
			}

			await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: "c:");

			return Result.FromSuccess();
		}
		
		[Command("message")]
		public async Task<IResult> SetUploadMessageAsync([Description("The upload message")] string uploadMessage)
		{
			var channel = GrpcChannel.ForAddress("http://localhost:5002");
			var client = new YoutubeSubscription.YoutubeSubscriptionClient(channel);

			try
			{
				var result = await client.UpdateSubscriptionSettingsAsync(new NotificationSettingsUpdate
				{
					Channel = _context.ChannelID.ToString(),
					Message = uploadMessage
				});
				if (result.Status == Status.Failed) throw new Exception(); // dirty
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: ":c");
				return Result.FromError(new Result<string>());
			}

			await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, content: "c:");

			return Result.FromSuccess();
		}

		[Command("list")]
		public async Task<IResult> GetSubscriptionsAsync()
		{
			var channel = GrpcChannel.ForAddress("http://localhost:5002");
			var client = new YoutubeSubscription.YoutubeSubscriptionClient(channel);

			var result = await client.GetSubscriptionsAsync(new Empty());

			var interestedIn = result.Subscriptions.Where(sub => sub.GuildIds.Contains(_context.GuildID.ToString()));
			
			if (interestedIn.Any())
			{
				var embedFields = new List<EmbedField>();

				foreach (var sub in interestedIn)
				{
					embedFields.Add(new EmbedField($"• {sub.ChannelTitle}", $"https://youtube.com/channel/{sub.ChannelId}"));
				}

				await _webhookAPI.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, embeds: new[] {new Embed(Fields: embedFields)});
			}
			return Result.FromSuccess();
		}
	}
}