﻿using System;
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
	static class EventHandler
	{
		public static Task OnReady(DiscordClient client, ReadyEventArgs e)
		{
			Logger.Log(LogID.DISCORD, "Client is ready to process events.");

			// Checking activity type
			if (!Enum.TryParse(Config.presenceType, true, out ActivityType activityType))
			{
				Console.WriteLine("Presence type '" + Config.presenceType + "' invalid, using 'Playing' instead.");
				activityType = ActivityType.Playing;
			}

			client.UpdateStatusAsync(new DiscordActivity(Config.presenceText, activityType), UserStatus.Online);
			return Task.CompletedTask;
		}

		public static Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e)
		{
			Logger.Log(LogID.DISCORD, "Guild available: " + e.Guild.Name);

			IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

			foreach ((ulong roleID, DiscordRole role) in roles)
			{
				Logger.Log(LogID.DISCORD, role.Name.PadRight(40, '.') + roleID);
			}
			return Task.CompletedTask;
		}

		public static Task OnClientError(DiscordClient client, ClientErrorEventArgs e)
		{
			Logger.Error(LogID.DISCORD, "Exception occured:\n" + e.Exception);
			return Task.CompletedTask;
		}

		public static Task OnGuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
		{
			foreach (DiscordRole role in e.Member.Roles)
			{
				if (Config.trackedRoles.Contains(role.Id))
				{
					Logger.Log(LogID.DISCORD, e.Member.DisplayName + " (" + e.Member.Id + ") left the server with tracked role '" + role.Name + "'.");
					Database.TryAddRole(e.Member.Id, role.Id);
				}
			}
			return Task.CompletedTask;
		}

		public static async Task OnGuildMemberAdded(DiscordClient client, GuildMemberAddEventArgs e)
		{
			if (!Database.TryGetRoles(e.Member.Id, out List<Database.SavedRole> savedRoles)) return;

			foreach (Database.SavedRole savedRole in savedRoles)
			{
				try
				{
					
					DiscordRole role = e.Guild.GetRole(savedRole.roleID);
					Logger.Log(LogID.DISCORD, e.Member.DisplayName + " (" + e.Member.Id + ") were given back the role '" + role.Name + "' on rejoin. ");
					await e.Member.GrantRoleAsync(role);
				}
				catch (NotFoundException) {}
				catch (UnauthorizedException) {}
			}

			Database.TryRemoveRoles(e.Member.Id);
		}
	}
}
