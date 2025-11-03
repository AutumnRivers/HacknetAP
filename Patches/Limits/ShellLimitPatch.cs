using Hacknet;
using Pathfinder.Event.Gameplay;
using System.Linq;

using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;

namespace HacknetArchipelago.Patches
{
    public class ShellLimitPatch
    {
        public const int MINIMUM_SHELLS = 0;
        public const int MAXIMUM_SHELLS = 10;

        public static void LimitShells(CommandExecuteEvent executeEvent)
        {
            string cmd = executeEvent.Args[0].ToLower();

            if (cmd != "shell" ||
                HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyRAM ||
                HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.Disabled)
            {
                return;
            }

            OS os = executeEvent.Os;

            int startingShells = HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShellsZero ?
                0 : 1;

            var currentlyOpenShells = os.exes.Count(exe => exe.GetType() == typeof(ShellExe));
            var shellLimit = startingShells + InventoryManager.ProgressiveShellLimitsCollected;

            shellLimit = (int)MathHelper.Clamp(shellLimit, MINIMUM_SHELLS, MAXIMUM_SHELLS);

            if (shellLimit < 0) return;

            var newOpenAmount = currentlyOpenShells + 1;
            string errorText = string.Format("ERROR : Maximum Shell Limit ({0}) Reached", InventoryManager._shellLimit);

            if (newOpenAmount > shellLimit)
            {
                executeEvent.Cancelled = true;
                os.terminal.writeLine(errorText);
                return;
            }
        }
    }
}
