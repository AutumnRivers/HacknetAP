using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Util;

namespace HacknetArchipelago.Managers
{
    public static class PointClickerManager
    {
        public static bool BlockUpgrades
        {
            get
            {
                if (ArchipelagoManager.Session == null) return false;
                return ArchipelagoManager.SlotData.PointClickerMode == "block_upgrade_effects";
            }
        }

        public static bool PointClickerShuffled { get; private set; } = true;

        private static PointClickerDaemon _ptcDaemon;

        private static float _storedPoints = 0;
        private static bool _resetPoints = false;

        public static int RateMultiplier
        {
            get { return _rateMultiplier; }
        }
        private static int _rateMultiplier = 1;

        public static int PassivePoints
        {
            get { return _passivePoints; }
        }
        private static int _passivePoints = 0;

        private static Dictionary<string, int> StaticPointClickerItems = new()
        {
            { "PointClicker +50pt.", 50 },
            { "PointClicker +500pt.", 500 },
            { "PointClicker +5000pt.", 5000 }
        };

        private static Dictionary<string, int> PassiveRateItems = new()
        {
            { "PointClicker +100pt./s", 100 },
            { "PointClicker +1000pt./s", 1000 }
        };

        private static Dictionary<string, int> RateMultItems = new()
        {
            { "PointClicker Passive*10", 10 },
            { "PointClicker Passive*100", 100 },
            { "PointClicker Passive*1000", 1000 }
        };

        public static void CheckIfPointClickerShuffled()
        {
            const string testLoc = "PointClicker -- Click Me!";
            var testLocId = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(
                HacknetAPCore.GameString,
                testLoc);
            PointClickerShuffled = testLocId != -1;
        }

        public static void RefreshPointClickerValues()
        {
            if(!PointClickerShuffled) return;
            
            var ptcItems = InventoryManager.CachedItemsReceived
                .Where(i => i.ItemDisplayName.Contains("PointClicker"));

            var staticItems = ptcItems.Where(i => i.ItemDisplayName.Contains("+") &&
                                                  !i.ItemDisplayName.EndsWith("/s")).ToList();
            var rateUps = ptcItems.Where(i => i.ItemDisplayName.Contains("+") &&
                                              i.ItemDisplayName.EndsWith("/s")).ToList();
            var rateMultUps = ptcItems.Where(i => i.ItemDisplayName.Contains("Passive")).ToList();

            var staticPoints = 0;
            foreach (var staticItem in staticItems)
            {
                if(!StaticPointClickerItems.ContainsKey(staticItem.ItemDisplayName)) continue;

                staticPoints += StaticPointClickerItems[staticItem.ItemDisplayName];
            }
            _storedPoints = staticPoints;

            var rate = 0;
            foreach (var rateUp in rateUps)
            {
                if(!PassiveRateItems.ContainsKey(rateUp.ItemDisplayName)) continue;

                rate += PassiveRateItems[rateUp.ItemDisplayName];
            }
            _passivePoints = rate;

            var rateMult = 1;
            foreach (var rateMultUp in rateMultUps)
            {
                if(!RateMultItems.ContainsKey(rateMultUp.ItemDisplayName)) continue;

                rateMult += RateMultItems[rateMultUp.ItemDisplayName];
            }
            _rateMultiplier = rateMult;
        }

        public static void RefreshPointClickerDaemon()
        {
            var ptcComp = ComputerLookup.FindById("pointclicker");
            _ptcDaemon = (PointClickerDaemon)ptcComp.getDaemon(typeof(PointClickerDaemon));
        }

        public static void UseStoredValues()
        {
            if (_ptcDaemon.activeState == null) return;

            _ptcDaemon.activeState.points = (long)_storedPoints;
            _ptcDaemon.currentRate = _passivePoints;
            if (_resetPoints) _ptcDaemon.activeState.points = 0;

            _storedPoints = 0;
            _resetPoints = false;
        }
    }
}
