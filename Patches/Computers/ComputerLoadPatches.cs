using System.Linq;

using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Loading;

using Hacknet;

namespace HacknetArchipelago.Patches
{
    public static class ComputerLoadPatches
    {
        public static void PreventArchipelagoExes(TextReplaceEvent textReplaceEvent)
        {
            var replacement = textReplaceEvent.Replacement;
            var itemName = ArchipelagoItems.ArchipelagoDataToItemName(replacement);
            if (itemName == null) return;

            if(!HacknetAPCore._localInventory.ContainsKey(itemName))
            {
                var itemID = ArchipelagoItems.ArchipelagoDataToItem(replacement);
                textReplaceEvent.Replacement = $"ArchipelagoItemID:{itemID}|{itemName}\n" +
                    "You haven't unlocked this item yet!\n\n" +
                    $"{ComputerLoader.filter("#BINARYSMALL#")}";
            }
        }

        public static void WarnWhenDownloadingArchipelagoExes(CommandExecuteEvent cmdExeEvent)
        {
            string fileName = cmdExeEvent.Args.FirstOrDefault(arg => arg.EndsWith(".exe"));
            if (fileName == default) return;

            string file = fileName.Split('.')[0];
            if(!HacknetAPCore._localInventory.ContainsKey(file))
            {
                HacknetAPCore.SpeakAsSystem($"The executable file {file} isn't in your Archipelago inventory!\n" +
                    "If this item was shuffled into the item pool, then it won't launch until you've received it via Archipelago.\n" +
                    "If this item was not shuffled, or you already have it, then you can safely ignore this warning.");
            };
        }
    }
}
