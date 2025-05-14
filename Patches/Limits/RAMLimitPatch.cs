using Hacknet;
using Pathfinder.Event.Gameplay;

using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches
{
    public class RAMLimitPatch
    {
        public static bool ramWasSet = false;
        private static int _lastRamLimit = InventoryManager._ramLimit;

        public static void LimitRAM(OSUpdateEvent oSUpdateEvent)
        {
            if(HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.OnlyRAM &&
                HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.EnableAllLimits)
            {
                return;
            }

            OS os = oSUpdateEvent.OS;

            if(!ramWasSet || _lastRamLimit != InventoryManager._ramLimit)
            {
                os.ramAvaliable -= InventoryManager._ramLimit - os.totalRam;
                os.totalRam = InventoryManager._ramLimit - (OS.TOP_BAR_HEIGHT + 2);
                ramWasSet = true;
                _lastRamLimit = InventoryManager._ramLimit;
            }
        }
    }
}
