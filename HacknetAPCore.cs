using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Hacknet;

using BepInEx;
using BepInEx.Hacknet;
using BepInEx.Logging;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Pathfinder.Event;
using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Loading;
using Pathfinder.Event.BepInEx;
using Pathfinder.Command;

using HacknetArchipelago.Patches;
using HacknetArchipelago.Commands;
using Pathfinder.Event.Saving;
using Pathfinder.Util;

using HacknetArchipelago.Managers;
using HacknetArchipelago.Daemons;
using Pathfinder.Daemon;
using Microsoft.Xna.Framework.Content;

namespace HacknetArchipelago
{
    public enum FactionAccess : int
    {
        NoAccess = 0,
        Entropy = 1,
        LabyrinthsOrCSEC = 2,
        CSEC = 3,
        Disabled = -1
    }

    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency("com.Pathfinder.API", BepInDependency.DependencyFlags.HardDependency)]
    public class HacknetAPCore : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.archipelago";
        public const string ModName = "Hacknet Archipelago Client";
        public const string ModVer = "0.5.0";

        public const string GameString = "Hacknet";

        public static readonly List<string> IntroTextFinishers =
        [
            "...then I'm already dead.",
            "...then I'm already BK'd.",
            "...then I need peace and tranquility.",
            "...then it's that damned hedgehog.",
            "...then I need more strawberries.",
            "...omae wa mou shindeiru.",
            "...ooh, banana.",
            "...then I'm still waiting for Silksong.",
            "...then it's dangerous to go alone.",
            "...you cannot sleep now, there are monsters nearby.",
            "...then I wanna be the very best, like no-one ever was.",
            "...then kids like you, should be burning in hell.",
            "...then I need a Puzzle Skip.",
            "...bowties are cool.",
            "...I'm with you in the dark."
        ];

        public static ArchipelagoSession ArchipelagoSession => ArchipelagoManager.Session;
        public static DeathLinkService DeathLinkService => DeathLinkManager.DLService;
        public static HacknetAPSlotData SlotData => ArchipelagoManager.SlotData;

        public static ManualLogSource Logger = new(ModName);
        public static ContentManager ContentManager => Game1.singleton.Content;

        public static bool SkipBootIntroText = false;
        public static bool BeepOnItemReceived = true;
        public static Tuple<string, string, string> CachedConnectionDetails = new(null, null, null);

        internal static string _originalBsodText = "";

        public const string DISABLE_CLIENT_ARGUMENT = "-disableap";

        public override bool Load()
        {
            var launchArguments = Environment.GetCommandLineArgs();

            if(launchArguments.Contains(DISABLE_CLIENT_ARGUMENT))
            {
                Log.LogWarning(string.Format("Hacknet was started with \"{0}\", so the Archipelago " +
                    "client will be disabled.", DISABLE_CLIENT_ARGUMENT));
                return true;
            }

            BepInEx.Logging.Logger.Sources.Add(Logger);

            HarmonyInstance.PatchAll(typeof(HacknetAPCore).Assembly);

            Settings.AllowExtensionMode = false;

            CommandManager.RegisterCommand("printitems", ArchipelagoUserCommands.ViewPlayerInventory);
            CommandManager.RegisterCommand("printprog", ArchipelagoUserCommands.ViewProgressiveItems);
            CommandManager.RegisterCommand("archistatus", ArchipelagoUserCommands.GetArchipelagoStatus);
            CommandManager.RegisterCommand("archirestock", ArchipelagoUserCommands.ForceRestockExecutables);
            CommandManager.RegisterCommand("rearchi", ArchipelagoUserCommands.ReconnectToArchipelago);
            CommandManager.RegisterCommand("uncachechecks", ArchipelagoUserCommands.ForceSendCachedLocations);
            CommandManager.RegisterCommand("archisay", ArchipelagoUserCommands.SayCommand);
            CommandManager.RegisterCommand("remaining", ArchipelagoUserCommands.PrintRemainingItems);

            CommandManager.RegisterCommand("archifh", ItemCommands.UseForceHack);
            CommandManager.RegisterCommand("skipmission", ItemCommands.UseMissionSkip);

            CommandManager.RegisterCommand("testdeathlink", ArchipelagoUserCommands.TestCrashDeathLink, false, true);
            CommandManager.RegisterCommand("pslotdata", ArchipelagoDebugCommands.PrintSlotData, false, true);
            CommandManager.RegisterCommand("debugsay", ArchipelagoDebugCommands.TestSayCommand, false, true);
            CommandManager.RegisterCommand("debughint", ArchipelagoDebugCommands.TestHintCommand, false, true);
            CommandManager.RegisterCommand("debugpeek", ArchipelagoDebugCommands.TestPeekLocation, false, true);
            CommandManager.RegisterCommand("setfactionaccess", ArchipelagoDebugCommands.DebugSetFactionAccess, false, true);
            CommandManager.RegisterCommand("printserverdata", ArchipelagoDebugCommands.DebugPrintStorage, false, true);
            CommandManager.RegisterCommand("addtoptcrate", ArchipelagoDebugCommands.AddToConstantRate, false, true);
            CommandManager.RegisterCommand("addtoptcpassive", ArchipelagoDebugCommands.AddToPassiveRate, false, true);
            CommandManager.RegisterCommand("addarchidebugentries", ArchipelagoDebugCommands.AddTestEntriesToIRC, false, true);
            CommandManager.RegisterCommand("hasexec", ArchipelagoDebugCommands.CheckIfPlayerHasExecutable, false, true);
            CommandManager.RegisterCommand("addtolocalinventoryonlyuseifyouknowhwatyouredoing",
                ArchipelagoDebugCommands.AddToLocalInventory, false, true);

            EventManager<TextReplaceEvent>.AddHandler(ComputerLoadPatches.PreventArchipelagoExes);
            EventManager<CommandExecuteEvent>.AddHandler(ComputerLoadPatches.WarnWhenDownloadingArchipelagoExes);
            EventManager<OSLoadedEvent>.AddHandler(InventoryManager.CheckItemsCacheOnLoad);
            EventManager<OSLoadedEvent>.AddHandler(SetupArchipelagoIRC);
            EventManager<OSUpdateEvent>.AddHandler(CheckForFlagsPatch.CheckFlagsForArchiLocations);
            EventManager<OSUpdateEvent>.AddHandler(ArchipelagoManager.AssureArchiConnection);
            EventManager<UnloadEvent>.AddHandler(ArchipelagoManager.UpdateServerDataOnClose);
            EventManager<SaveEvent>.AddHandler(SaveLoadExecutors.ArchipelagoDataSaver.InjectArchipelagoSaveData);

            EventManager<CommandExecuteEvent>.AddHandler(CommandPatches.PreventDownloadingUncollectedExecutables);
            EventManager<CommandExecuteEvent>.AddHandler(CommandPatches.PreventModifyingPTCSaveData);

            EventManager<CommandExecuteEvent>.AddHandler(ShellLimitPatch.LimitShells);
            EventManager<OSUpdateEvent>.AddHandler(RAMLimitPatch.LimitRAM);

            DaemonManager.RegisterDaemon<ArchipelagoIRCDaemon>();

            return true;
        }

        public const string ARCHI_IRC_ID = "archiIRC";

        public static void SetupArchipelagoIRC(OSLoadedEvent oSLoadedEvent)
        {
            OS os = oSLoadedEvent.Os;

            bool existsAlready = os.netMap.nodes.Any(c => c.idName == ARCHI_IRC_ID);
            if (existsAlready) return;

            Computer archiIRCComp = new("Archipelago IRC", "archipelago.gg", new(0.5f, 0.5f), 0, 0, os);
            archiIRCComp.idName = ARCHI_IRC_ID;
            ArchipelagoIRCDaemon archipelagoIRC = new(archiIRCComp, "", os);
            archiIRCComp.daemons.Add(archipelagoIRC);
            archiIRCComp.initDaemons();

            os.netMap.nodes.Add(archiIRCComp);
            os.netMap.discoverNode(archiIRCComp);
        }

        public const string SYSTEM_PREFIX = "(HACKNET_ARCHIPELAGO) ";

        internal static void SpeakAsSystem(string message, bool needsAttention = false)
        {
            OS os = OS.currentInstance;

            if(needsAttention)
            {
                os.beepSound.Play();
                os.warningFlash();
            }

            os.terminal.writeLine(SYSTEM_PREFIX + message);
        }
    }

    public class HacknetAPSlotData
    {
        public enum ExecutableShuffleMode : long
        {
            ShuffleAll = 1,
            ProgAndUseful = 2,
            ProgressionOnly = 3,
            Disabled = 4
        }

        public enum ExecutableGroupingMode : long
        {
            Individually = 1,
            Regional = 2,
            Practicality = 3
        }

        public enum LimitsMode : long
        {
            EnableAllLimits = 1,
            OnlyShells = 2,
            OnlyShellsZero = 3,
            OnlyRAM = 4,
            Disabled = 5
        }

        public enum VictoryCondition : long
        {
            Heartstopper = 1,
            AltitudeLoss = 2,
            VIP = 3,
            Veteran = 4,
            Completionist = 5
        }

        public VictoryCondition PlayerGoal = VictoryCondition.Heartstopper;
        public string PointClickerMode = "vanilla";
        public ExecutableShuffleMode ExecutableShuffle = ExecutableShuffleMode.ShuffleAll;
        public ExecutableGroupingMode ExecutableGrouping = ExecutableGroupingMode.Individually;
        public LimitsMode LimitsShuffle = LimitsMode.Disabled;
        public bool SprintReplacesBounce = true;
        public bool DeathLink = false;
        public uint RandomizationSeed = 0;
        public bool ShuffleLabyrinths = true;
        public bool EnableFactionAccess = false;
        public bool ShuffleAchievements = false;
        public bool ShuffleAdminAccess = false;

        internal Dictionary<string, object> rawSlotData = new();

        public HacknetAPSlotData(Dictionary<string, object> rawSlotData)
        {
            this.rawSlotData = rawSlotData;
            foreach(var key in rawSlotData.Keys)
            {
                HacknetAPCore.Logger.LogDebug($"Received Slot Data -- {key} : {rawSlotData[key]}");
            }
            PlayerGoal = (VictoryCondition)rawSlotData["player_goal"];
            PointClickerMode = (string)rawSlotData["pointclicker_mode"];
            ExecutableShuffle = (ExecutableShuffleMode)rawSlotData["executable_shuffle"];
            ExecutableGrouping = (ExecutableGroupingMode)rawSlotData["executable_grouping"];
            LimitsShuffle = (LimitsMode)rawSlotData["limits_mode"];
            SprintReplacesBounce = (bool)rawSlotData["sprint_replaces_bounce"];
            DeathLink = (bool)rawSlotData["deathlink"];
            ShuffleLabyrinths = (bool)rawSlotData["enable_labyrinths"]; // enable_labyrinths
            EnableFactionAccess = (bool)rawSlotData["enable_faction_access"];
        }

        public string GetRawSlotData()
        {
            StringBuilder resultBuilder = new();
            foreach(var key in rawSlotData.Keys)
            {
                resultBuilder.Append($"{key} : {rawSlotData[key]}");
                resultBuilder.AppendLine();
            }
            return resultBuilder.ToString();
        }
    }

    public class HacknetArchipelagoUserData
    {
        public int StoredFactionAccess = (int)FactionAccess.Disabled;
        public int StoredShellLimit = -1;
        public int StoredRAMLimit = -1;
        public int RemainingMissionSkips = 0;
        public int RemainingForceHacks = 0;
        public List<string> CheckedLocations = [];
    }
}
