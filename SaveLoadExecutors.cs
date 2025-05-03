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

                saveEvent.Save.FirstNode.AddAfterSelf(archiElement);
            }
        }
    }
}
