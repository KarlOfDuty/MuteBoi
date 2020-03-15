using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace MuteBoi
{
	class EventHandler
	{
		private DiscordClient discordClient;
		internal EventHandler(DiscordClient client)
		{
			this.discordClient = client;
			
			
			this.discordClient.Ready += this.OnReady;
			this.discordClient.GuildAvailable += this.OnGuildAvailable;
			this.discordClient.ClientErrored += this.OnClientError;
			this.discordClient.GuildMemberRemoved += this.OnGuildMemberRemoved;
			this.discordClient.GuildMemberAdded += this.OnGuildMemberAdded;
		}

		private Task OnReady(ReadyEventArgs e)
		{
			this.discordClient.DebugLogger.LogMessage(LogLevel.Info, "MuteBoi", "Client is ready to process events.", DateTime.UtcNow);
			this.discordClient.UpdateStatusAsync(new DiscordGame("memory games"), UserStatus.Online);
			return Task.CompletedTask;
		}

		private Task OnGuildAvailable(GuildCreateEventArgs e)
		{
			this.discordClient.DebugLogger.LogMessage(LogLevel.Info, "MuteBoi", $"Guild available: {e.Guild.Name}", DateTime.UtcNow);

			IReadOnlyList<DiscordRole> roles = e.Guild.Roles;

			foreach (DiscordRole role in roles)
			{
				this.discordClient.DebugLogger.LogMessage(LogLevel.Info, "MuteBoi", role.Name.PadRight(40, '.') + role.Id, DateTime.UtcNow);
			}
			return Task.CompletedTask;
		}

		private Task OnClientError(ClientErrorEventArgs e)
		{
			this.discordClient.DebugLogger.LogMessage(LogLevel.Error, "MuteBoi", $"Exception occured: {e.Exception.GetType()}: {e.Exception}", DateTime.UtcNow);
			return Task.CompletedTask;
		}

		private Task OnGuildMemberRemoved(GuildMemberRemoveEventArgs e)
		{
			Console.Write("LEAVE" + e.Member.Roles.Count());
			foreach (DiscordRole role in e.Member.Roles)
			{
				if (Config.trackedRoles.Contains(role.Id))
				{
					Database.TryAddRole(e.Member.Id, role.Id);
				}
			}
			return Task.CompletedTask;
		}

		private async Task OnGuildMemberAdded(GuildMemberAddEventArgs e)
		{
			Console.Write("JOIN");
			if (!Database.TryGetRoles(e.Member.Id, out List<Database.SavedRole> savedRoles)) return;

			foreach (Database.SavedRole savedRole in savedRoles)
			{
				try
				{
					DiscordRole role = e.Guild.GetRole(savedRole.roleID);
					await e.Member.GrantRoleAsync(role);
				}
				catch (NotFoundException) {}
				catch (UnauthorizedException) {}
			}

			Database.TryRemoveRoles(e.Member.Id);
		}
	}
}
