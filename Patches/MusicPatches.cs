using Hacknet;
using HacknetArchipelago;
using HacknetArchipelago.Managers;
using HarmonyLib;

namespace HacknetAPClient.Patches;

[HarmonyPatch]
public class MusicPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicManager), "transitionToSong")]
    public static void InterceptAndRandomizeSong(ref string songName)
    {
        if(!HacknetAPCore.EnableMusicRando) return;

        if (FileManager.GetRandomTrackForBuiltInTrack(songName, out var newSong))
        {
            songName = newSong;
        }
    }

    [HarmonyPrefix]
    // ReSharper disable once StringLiteralTypo
    [HarmonyPatch(typeof(MusicManager), "playSongImmediatley")]
    public static void InterceptAndRandomizeSongImmediately(ref string songname)
    {
        if(!HacknetAPCore.EnableMusicRando) return;

        if (FileManager.GetRandomTrackForBuiltInTrack(songname, out var newSong))
        {
            songname = newSong;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicManager), "loadAsCurrentSong")]
    public static void InterceptAndRandomizeLoadedSong(ref string songname)
    {
        if(!HacknetAPCore.EnableMusicRando) return;

        if (FileManager.GetRandomTrackForBuiltInTrack(songname, out var newSong))
        {
            songname = newSong;
        }
    }
}
