using Hacknet;
using Pathfinder.Event.Gameplay;

namespace HacknetArchipelago.Patches
{
    public class RAMLimitPatch
    {
        public static bool ramWasSet = false;
        private static int _lastRamLimit = HacknetAPCore._ramLimit;

        public static void LimitRAM(OSUpdateEvent oSUpdateEvent)
        {
            if(HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.OnlyRAM &&
                HacknetAPCore.SlotData.LimitsShuffle != HacknetAPSlotData.LimitsMode.EnableAllLimits)
            {
                return;
            }

            OS os = oSUpdateEvent.OS;

            if(!ramWasSet || _lastRamLimit != HacknetAPCore._ramLimit)
            {
                os.ramAvaliable -= HacknetAPCore._ramLimit - os.totalRam;
                os.totalRam = HacknetAPCore._ramLimit - (OS.TOP_BAR_HEIGHT + 2);
                ramWasSet = true;
                _lastRamLimit = HacknetAPCore._ramLimit;
            }
        }
    }
}
