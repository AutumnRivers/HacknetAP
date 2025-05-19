using HarmonyLib;
using Hacknet;
using System;
using System.Collections.Generic;
using HacknetArchipelago.Managers;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Hacknet.Gui;
using System.Linq;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class PointClickerPatches
    {
        private static readonly List<int> _collectedIndices = [];
        private static bool _purchasedFinalUpgrade = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointClickerDaemon), "PurchaseUpgrade")]
        public static bool BlockUpdatingRateOnUpgrade(PointClickerDaemon __instance, int index)
        {
            bool doNotBlock = ArchipelagoManager.SlotData.PointClickerMode != "block_upgrade_effects";
            bool canPurchase = __instance.activeState.points >= __instance.upgradeCosts[index];

            if(canPurchase && !doNotBlock)
            {
                if(index == __instance.upgradeCosts.Count - 1 && !_purchasedFinalUpgrade)
                {
                    _purchasedFinalUpgrade = true;
                }
                if(index == __instance.upgradeCosts.Count - 1)
                {
                    AchievementsManager.Unlock("pointclicker_basic", recordAndCheckFlag: true);
                }
                __instance.activeState.points -= (long)__instance.upgradeCosts[index];
            }

            return doNotBlock;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PointClickerDaemon), "PurchaseUpgrade")]
        public static void SendPointClickerUpgrades(int index)
        {
            if (OS.DEBUG_COMMANDS) HacknetAPCore.Logger.LogDebug($"PointClicker Upgrade Index {index} Purchased");
            if (_collectedIndices.Contains(index)) return;
            _collectedIndices.Add(index);
            if (ArchipelagoLocations.UpgradeIndexToLocation.Count <= index) return;
            string locationName = ArchipelagoLocations.UpgradeIndexToLocation[index];
            if (OS.DEBUG_COMMANDS) HacknetAPCore.Logger.LogDebug($"Sending PointClicker Upgrade Index {index}");

            long locationID = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
                locationName);
            if(locationID == -1)
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogWarning($"PointClicker Upgrade Index {index} not found in " +
                        "multiworld. If you're not shuffling PointClicker, you can ignore this.");
                }
                return;
            }

            LocationManager.SendArchipelagoLocations(locationID);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointClickerDaemon), "UpdateRate")]
        public static bool BlockPointClickerUpgrades()
        {
            return ArchipelagoManager.SlotData.PointClickerMode != "block_upgrade_effects";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PointClickerDaemon), "DrawMainScreen")]
        public static void LoadStoredPTCValues()
        {
            PointClickerManager.UseStoredValues();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PointClickerDaemon),"navigatedTo")]
        public static void RefreshStoredDaemon()
        {
            PointClickerManager.RefreshPointClickerDaemon();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointClickerDaemon),"UpdatePoints")]
        public static bool PointClickerUpdatePointsReplacement(PointClickerDaemon __instance)
        {
            if (__instance.activeState == null)
            {
                return false;
            }

            if (__instance.currentRate > 0.0 || __instance.currentRate < -1.0)
            {
                double pointsToAdd = __instance.currentRate * __instance.os.lastGameTime.ElapsedGameTime.TotalSeconds
                    * PointClickerManager.RateMultiplier;
                var newPoints = __instance.activeState.points + (int)pointsToAdd;
                if((newPoints >= __instance.upgradeCosts.Last() || newPoints <= -1.0) && !_purchasedFinalUpgrade)
                {
                    __instance.activeState.points = (long)__instance.upgradeCosts.Last();
                    return false;
                } else if(newPoints <= -1.0)
                {
                    AchievementsManager.Unlock("pointclicker_expert", true);
                }
                __instance.activeState.points = newPoints;
                // No, the (double) shouldn't be needed. Yes, it's in the original code.
                __instance.pointOverflow += (float)(pointsToAdd - (double)(int)pointsToAdd);
                if (__instance.pointOverflow > 1f)
                {
                    int overflow = (int)__instance.pointOverflow;
                    __instance.activeState.points += overflow;
                    __instance.pointOverflow -= overflow;
                }
            }

            __instance.UpdateStory();
            if (__instance.ActiveStory == null)
            {
                __instance.ActiveStory = "";
            }

            __instance.timeSinceLastSave += (float)__instance.os.lastGameTime.ElapsedGameTime.TotalSeconds;
            if (__instance.timeSinceLastSave > 10f)
            {
                __instance.SaveProgress();
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointClickerDaemon), "DrawStatsTextBlock")]
        public static bool PointClickerReplaceMainText(string anouncer, ref string main)
        {
            if(anouncer == "+PPS" && ArchipelagoManager.SlotData.PointClickerMode == "block_upgrade_effects")
            {
                main = "CHECK!";
                return true;
            } else if(anouncer != "PPS") { return true; }

            main += "*" + PointClickerManager.RateMultiplier;

            return true;
        }
    }
}
