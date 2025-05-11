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
        public const string ModVer = "0.6.2";

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
            "...bowties are cool."
        ];

        public static ArchipelagoSession ArchipelagoSession;
        public static DeathLinkService DeathLinkService;
        public static HacknetAPSlotData SlotData;

        public static ManualLogSource Logger = new(ModName);

        public static bool IsNewRun = false;
        public static bool IsConnected = false;
        public static bool SkipBootIntroText = false;
        public static Tuple<string, string, string> CachedConnectionDetails = new(null, null, null);

        internal static Dictionary<string, int> _localInventory = [];
        internal static List<string> _collectedFlags = [];
        internal static List<string> _cachedChecks = [];

        internal static int _remainingMissionSkips = 0;
        internal static int _remainingForceHacks = 0;

        internal static int _shellLimit = -1;
        internal static int _ramLimit = 300;
        internal static FactionAccess _factionAccess = FactionAccess.Disabled;

        internal static bool _crashCausedByDeathLink = false;
        internal static string _lastDeathLinkCause = "";

        internal static string _originalBsodText = "";

        public override bool Load()
        {
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

            CommandManager.RegisterCommand("archifh", ItemCommands.UseForceHack);
            CommandManager.RegisterCommand("skipmission", ItemCommands.UseMissionSkip);

            CommandManager.RegisterCommand("testdeathlink", ArchipelagoUserCommands.TestCrashDeathLink, false, true);
            CommandManager.RegisterCommand("pslotdata", ArchipelagoDebugCommands.PrintSlotData, false, true);
            CommandManager.RegisterCommand("debugsay", ArchipelagoDebugCommands.TestSayCommand, false, true);
            CommandManager.RegisterCommand("debughint", ArchipelagoDebugCommands.TestHintCommand, false, true);
            CommandManager.RegisterCommand("debugpeek", ArchipelagoDebugCommands.TestPeekLocation, false, true);
            CommandManager.RegisterCommand("setfactionaccess", ArchipelagoDebugCommands.DebugSetFactionAccess, false, true);
            CommandManager.RegisterCommand("printserverdata", ArchipelagoDebugCommands.DebugPrintStorage, false, true);

            EventManager<TextReplaceEvent>.AddHandler(ComputerLoadPatches.PreventArchipelagoExes);
            EventManager<CommandExecuteEvent>.AddHandler(ComputerLoadPatches.WarnWhenDownloadingArchipelagoExes);
            EventManager<OSLoadedEvent>.AddHandler(CheckItemsCacheOnLoad);
            EventManager<OSUpdateEvent>.AddHandler(CheckForFlagsPatch.CheckFlagsForArchiLocations);
            EventManager<OSUpdateEvent>.AddHandler(AssureArchiConnection);
            EventManager<UnloadEvent>.AddHandler(UpdateServerDataOnClose);
            EventManager<SaveEvent>.AddHandler(SaveLoadExecutors.ArchipelagoDataSaver.InjectArchipelagoSaveData);

            EventManager<ExecutableExecuteEvent>.AddHandler(ShellLimitPatch.LimitShells);
            EventManager<OSUpdateEvent>.AddHandler(RAMLimitPatch.LimitRAM);

            return true;
        }

        private static bool _warnedAboutDisconnect = false;

        private static void AssureArchiConnection(OSUpdateEvent oSUpdateEvent)
        {
            if (oSUpdateEvent.OS == null) return;

            if(!_warnedAboutDisconnect && ArchipelagoSession == null)
            {
                SpeakAsSystem("No Archipelago session is currently active!! Something went VERY wrong!");
                oSUpdateEvent.OS.warningFlash();
                oSUpdateEvent.OS.beepSound.Play();
                IsConnected = false;
                _warnedAboutDisconnect = true;
            } else if(!_warnedAboutDisconnect && !ArchipelagoSession.Socket.Connected)
            {
                SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                oSUpdateEvent.OS.warningFlash();
                oSUpdateEvent.OS.beepSound.Play();
                Logger.LogError("Lost connection to Archipelago!");
                IsConnected = false;
                _warnedAboutDisconnect = true;
            } else
            {
                IsConnected = true;
                _warnedAboutDisconnect = false;
            }
        }

        private static void UpdateServerDataOnClose(UnloadEvent unloadEvent)
        {
            if (ArchipelagoSession == null) return;

            UpdateServerData();
        }

        private static void CheckItemsCacheOnLoad(OSLoadedEvent osLoadedEvent)
        {
            if (osLoadedEvent.Thrown || osLoadedEvent.Cancelled) return;
            Logger.LogDebug("Successful OS load detected. Checking items cache...");
            _originalBsodText = osLoadedEvent.Os.crashModule.bsodText;
            ForceCheckItemsCache();

            if(SlotData.EnableFactionAccess && _factionAccess == FactionAccess.Disabled)
            {
                _factionAccess = FactionAccess.NoAccess;
            }

            if(SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShellsZero && _shellLimit == -1)
            {
                _shellLimit = 0;
            } else if((SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.EnableAllLimits ||
                SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShells) && _shellLimit <= 0)
            {
                _shellLimit = 1;
            }

            GetLocalInventoryFromServerInventory();
        }

        private static void GetLocalInventoryFromServerInventory()
        {
            bool itemsExist = ArchipelagoSession.Items.AllItemsReceived.Count > 0;
            if (!itemsExist) return;
            var executableNames = ArchipelagoItems.ExecutableNames;
            var collectedExecutables = ArchipelagoSession.Items.AllItemsReceived.Where(i => executableNames.Contains(i.ItemDisplayName));

            _localInventory.Clear();
            foreach(var exe in collectedExecutables)
            {
                if (_localInventory.ContainsKey(exe.ItemDisplayName)) continue;
                _localInventory.Add(exe.ItemDisplayName, 1);
            }

            if (OS.currentInstance == null) return;

            var playerBin = OS.currentInstance.thisComputer.getFolderFromPath("bin");
            var exeFiles = playerBin.files.Where(f => f.name.EndsWith(".exe"));

            Dictionary<string, string> exeToPack = new()
            {
                { "Decypher", "DEC Suite" },
                { "MemDumpGenerator", "Mem Suite" }
            };

            foreach(var file in exeFiles)
            {
                var cleanName = file.name.Split('.')[0];
                string exeName = cleanName;
                if (exeToPack.ContainsKey(exeName)) exeName = exeToPack[exeName];

                if (_localInventory.ContainsKey(exeName) || !ArchipelagoItems.ExecutableNames.Contains(exeName)) continue;
                _localInventory.Add(exeName, 1);
            }
        }

        internal static void ForceRestockItems()
        {
            var items = ArchipelagoSession.Items.AllItemsReceived;
            ClearPlayerBinaries();
            foreach(var item in items)
            {
                CollectArchipelagoItem(item, false, true);
            }
        }

        private static void ClearPlayerBinaries()
        {
            Computer playerComp = OS.currentInstance.thisComputer;
            Folder binFolder = playerComp.getFolderFromPath("bin");

            binFolder.files.Clear();
        }

        public const string SYSTEM_PREFIX = "(HACKNET_ARCHIPELAGO) ";

        internal static LoginResult ConnectToArchipelago(string uri, string slotName, string password = "")
        {
            var session = ArchipelagoSessionFactory.CreateSession(uri);
            session.Items.ItemReceived -= ReceiveArchipelagoItem;
            session.Items.ItemReceived += ReceiveArchipelagoItem;
            var result = session.TryConnectAndLogin(GameString,
                        slotName,
                        ItemsHandlingFlags.AllItems,
                        null,
                        ["NoText"],
                        null,
                        password, true);
            if(result.Successful)
            {
                var sessionData = (LoginSuccessful)result;
                SlotData = new HacknetAPSlotData(sessionData.SlotData);
                if(SlotData.DeathLink)
                {
                    DeathLinkService = session.CreateDeathLinkService();
                    DeathLinkService.OnDeathLinkReceived += HandleDeathLink;
                    DeathLinkService.EnableDeathLink();
                }
                ArchipelagoSession = session;
                IsConnected = true;
                Logger.LogInfo("Successfully (re-)connected to Archipelago!");
                RetrieveDataFromServer();
                SendCachedLocations();
            }
            return result;
        }

        internal static void SendCachedLocations()
        {
            if (!_cachedChecks.Any()) return;

            List<long> cachedLocationIDs = new();
            foreach(var location in _cachedChecks)
            {
                var locationID = ArchipelagoSession.Locations.GetLocationIdFromName(GameString, location);
                if (locationID == -1) continue;
                cachedLocationIDs.Add(locationID);
            }

            SendArchipelagoLocations([.. cachedLocationIDs]);
        }

        internal static void SendArchipelagoLocations(long locationID)
        {
            ArchipelagoSession.Locations.CompleteLocationChecks([locationID]);
            NotifyItemFoundAtLocation(locationID);
        }

        internal static void SendArchipelagoLocations(long[] locationIDs)
        {
            var checkedLocations = ArchipelagoSession.Locations.AllLocationsChecked;

            List<long> nonCheckedLocations = locationIDs.ToList();
            List<long> readOnlyCheckedLocations = nonCheckedLocations.ToList();
            foreach(var id in readOnlyCheckedLocations)
            {
                if (checkedLocations.Contains(id))
                {
                    if(OS.DEBUG_COMMANDS)
                    {
                        Logger.LogInfo($"Not sending check for location ID {id} as it has already been checked");
                    }
                    nonCheckedLocations.Remove(id);
                }
            }

            ArchipelagoSession.Locations.CompleteLocationChecks([.. nonCheckedLocations]);

            UpdateServerData();
        }

        internal static async void NotifyItemFoundAtLocation(long locationID)
        {
            if(OS.DEBUG_COMMANDS) { Logger.LogDebug($"Notifying about item found at location ID {locationID}"); }
            var locationItems = await ArchipelagoSession.Locations.ScoutLocationsAsync([locationID]);
            if (!locationItems.Any())
            {
                if(OS.DEBUG_COMMANDS)
                {
                    Logger.LogWarning($"No items found for location ID {locationID}");
                }
                return;
            }
            var item = locationItems[locationID];
            bool isPlayersItem = item.Player.Slot == ArchipelagoSession.ConnectionInfo.Slot;
            string punctuation = ".";

            if(item.Flags.HasFlag(ItemFlags.Advancement))
            {
                punctuation = "!";
            } else if(item.Flags.HasFlag(ItemFlags.Trap))
            {
                punctuation = "...";
            }

            StringBuilder notifBuilder = new("You found ");
            if(isPlayersItem)
            {
                notifBuilder.Append("your ");
                notifBuilder.Append(item.ItemDisplayName);
            } else
            {
                notifBuilder.Append(item.ItemDisplayName);
                notifBuilder.Append(" for ");
                notifBuilder.Append(item.Player.Name);
            }
            notifBuilder.Append(punctuation);
            notifBuilder.Append(" (");
            notifBuilder.Append(item.LocationDisplayName);
            notifBuilder.Append(")");

            if (OS.DEBUG_COMMANDS) Logger.LogDebug(notifBuilder.ToString());

            SpeakAsSystem(notifBuilder.ToString());
        }

        private static async void RetrieveDataFromServer()
        {
            var existingData = await ArchipelagoSession.DataStorage[Scope.Slot, "userdata"].GetAsync();
            IsNewRun = !existingData.HasValues;
            Logger.LogDebug($"Is new run: {IsNewRun}");

            var defaultUserData = JToken.FromObject(new HacknetArchipelagoUserData());
            ArchipelagoSession.DataStorage[Scope.Slot, "userdata"].Initialize(defaultUserData);
            if (IsNewRun) return;

            var storedUserData = ArchipelagoSession.DataStorage[Scope.Slot, "userdata"].To<HacknetArchipelagoUserData>();
            
            _factionAccess = (FactionAccess)storedUserData.StoredFactionAccess;
            _shellLimit = storedUserData.StoredShellLimit;
            _ramLimit = storedUserData.StoredRAMLimit;
            _remainingMissionSkips = storedUserData.RemainingMissionSkips;
            _remainingForceHacks = storedUserData.RemainingForceHacks;

            if(OS.DEBUG_COMMANDS)
            {
                Logger.LogDebug($"Stored Faction Access: {storedUserData.StoredFactionAccess} / Local: {_factionAccess}");
                Logger.LogDebug($"Stored Shell Limit: {storedUserData.StoredShellLimit}");
                Logger.LogDebug($"Stored RAM Limit: {storedUserData.StoredRAMLimit}");
                Logger.LogDebug($"Stored Skips: {storedUserData.RemainingMissionSkips}");
                Logger.LogDebug($"Stored FHs: {storedUserData.RemainingForceHacks}");
            }
        }

        private static void UpdateServerData()
        {
            bool dataIsNull = ArchipelagoSession.DataStorage[Scope.Slot, "userdata"] == null;
            if(dataIsNull)
            {
                ArchipelagoSession.DataStorage[Scope.Slot, "userdata"] = JObject.FromObject(new HacknetArchipelagoUserData());
                return;
            }

            HacknetArchipelagoUserData userData = new()
            {
                StoredFactionAccess = (int)_factionAccess,
                StoredShellLimit = _shellLimit,
                StoredRAMLimit = _ramLimit,
                RemainingMissionSkips = _remainingMissionSkips,
                RemainingForceHacks = _remainingForceHacks
            };
            ArchipelagoSession.DataStorage[Scope.Slot, "userdata"] = JObject.FromObject(userData);
        }

        internal static async void DisconnectFromArchipelago()
        {
            if(ArchipelagoSession != null)
            {
                if(OS.currentInstance != null) UpdateServerData();

                await ArchipelagoSession.Socket.DisconnectAsync();
                ArchipelagoSession = null;
                SlotData = null;
                DeathLinkService = null;
                Logger.LogInfo("Successfully disconnected from Archipelago.");
            }
        }

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

        internal static bool PlayerHasItem(string itemName)
        {
            Console.WriteLine($"Item Name: {itemName} | Is Null or Whitespace: {itemName.IsNullOrWhiteSpace()}");
            if(itemName.IsNullOrWhiteSpace())
            {
                return false;
            }
            return _localInventory.ContainsKey(itemName);
        }

        private static ReceivedItemsHelper _itemsCache;

        private static readonly List<string> _junkItems = ["l33t hax0r skillz"];
        private static readonly List<string> _specialItems = [
                "Progressive Faction Access",
                "Progressive Shell Limit",
                "Progressive RAM",
                "Mission Skip", "ForceHack",
                "Random IRC Log"
            ];
        private static readonly List<string> _trapNames = [
                "Fake Connection", "ForkBomb",
                "Reset PointClicker Points",
                "ETAS Trap"
            ];
        private static readonly List<string> _eventNames = [
                "Fulfill Bit's Final Request",
                "Altitude Loss",
                "Become A Veteran",
                "Entropy VIP",
                "CSEC VIP",
                "CSEC Member ID"
            ];

        private static int facAccessReceived = (int)_factionAccess;
        private static int facAccessQueued = 0;

        private static void ReceiveArchipelagoItem(ReceivedItemsHelper receivedItemsHelper)
        {
            facAccessReceived = (int)_factionAccess;

            if(OS.currentInstance == null)
            {
                _itemsCache = receivedItemsHelper;
                return;
            }
            _itemsCache = null;

            while(receivedItemsHelper.Any() && OS.currentInstance != null)
            {
                var item = receivedItemsHelper.PeekItem();

                if(item.ItemDisplayName == "Progressive Faction Access")
                {
                    facAccessQueued++;
                    if(facAccessQueued <= facAccessReceived)
                    {
                        receivedItemsHelper.DequeueItem();
                        continue;
                    }
                };

                if (OS.DEBUG_COMMANDS) Logger.LogDebug($"Received Item: {item.ItemDisplayName} ({item.ItemId})");
                CollectArchipelagoItem(receivedItemsHelper.PeekItem(), true);
                receivedItemsHelper.DequeueItem();
            }

            UpdateServerData();
        }

        public static void ForceCheckItemsCache()
        {
            if (_itemsCache == null) return;
            ReceiveArchipelagoItem(_itemsCache);
        }

        private const int RAM_UPGRADE_AMOUNT = 50;

        internal static void CollectArchipelagoItem(ItemInfo itemInfo, bool logUnknownItems = false,
            bool ignoreNonExecutables = false)
        {
            var itemID = (int)itemInfo.ItemId;
            string itemName = itemInfo.ItemName;

            if(itemInfo.Player.Slot != ArchipelagoSession.ConnectionInfo.Slot)
            {
                SpeakAsSystem($"Received {itemInfo.ItemDisplayName}!");
            }

            if (_junkItems.Contains(itemName) || PlayerHasItem(itemName) || _eventNames.Contains(itemName))
            {
                if(OS.DEBUG_COMMANDS && PlayerHasItem(itemName))
                {
                    Logger.LogDebug($"Not notifying user of item {itemName} - user already has item");
                }
                return;
            }

            bool isCommonItem = ArchipelagoItems.ArchipelagoItemToData.ContainsKey(itemID);

            if (isCommonItem)
            {
                HandleCommonItem(itemName, itemID);
            }
            else if (_specialItems.Contains(itemName))
            {
                if (OS.DEBUG_COMMANDS) Logger.LogDebug($"Handling special item {itemName} ({itemID})");
                HandleSpecialItem(itemName, itemID);
            }
            else if (_trapNames.Contains(itemName))
            {
                HandleTrap(itemName, itemID);
            }
            else if (itemName.StartsWith("PointClicker"))
            {

            }
            else if (logUnknownItems)
            {
                Logger.LogError($"Received unknown Archipelago item \"{itemName}\" with ID of {itemID}. Skipping...");
            }
        }

        private static void HandleCommonItem(string itemName, int itemID)
        {
            var item = ArchipelagoItems.ArchipelagoItemToData[itemID];
            foreach (var key in item.Keys)
            {
                var filename = key + ".exe";
                var filedata = item[key];
                AddItemFileToPlayerComputer(filename, filedata);
            }
            AddToInventory(itemName);
        }

        private static void HandleSpecialItem(string itemName, int itemID)
        {
            switch(itemID)
            {
                case 119: // Progressive Faction Access
                    _factionAccess++;
                    switch(_factionAccess)
                    {
                        case FactionAccess.Entropy:
                            SpeakAsSystem("You can now take on missions from Entropy!");
                            break;
                        case FactionAccess.LabyrinthsOrCSEC:
                            bool labsShuffled = SlotData.ShuffleLabyrinths;
                            string message = labsShuffled ?
                                "You can now take on The Kaguya Trials!" :
                                "You can now take on missions from CSEC!";
                            SpeakAsSystem(message);
                            break;
                        case FactionAccess.CSEC:
                            SpeakAsSystem("You can now take on missions from CSEC!");
                            break;
                        default:
                            SpeakAsSystem("Progressive Faction Access Received!");
                            break;
                    }
                    break;
                case 131: // Progressive Shell Limit
                    _shellLimit++;
                    break;
                case 132: // Progressive RAM
                    _ramLimit += RAM_UPGRADE_AMOUNT;
                    break;
                case 140: // Mission Skip
                    _remainingMissionSkips++;
                    break;
                case 141: // ForceHack
                    _remainingForceHacks++;
                    break;
                case 143: // Random IRC Log
                    break;
            }
        }

        private static void HandleTrap(string trapName, int itemID)
        {
            OS os = OS.currentInstance;

            switch(itemID)
            {
                case 666: // ETAS Trap
                    os.TraceDangerSequence.BeginTraceDangerSequence();
                    SpeakAsSystem("ETAS TRAP ACTIVATED : PREPARE FOR SYSTEM REBOOT");
                    break;
                case 667: // Fake Connection
                    os.IncConnectionOverlay.Activate();
                    break;
                case 668: // Reset PointClicker Points
                    ChangePointClickerPoints(-1);
                    break;
                case 669: // ForkBomb
                    Multiplayer.parseInputMessage($"eForkBomb {os.thisComputer.ip}", os);
                    SpeakAsSystem("!!! WARNING : FORKBOMB RECEIVED !!!");
                    break;
            }
        }

        private static void AddToInventory(string itemName, int amount = 1)
        {
            if(!_localInventory.ContainsKey(itemName))
            {
                _localInventory.Add(itemName, amount);
            } else
            {
                _localInventory[itemName] += amount;
            }
        }

        private static void AddItemFileToPlayerComputer(string filename, string data)
        {
            bool isExe = filename.EndsWith(".exe");
            string folder = isExe ? "bin" : "home";

            Computer playerComp = OS.currentInstance.thisComputer;
            FileEntry file = new(data, filename);
            Folder fileFolder = playerComp.getFolderFromPath(folder);
            
            if(fileFolder == null)
            {
                Logger.LogError($"Couldn't add {filename} to player computer: folder {folder} doesn't exist!");
                return;
            }

            bool fileExists = fileFolder.containsFileWithData(data);
            if(fileExists)
            {
                Logger.LogInfo($"Couldn't add {filename} to player computer: a file with that filedata already exists.");
                return;
            }

            fileFolder.files.Add(file);
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

        internal static void SendVictory()
        {
            ArchipelagoSession.SetGoalAchieved();
            SpeakAsSystem("!!! VICTORY !!!");
        }

        internal static void ChangePointClickerPoints(int amount)
        {
            bool reset = amount < 0;

            Computer ptcComp = ComputerLookup.FindById("pointclicker");
            PointClickerDaemon ptcDaemon = (PointClickerDaemon)ptcComp.getDaemon(typeof(PointClickerDaemon));
            
            if(reset)
            {
                ptcDaemon.activeState.points = 0;
            } else
            {
                ptcDaemon.activeState.points += amount;
            }
        }

        internal static void ChangePointClickerRate(int amount)
        {
            Computer ptcComp = ComputerLookup.FindById("pointclicker");
            PointClickerDaemon ptcDaemon = (PointClickerDaemon)ptcComp.getDaemon(typeof(PointClickerDaemon));

            ptcDaemon.currentRate += amount;
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
