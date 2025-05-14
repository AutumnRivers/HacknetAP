using Hacknet;
using Pathfinder.Event.Gameplay;
using System.Linq;

using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches
{
    public class ShellLimitPatch
    {
        public static void LimitShells(ExecutableExecuteEvent executeEvent)
        {
            if(executeEvent.ExecutableName != "shell" ||
                HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyRAM ||
                HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.Disabled)
            {
                return;
            }

            OS os = executeEvent.OS;

            var currentlyOpenShells = os.exes.Count(exe => exe.GetType() == typeof(ShellExe));
            var shellLimit = InventoryManager._shellLimit;

            var newOpenAmount = currentlyOpenShells + 1;
            string errorText = string.Format("ERROR : Maximum Shell Limit ({0}) Reached", InventoryManager._shellLimit);

            if(newOpenAmount > shellLimit)
            {
                executeEvent.Result = ExecutionResult.Error;
                executeEvent.Cancelled = true;
                os.terminal.writeLine(errorText);
                return;
            }
        }
    }
}
