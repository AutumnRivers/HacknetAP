using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet;
// ReSharper disable UseVerbatimString

namespace HacknetArchipelago.Managers;

public static class FileManager
{
    public const string MUSIC_DIRECTORY_NAME = "/APMusic";
    public const int RANDOM_MUSIC_AMOUNT = 23;
    public static List<string> RandomMusicTracks { get; set; } = [];

    public static List<string> HacknetTracks { get; } =
    [
        "Music\\Bit(Ending)",
        "Music\\Broken_Boy",
        "Music\\Irritations",
        "Music\\out_run_the_wolves",
        "Music\\Revolve",
        "Music\\Rico_Puestel-Roja_Drifts_By",
        "Music\\Roller_Mobster_Clipped",
        "Music\\Ryan3",
        "Music\\Ryan10",
        "Music\\tetrameth",
        "Music\\The_Quickening",
        "Music\\TheAlgorithm",
        "Music\\Traced",
        "Music\\Revolve",
        "DLC\\Music\\DreamHead",
        "DLC\\Music\\HOME_Resonance",
        "DLC\\Music\\Remi_Finale",
        "DLC\\Music\\Remi2",
        "DLC\\Music\\RemiDrone",
        "DLC\\Music\\Slow_Motion",
        "DLC\\Music\\snidelyWhiplash",
        "DLC\\Music\\Userspacelike",
        "DLC\\Music\\World_Chase"
    ];

    private static Random _random;

    internal static void InitRandomMusic(int seed)
    {
        _random = new Random(seed);
        
        var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (!Directory.Exists(pluginDirectory + MUSIC_DIRECTORY_NAME))
        {
            HacknetAPCore.Logger.LogError("Unable to load random music tracks - folder doesn't exist!");
            return;
        }

        var musicDirectory = new DirectoryInfo(pluginDirectory + MUSIC_DIRECTORY_NAME);
        var files = musicDirectory.GetFiles().Where(f => f.Name.EndsWith(".ogg")).ToList();

        var allTracks = files.Select(f => f.Name).ToList();
        allTracks.AddRange(HacknetTracks);

        for (var idx = 0; idx < RANDOM_MUSIC_AMOUNT; idx++)
        {
            var listIndex = _random.Next(0, allTracks.Count);
            var trackPath = allTracks[listIndex];

            if (RandomMusicTracks.Contains(trackPath))
            {
                idx--;
                continue;
            }

            if (trackPath.EndsWith(".ogg"))
            {
                trackPath = "../BepInEx/plugins/APMusic/" + trackPath;
            }
            
            RandomMusicTracks.Add(trackPath);
        }
    }

    public static bool GetRandomTrackForBuiltInTrack(string songName, out string newSong)
    {
        newSong = string.Empty;
        
        var songExists = HacknetTracks.Contains(songName);
        if (!songExists)
        {
            HacknetAPCore.Logger.LogWarning($"Couldn't randomize track {songName} - " +
                                            "is it not a built-in track?!");
            return false;
        }

        var songIndex = HacknetTracks.IndexOf(songName);
        newSong = RandomMusicTracks[songIndex];

        return true;
    }
}