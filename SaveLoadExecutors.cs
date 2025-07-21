using HacknetArchipelago.Managers;

using Pathfinder.Event.Saving;
using Pathfinder.Meta.Load;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HacknetArchipelago
{
    public class SaveLoadExecutors
    {
        [SaveExecutor("HacknetSave.HacknetArchipelagoSave", ParseOption.ParseInterior)]
        public class ArchipelagoSaveReader : SaveLoader.SaveExecutor
        {
            public override void Execute(EventExecutor exec, ElementInfo info)
            {
                if (!info.Children.Any()) return;

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
                        case "PointClickerSaveData":
                            LoadPointClickerSaveData(child);
                            break;
                        default:
                            break;
                    }
                }
            }

            private void LoadStoredCollectedItemData(List<ElementInfo> collectedItemsElems)
            {
                string nameElem = "ItemName";
                string playersChild = "AssociatedPlayer";
                string playerValue = "Value";

                foreach(var item in collectedItemsElems)
                {
                    Console.WriteLine(item.Name);
                    List<string> players = [];
                    string name = "Item";
                    foreach(var child in item.Children)
                    {
                        if(child.Name == nameElem)
                        {
                            name = child.Content;
                        } else if(child.Name == playersChild)
                        {
                            players.Add(child.Attributes[playerValue]);
                        } else { continue; }
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
                    
                    if(Hacknet.OS.DEBUG_COMMANDS)
                    {
                        HacknetAPCore.Logger.LogDebug($"Adding {localItem.Attributes["ItemName"]} to local inventory...");
                    }

                    localInventory.Add(localItem.Attributes["ItemName"]);
                    if (InventoryManager._localInventory.ContainsKey(localItem.Attributes["ItemName"])) continue;
                    InventoryManager._localInventory.Add(localItem.Attributes["ItemName"], null);
                }
            }

            private void LoadPointClickerSaveData(ElementInfo ptcElem)
            {
                string rateAttr = "RateMult";
                string ptsAttr = "PassivePts";

                int mult = 1;
                int pts = 0;

                if (int.TryParse(ptcElem.Attributes[rateAttr], out int storedMult)) { mult = storedMult; }
                if (int.TryParse(ptcElem.Attributes[ptsAttr], out int storedPts)) { pts = storedPts; }

                PointClickerManager.ChangePointClickerPassiveRate(pts);
                PointClickerManager.ChangeRateMultiplier(mult);
            }
        }

        public class ArchipelagoDataSaver
        {
            public static void InjectArchipelagoSaveData(SaveEvent saveEvent)
            {
                XElement archiElement = new("HacknetArchipelagoSave");

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
                int rateMult = PointClickerManager.RateMultiplier;
                if (rateMult <= 0) rateMult = 1;
                XAttribute ptcRate = new("RateMult", rateMult);
                XAttribute ptcPassive = new("PassivePts", PointClickerManager.PassivePoints);
                ptcElem.Add([ptcRate, ptcPassive]);
                archiElement.Add(ptcElem);

                XElement archiDataElem = new("ArchipelagoConnection");
                XAttribute archiSlotElem = new("SlotName", ArchipelagoManager.PlayerName);
                XAttribute archiURI = new("URI", ArchipelagoManager.Session.Socket.Uri.OriginalString);
                XAttribute archiUUID = new("UUID", ArchipelagoManager.Session.ConnectionInfo.Uuid);
                archiDataElem.Add([archiSlotElem, archiUUID, archiURI]);

                saveEvent.Save.FirstNode.AddBeforeSelf(archiElement);
                archiElement.AddAfterSelf(archiDataElem);
            }
        }
    }
}
