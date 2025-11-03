using Hacknet;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Pathfinder.Event.Gameplay;
using System;

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
            if(InventoryManager._ramLimit == 0 || OS.currentInstance.initShowsTutorial) { return; }

            OS os = oSUpdateEvent.OS;

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
            OS os = OS.currentInstance;

            os.modules.Remove(os.ram);
            os.ram = new RamModule(new Rectangle(2, OS.TOP_BAR_HEIGHT,
                RamModule.MODULE_WIDTH, os.ramAvaliable + RamModule.contentStartOffset), os);
            os.ram.name = "RAM";
            os.modules.Add(os.ram);
        }
    }
}
