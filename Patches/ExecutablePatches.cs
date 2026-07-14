using Hacknet;
using HarmonyLib;

namespace HacknetAPClient.Patches;

[HarmonyPatch]
public class ExecutablePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DLCIntroExe), "StartAssignment")]
    public static void LowerKaguyaTrialsRamCostPatch(DLCIntroExe __instance)
    {
        // This is to address an issue with RAM limits
        __instance.ramCost = 150;
    }
}