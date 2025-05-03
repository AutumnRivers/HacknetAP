using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;
using HarmonyLib;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class PortHackHeartPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PorthackHeartDaemon),"BreakHeart")]
        public static void CheckPortHackHeartForGoal()
        {
            if (HacknetAPCore.SlotData.PlayerGoal == HacknetAPSlotData.VictoryCondition.Heartstopper)
            {
                HacknetAPCore.SendVictory();
            };
        }
    }
}
