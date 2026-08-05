using System.Collections.Generic;
using HarmonyLib;
using Hacknet;
using HacknetArchipelago;
using Pathfinder.Util;

namespace HacknetAPClient.Patches;

[HarmonyPatch]
public class AdminAccessPatches
{
    private static Dictionary<string, List<string>> UnlockNodesOnMissionComplete = new()
    {
        { "Aggression must be Punished", ["naixGateway", "themeHackTransComp"] },
        { "Striker's Stash", ["dAttackSource", "dAttackHome"] }
    };

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActiveMission), "finish")]
    public static void LoadMissableNodesOnMissionCompletePatch(ActiveMission __instance)
    {
        if(!HacknetAPCore.SlotData.ShuffleAdminAccess ||
           !UnlockNodesOnMissionComplete.ContainsKey(__instance.email.subject)) return;

        var nodesToUnlock = UnlockNodesOnMissionComplete[__instance.email.subject];
        foreach (var nodeId in nodesToUnlock)
        {
            var node = ComputerLookup.FindById(nodeId);
            if (node == null)
            {
                HacknetAPCore.Logger.LogError($"Failed to add node ID {nodeId} to the netmap: " +
                                              "ComputerLookup returned null! Error!");
                continue;
            }

            var os = OS.currentInstance;
            os.netMap.discoverNode(node);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DLCIntroExe), "UpdateUIBreaking")]
    public static bool PreventNetMapFromBeingCleared()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DLC1SessionUpgrader), "ReDsicoverAllVisibleNodesInOSCache")]
    public static bool PreventReDiscoverFromRunning()
    {
        return false;
    }
}