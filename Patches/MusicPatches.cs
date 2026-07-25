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
    [HarmonyPatch(typeof(MusicManager), "playSong")]
    // Would you believe me if I told you we only need this for when the game uses it ONCE
    public static void InterceptAndRandomizeIntroSong()
    {
        if(!HacknetAPCore.EnableMusicRando) return;

        var songName = "Music\\" + MusicManager.curentSong.Name;
        
        if (FileManager.GetRandomTrackForBuiltInTrack(songName, out var newSong))
        {
            MusicManager.loadAsCurrentSong(newSong);
        }
    }
}