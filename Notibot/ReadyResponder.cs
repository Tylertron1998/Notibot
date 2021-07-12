using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Notibot
{
	public class ReadyResponder : IResponder<GuildCreate>
	{
		private readonly SlashService _slashService;

		public ReadyResponder(SlashService slashService)
		{
			_slashService = slashService;
		}

		public async Task<Result> RespondAsync(GuildCreate gatewayEvent, CancellationToken ct = new CancellationToken())
		{
			await _slashService.UpdateSlashCommandsAsync(gatewayEvent.ID);
			return Result.FromSuccess();
		}
	}
}