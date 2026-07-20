using System.Linq;

using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Loading;

using Hacknet;
using System.Collections.Generic;
using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches
{
    public static class ComputerLoadPatches
    {
        public static void PreventArchipelagoExes(TextReplaceEvent textReplaceEvent)
        {
            var replacement = textReplaceEvent.Replacement;
            var itemName = ArchipelagoItems.ArchipelagoDataToItemName(replacement);
            if (itemName == null) return;

            if(!InventoryManager._localInventory.ContainsKey(itemName))
            {
                var itemID = ArchipelagoItems.ArchipelagoDataToItem(replacement);
                textReplaceEvent.Replacement = $"ArchipelagoItemID:{itemID}|{itemName}\n" +
                    "You haven't unlocked this item yet!\n\n" +
                    $"{ComputerLoader.filter("#BINARYSMALL#")}";
            }
        }
    }
}
