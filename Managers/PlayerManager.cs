using BepInEx.Logging;
using Hacknet;
using Pathfinder.Event.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetArchipelago.Managers
{
    public static class PlayerManager
    {
        public static OS Os => OS.currentInstance;

        private static ManualLogSource Logger => HacknetAPCore.Logger;

        internal static void AddItemFileToPlayerComputer(string filename, string data)
        {
            bool isExe = filename.EndsWith(".exe");
            string folder = isExe ? "bin" : "home";

            Computer playerComp = OS.currentInstance.thisComputer;
            FileEntry file = new(data, filename);
            Folder fileFolder = playerComp.getFolderFromPath(folder);

            if (fileFolder == null)
            {
                Logger.LogError($"Couldn't add {filename} to player computer: folder {folder} doesn't exist!");
                return;
            }

            bool fileExists = fileFolder.containsFileWithData(data);
            if (fileExists)
            {
                Logger.LogInfo($"Couldn't add {filename} to player computer: a file with that filedata already exists.");
                return;
            }

            fileFolder.files.Add(file);
        }

        internal static void ClearPlayerBinaries()
        {
            Computer playerComp = OS.currentInstance.thisComputer;
            Folder binFolder = playerComp.getFolderFromPath("bin");

            binFolder.files.Clear();
        }

        public static void FlashFakeConnection()
        {
            Os.IncConnectionOverlay.Activate();
        }

        public static void ActivateETAS()
        {
            Os.TraceDangerSequence.BeginTraceDangerSequence();
        }

        private static string _forkbombSource = "NONE";

        public static void ForkbombPlayer(string source, int delay = 3)
        {
            _forkbombSource = source;

            OS.currentInstance.delayer.Post(ActionDelayer.Wait(delay), ActuallyForkbombPlayer);

            HacknetAPCore.SpeakAsSystem("!!! WARNING : FORKBOMB RECEIVED !!!", true);
            HacknetAPCore.SpeakAsSystem($"!!! ACTIVE IN {delay} SECONDS !!!");
        }

        private static void ActuallyForkbombPlayer()
        {
            Os.thisComputer.log($"RECEIVED_FORKBOMB_FROM_{_forkbombSource}");
            Os.warningFlash();
            Os.beepSound.Play();
            Multiplayer.parseInputMessage($"eForkBomb {Os.thisComputer.ip}", Os);
        }
    }
}
