using Hacknet;
using Pathfinder.Util;

namespace HacknetArchipelago.Managers
{
    public static class PointClickerManager
    {
        private static PointClickerDaemon _ptcDaemon;

        private static float _storedPoints = 0;
        private static bool _resetPoints = false;

        public static int RateMultiplier
        {
            get { return _rateMultiplier; }
        }
        private static int _rateMultiplier = 1;

        public static void RefreshPointClickerDaemon()
        {
            Computer ptcComp = ComputerLookup.FindById("pointclicker");
            _ptcDaemon = (PointClickerDaemon)ptcComp.getDaemon(typeof(PointClickerDaemon));
        }

        public static void UseStoredValues()
        {
            if (_ptcDaemon.activeState == null) return;

            _ptcDaemon.activeState.points += (long)_storedPoints;
            if (_resetPoints) _ptcDaemon.activeState.points = 0;

            _storedPoints = 0;
            _resetPoints = false;
        }

        internal static void ChangePointClickerPoints(int amount)
        {
            bool reset = amount < 0;

            if (reset)
            {
                if (_ptcDaemon.activeState == null) {
                    _resetPoints = true;
                } else
                {
                    _ptcDaemon.activeState.points = 0;
                }
            }
            else
            {
                if(_ptcDaemon.activeState == null)
                {
                    _storedPoints += amount;
                } else
                {
                    _ptcDaemon.activeState.points += amount;
                }
            }
        }

        internal static void ChangePointClickerRate(int amount)
        {
            _ptcDaemon.currentRate += amount;
        }

        internal static void ChangeRateMultiplier(int amount)
        {
            _rateMultiplier *= amount;
        }

        internal static void HandlePointClickerUpgrade(string itemName)
        {
            bool adds = itemName.Contains("+");
            bool mult = itemName.Contains("*");

            if(adds)
            {
                bool passive = itemName.EndsWith("s");
                string half = itemName.Split('+')[1];
                string valueString = half.Split('p')[0];

                if(passive)
                {
                    ChangePointClickerRate(int.Parse(valueString));
                } else
                {
                    ChangePointClickerPoints(int.Parse(valueString));
                }
            } else if(mult)
            {
                string valueString = itemName.Split('*')[1];
                ChangeRateMultiplier(int.Parse(valueString));
            }
        }
    }
}
