using Hacknet;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Pathfinder.Event.Gameplay;
using System;
using System.Linq;

namespace HacknetArchipelago.Patches
{
    public class RAMLimitPatch
    {
        public static bool ramWasSet = false;
        internal static int _lastRamLimit = -1;

        public const int MINIMUM_RAM = 350;
        public const int RAM_UPGRADE_STEP = 50;
        public const int MAXIMUM_RAM = 800;

        public static void LimitRAM(OSUpdateEvent oSUpdateEvent)
        {
            if(HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.OnlyRAM &&
                HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.EnableAllLimits)
            {
                return;
            }
            var os = oSUpdateEvent.OS;
            
            if(os.initShowsTutorial && !HacknetAPCore.KilledTutorial) return;
            if (!HacknetAPCore.KilledTutorial)
            {
                HacknetAPCore.KilledTutorial = true;
            }

            int totalRam = GetRAMLimit();

            if(_lastRamLimit != totalRam)
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogDebug($"Updating RAM to new value: {InventoryManager._ramLimit}");
                }

                os.ramAvaliable = totalRam;
                os.totalRam = totalRam - (OS.TOP_BAR_HEIGHT + 2);
                _lastRamLimit = totalRam;

                UpdateRamModule();
            }
        }

        public static int GetRAMLimit()
        {
            var ramUpgradesCollected = InventoryManager.ProgressiveRAMsCollected;
            int totalRam = MINIMUM_RAM + (ramUpgradesCollected * RAM_UPGRADE_STEP);

            totalRam = (int)MathHelper.Clamp(totalRam, MINIMUM_RAM, MAXIMUM_RAM);

            return totalRam;
        }

        public static void UpdateRamModule()
        {
            var os = OS.currentInstance;

            var ram = os.ram;
            ram.bounds.Height = os.ramAvaliable + RamModule.contentStartOffset;
            os.modules.Remove(os.ram);
            os.ram = ram;
            os.ram.name = "RAM";
            os.modules.Add(os.ram);
        }
    }
}
