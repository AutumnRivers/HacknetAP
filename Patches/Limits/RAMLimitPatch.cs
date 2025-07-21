using Hacknet;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Pathfinder.Event.Gameplay;

namespace HacknetArchipelago.Patches
{
    public class RAMLimitPatch
    {
        public static bool ramWasSet = false;
        internal static int _lastRamLimit = -1;

        public static void LimitRAM(OSUpdateEvent oSUpdateEvent)
        {
            if(HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.OnlyRAM &&
                HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.EnableAllLimits)
            {
                return;
            }
            if(InventoryManager._ramLimit == 0 || OS.currentInstance.initShowsTutorial) { return; }

            OS os = oSUpdateEvent.OS;

            if(_lastRamLimit != InventoryManager._ramLimit)
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogDebug($"Updating RAM to new value: {InventoryManager._ramLimit}");
                }

                os.ramAvaliable = InventoryManager._ramLimit;
                os.totalRam = InventoryManager._ramLimit - (OS.TOP_BAR_HEIGHT + 2);
                _lastRamLimit = InventoryManager._ramLimit;

                UpdateRamModule();
            }
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
