using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace NowPlayingParser
{
    /// <summary>
    /// Class to start a bot that gets now playing information from iTunes and display it as 'Playing' status on discord.
    /// </summary>
    class NowPlayingBot
    {
        // Discord client
        static DiscordClient discord;
        // Initialize iTunes client (will start up iTunes)
        static iTunesLib.iTunesApp iTunesClient;
        string currentTrack = null;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("NowPlayingParser Initializing...");
            if(!System.IO.File.Exists("token.txt"))
            {
                Console.WriteLine("Please create 'token.txt' and write your token in it.\n");
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(1);
            }
            string token = System.IO.File.ReadAllText("token.txt");
            discord = new DiscordClient(new DiscordConfiguration {
                Token = token,
                TokenType = TokenType.User
            });
            // setup iTunes event handler
            iTunesClient = new iTunesLib.iTunesApp();
            iTunesClient.OnPlayerPlayEvent += ITunesClient_OnPlayerPlayEvent;
            iTunesClient.OnPlayerStopEvent += ITunesClient_OnPlayerStopEvent;
            // connect to discord and get Username
            await discord.ConnectAsync();
            Console.WriteLine("Connected to Discord!");
            // get current user
            DiscordUser userObject = discord.CurrentUser;
            Console.WriteLine("Connected as: " + userObject.Username);
            // wait
            await Task.Delay(-1);
        }

        private static void ITunesClient_OnPlayerPlayEvent(object iTrack)
        {
            iTunesLib.IITTrack currentTrack = (iTunesLib.IITTrack)iTrack;
            string trackName = currentTrack.Name;
            string artistName = currentTrack.Artist;
            string trackInfo = trackName + " - " + artistName;
            if(trackInfo.Equals(currentTrack))
            {
                Console.WriteLine("Player Resumed.");
            }
            else
            {
                Console.WriteLine("Now Playing: " + trackInfo);
            }
            // update playing status
            Game trackInfoWrap = new Game("♫ " + trackInfo);
            discord.UpdateStatusAsync(trackInfoWrap, null, null);
        }

        private static void ITunesClient_OnPlayerStopEvent(object iTrack)
        {
            Console.WriteLine("Player Stopped.");
            discord.UpdateStatusAsync(null, null, null);
        }
    }
}
