using Hacknet;
using Pathfinder.Util;

namespace HacknetArchipelago.Managers
{
    public static class PointClickerManager
    {
        private static PointClickerDaemon _ptcDaemon;

        public static void RefreshPointClickerDaemon()
        {
            Computer ptcComp = ComputerLookup.FindById("pointclicker");
            _ptcDaemon = (PointClickerDaemon)ptcComp.getDaemon(typeof(PointClickerDaemon));
        }

        internal static void ChangePointClickerPoints(int amount)
        {
            bool reset = amount < 0;

            if (reset)
            {
                _ptcDaemon.activeState.points = 0;
            }
            else
            {
                _ptcDaemon.activeState.points += amount;
            }
        }

        internal static void ChangePointClickerRate(int amount)
        {
            _ptcDaemon.currentRate += amount;
        }
    }
}
