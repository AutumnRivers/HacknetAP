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
                        default:
                            break;
                    }
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
                HacknetAPCore._cachedChecks = cachedLocations;
            }

            private void LoadCollectedFlags(string flags)
            {
                List<string> collectedFlags = [.. flags.Split(',')];
                HacknetAPCore._collectedFlags = collectedFlags;
            }

            private void LoadStoredLocalInventory(List<ElementInfo> localItemElems)
            {
                List<string> localInventory = [];
                foreach(var localItem in localItemElems)
                {
                    if (localItem.Name != "LocalItem" ||
                        !localItem.Attributes.ContainsKey("ItemName")) continue;

                    localInventory.Add(localItem.Attributes["ItemName"]);
                    HacknetAPCore._localInventory.Add(localItem.Attributes["ItemName"], 1);
                }
            }
        }

        public class ArchipelagoDataSaver
        {
            public static void InjectArchipelagoSaveData(SaveEvent saveEvent)
            {
                XElement archiElement = new(BASE_ELEMENT_NAME);

                if (HacknetAPCore._cachedChecks.Count > 0)
                {
                    XElement cachedLocsElem = new("CachedLocations");

                    foreach (var loc in HacknetAPCore._cachedChecks)
                    {
                        XElement cachedElem = new("CachedLoc");
                        XAttribute cachedAttr = new("ArchipelagoName", loc);
                        cachedElem.Add(cachedAttr);
                        cachedLocsElem.Add(cachedElem);
                    }

                    archiElement.Add(cachedLocsElem);
                }

                if(HacknetAPCore._collectedFlags.Count > 0)
                {
                    XElement collectedFlagsElem = new("CollectedFlags");
                    XAttribute flagsAttr = new("flags", "");
                    StringBuilder flagBuilder = new();

                    foreach(var flag in HacknetAPCore._collectedFlags)
                    {
                        flagBuilder.Append(flag);
                        if (HacknetAPCore._collectedFlags.Last() != flag) flagBuilder.Append(",");
                    }
                    flagsAttr.SetValue(flagBuilder.ToString());
                    collectedFlagsElem.Add(flagsAttr);

                    archiElement.Add(collectedFlagsElem);
                }

                if(HacknetAPCore._localInventory.Count > 0)
                {
                    XElement localInvElem = new("LocalInventory");

                    foreach(var item in HacknetAPCore._localInventory)
                    {
                        XElement invElem = new("LocalItem");
                        XAttribute nameAttr = new("ItemName", item.Key);
                        invElem.Add(nameAttr);
                        localInvElem.Add(invElem);
                    }

                    archiElement.Add(localInvElem);
                }

                if(HacknetAPCore.SlotData.EnableFactionAccess)
                {
                    XElement facAccessElem = new("FactionAccess");
                    XAttribute facAccessAttr = new("Value", HacknetAPCore._factionAccess);
                    facAccessElem.Add(facAccessAttr);
                    archiElement.Add(facAccessElem);
                }

                saveEvent.Save.FirstNode.AddAfterSelf(archiElement);
            }
        }
    }
}
