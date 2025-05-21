using HacknetArchipelago.Managers;
using Pathfinder.Event.Saving;
using Pathfinder.Meta.Load;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HacknetArchipelago
{
    public class SaveLoadExecutors
    {
        public const string BASE_ELEMENT_NAME = "HacknetArchipelagoSave";

        [SaveExecutor("HacknetSave." + BASE_ELEMENT_NAME)]
        public class ArchipelagoSaveReader : SaveLoader.SaveExecutor
        {
            public override void Execute(EventExecutor exec, ElementInfo info)
            {
                if (info.Children.Count == 0) return;

                foreach(var child in info.Children)
                {
                    switch(child.Name)
                    {
                        case "CachedLocations":
                            LoadCachedLocations(child.Children);
                            break;
                        case "CollectedFlags":
                            LoadCollectedFlags(child.Attributes["flags"]);
                            break;
                        case "LocalInventory":
                            LoadStoredLocalInventory(child.Children);
                            break;
                        case "AllCollectedItems":
                            LoadStoredCollectedItemData(child.Children);
                            break;
                        default:
                            break;
                    }
                }
            }

            private void LoadStoredCollectedItemData(List<ElementInfo> collectedItemsElems)
            {
                string nameAttr = "ItemName";
                string playersChild = "AssociatedPlayer";
                string playerValue = "Value";

                foreach(var item in collectedItemsElems)
                {
                    List<string> players = [];
                    string name = item.Attributes[nameAttr];
                    foreach(var child in item.Children)
                    {
                        if (child.Name != playersChild) continue;
                        players.Add(child.Attributes[playerValue]);
                    }
                    InventoryManager.AddNewItem(name, players);
                }
            }

            private void LoadCachedLocations(List<ElementInfo> cachedLocationElements)
            {
                List<string> cachedLocations = [];
                foreach(var cachedLoc in cachedLocationElements)
                {
                    if (cachedLoc.Name != "CachedLoc" ||
                        !cachedLoc.Attributes.ContainsKey("ArchipelagoName")) continue;

                    cachedLocations.Add(cachedLoc.Attributes["ArchipelagoName"]);
                }
                LocationManager._cachedChecks = cachedLocations;
            }

            private void LoadCollectedFlags(string flags)
            {
                List<string> collectedFlags = [.. flags.Split(',')];
                LocationManager._collectedFlags = collectedFlags;
            }

            private void LoadStoredLocalInventory(List<ElementInfo> localItemElems)
            {
                List<string> localInventory = [];
                foreach(var localItem in localItemElems)
                {
                    if (localItem.Name != "LocalItem" ||
                        !localItem.Attributes.ContainsKey("ItemName")) continue;

                    localInventory.Add(localItem.Attributes["ItemName"]);
                    InventoryManager._localInventory.Add(
                        localItem.Attributes["ItemName"],
                        localItem.Attributes["AssociatedPlayer"]);
                }
            }
        }

        public class ArchipelagoDataSaver
        {
            public static void InjectArchipelagoSaveData(SaveEvent saveEvent)
            {
                XElement archiElement = new(BASE_ELEMENT_NAME);

                XElement slotElem = new("AssociatedSlot");
                XAttribute slotName = new("SlotName", ArchipelagoManager.PlayerName);
                XAttribute slotURI = new("URI", ArchipelagoManager.Session.Socket.Uri);
                slotElem.Add([slotName, slotURI]);
                archiElement.Add(slotElem);

                if (LocationManager._cachedChecks.Count > 0)
                {
                    XElement cachedLocsElem = new("CachedLocations");

                    foreach (var loc in LocationManager._cachedChecks)
                    {
                        XElement cachedElem = new("CachedLoc");
                        XAttribute cachedAttr = new("ArchipelagoName", loc);
                        cachedElem.Add(cachedAttr);
                        cachedLocsElem.Add(cachedElem);
                    }

                    archiElement.Add(cachedLocsElem);
                }

                if(LocationManager._collectedFlags.Count > 0)
                {
                    XElement collectedFlagsElem = new("CollectedFlags");
                    XAttribute flagsAttr = new("flags", "");
                    StringBuilder flagBuilder = new();

                    foreach(var flag in LocationManager._collectedFlags)
                    {
                        flagBuilder.Append(flag);
                        if (LocationManager._collectedFlags.Last() != flag) flagBuilder.Append(",");
                    }
                    flagsAttr.SetValue(flagBuilder.ToString());
                    collectedFlagsElem.Add(flagsAttr);

                    archiElement.Add(collectedFlagsElem);
                }

                if(InventoryManager._localInventory.Count > 0)
                {
                    XElement localInvElem = new("LocalInventory");

                    foreach(var item in InventoryManager._localInventory)
                    {
                        XElement invElem = new("LocalItem");
                        XAttribute nameAttr = new("ItemName", item.Key);
                        invElem.Add(nameAttr);
                        localInvElem.Add(invElem);
                    }

                    archiElement.Add(localInvElem);
                }

                if(InventoryManager.allCollectedItems.Count > 0)
                {
                    XElement collectedItemsElem = new("AllCollectedItems");

                    foreach(var item in InventoryManager.allCollectedItems)
                    {
                        XElement itemElem = new("CollectedItem");
                        XElement nameAttr = new("ItemName", item.Key);
                        itemElem.Add(nameAttr);

                        foreach(var player in item.Value)
                        {
                            XElement playerElem = new("AssociatedPlayer");
                            XAttribute playerName = new("Value", player);
                            playerElem.Add(playerName);
                            itemElem.Add(playerElem);
                        }

                        collectedItemsElem.Add(itemElem);
                    }

                    archiElement.Add(collectedItemsElem);
                }

                if(HacknetAPCore.SlotData.EnableFactionAccess)
                {
                    XElement facAccessElem = new("FactionAccess");
                    XAttribute facAccessAttr = new("Value", InventoryManager._factionAccess);
                    facAccessElem.Add(facAccessAttr);
                    archiElement.Add(facAccessElem);
                }

                XElement ptcElem = new("PointClickerSaveData");
                XAttribute ptcRate = new("RateMult", PointClickerManager.RateMultiplier);
                ptcElem.Add(ptcRate);
                archiElement.Add(ptcElem);

                saveEvent.Save.FirstNode.AddAfterSelf(archiElement);
            }
        }
    }
}
