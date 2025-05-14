using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Hacknet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetArchipelago.Managers
{
    public static class DeathLinkManager
    {
        public static DeathLinkService DLService;

        internal static bool _crashCausedByDeathLink = false;
        internal static string _lastDeathLinkCause = "";

        public static void SetupDeathLink()
        {
            DLService = ArchipelagoManager.Session.CreateDeathLinkService();
            DLService.OnDeathLinkReceived += HandleDeathLink;
            DLService.EnableDeathLink();
        }

        internal static void HandleDeathLink(DeathLink deathLink)
        {
            _crashCausedByDeathLink = true;
            string cause = deathLink.Cause;
            cause ??= $"{deathLink.Source} sent out a deathlink!";
            _lastDeathLinkCause = cause;
            OS.currentInstance.thisComputer.log($"RECEIVED_DEATHLINK_FROM_{deathLink.Source}");
            OS.currentInstance.thisComputer.crash(deathLink.Source);
        }
    }
}
