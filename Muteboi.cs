using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace MuteBoi
{
	static class MuteBoi
	{
		// Sets up a dummy client to use for logging
		public static DiscordClient discordClient = new DiscordClient(new DiscordConfiguration { Token = "DUMMY_TOKEN", TokenType = TokenType.Bot, MinimumLogLevel = LogLevel.Debug });

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}

		public static string GetVersion()
		{
			Version version = Assembly.GetEntryAssembly()?.GetName().Version;
			return version?.Major + "." + version?.Minor + "." + version?.Build + (version?.Revision == 0 ? "" : "-" + (char)(64 + version?.Revision ?? 0));
		}

		private static async Task MainAsync()
		{
			Console.WriteLine("Starting MuteBoi version " + GetVersion() + "...");
			try
			{
				Initialize();

				// Block this task until the program is closed.
				await Task.Delay(-1);
			}
			catch (Exception e)
			{
				Console.WriteLine("Fatal error:");
				Console.WriteLine(e);
				Console.ReadLine();
			}
		}

		public static async void Initialize()
		{
			Console.WriteLine("Loading config \"" + Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "config.yml\"");
			Config.LoadConfig();

			// Check if token is unset
			if (Config.token == "<add-token-here>" || string.IsNullOrWhiteSpace(Config.token))
			{
				Console.WriteLine("You need to set your bot token in the config and start the bot again.");
				throw new ArgumentException("Invalid Discord bot token");
			}

			// Database connection and setup
			try
			{
				Console.WriteLine("Connecting to database...");
				Database.SetConnectionString(Config.hostName, Config.port, Config.database, Config.username, Config.password);
				Database.SetupTables();
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not set up database tables, please confirm connection settings, status of the server and permissions of MySQL user. Error: " + e);
				throw;
			}

			Console.WriteLine("Setting up Discord client...");

			// Checking log level
			if (!Enum.TryParse(Config.logLevel, true, out LogLevel logLevel))
			{
				Console.WriteLine("Log level " + Config.logLevel + " invalid, using 'Info' instead.");
				logLevel = LogLevel.Information;
			}

			// Setting up client configuration
			DiscordConfiguration cfg = new DiscordConfiguration
			{
				Token = Config.token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = logLevel,
				AutoReconnect = true,
				Intents = DiscordIntents.All
			};

			discordClient = new DiscordClient(cfg);

			Console.WriteLine("Hooking events...");
			discordClient.Ready += EventHandler.OnReady;
			discordClient.GuildAvailable += EventHandler.OnGuildAvailable;
			discordClient.ClientErrored += EventHandler.OnClientError;
			discordClient.GuildMemberAdded += EventHandler.OnGuildMemberAdded;
			discordClient.GuildMemberRemoved += EventHandler.OnGuildMemberRemoved;

			Console.WriteLine("Connecting to Discord...");
			await discordClient.ConnectAsync();
		}
	}
}
