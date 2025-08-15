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
            if (ArchipelagoManager.Session == null) return;
            DLService = ArchipelagoManager.Session.CreateDeathLinkService();
            DLService.OnDeathLinkReceived += HandleDeathLink;
            DLService.EnableDeathLink();
        }

        internal static void HandleDeathLink(DeathLink deathLink)
        {
            _crashCausedByDeathLink = true;
            OS os = OS.currentInstance;
            string cause = deathLink.Cause;
            cause ??= $"{deathLink.Source} sent out a deathlink!";
            _lastDeathLinkCause = cause;
            os.thisComputer.log($"RECEIVED_DEATHLINK_FROM_{deathLink.Source}");
            os.thisComputer.disabled = true;
            os.thisComputer.bootTimer = Computer.BASE_BOOT_TIME;
            os.thisComputerCrashed();
        }

        public static void SendDeathLink(DeathLink deathLink)
        {
            DLService.SendDeathLink(deathLink);
        }
    }
}
