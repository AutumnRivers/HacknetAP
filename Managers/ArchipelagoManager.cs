using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;

using BepInEx.Logging;

using Newtonsoft.Json.Linq;

using Pathfinder.Event.BepInEx;
using Pathfinder.Event.Gameplay;

using static HacknetArchipelago.Managers.InventoryManager;
using static HacknetArchipelago.Managers.DeathLinkManager;
using Hacknet;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using HacknetArchipelago.Daemons;
using Archipelago.MultiClient.Net.MessageLog.Messages;

namespace HacknetArchipelago.Managers
{
    public static class ArchipelagoManager
    {
        public const string GameString = HacknetAPCore.GameString;

        public static ArchipelagoSession Session;
        public static HacknetAPSlotData SlotData;

        public static int PlayerSlot = -1; // -1 = not set
        public static bool IsConnected = false;
        public static bool IsNewRun = false;

        public static GoalManager EventManager = new();

        private static ManualLogSource Logger => HacknetAPCore.Logger;

        private static bool _warnedAboutDisconnect = false;
        private static ReceivedItemsHelper _itemsCache;

        public static string PlayerName
        {
            get
            {
                if (Session == null) return "Someone";
                return Session.Players.ActivePlayer.Name;
            }
        }

        internal static void AssureArchiConnection(OSUpdateEvent oSUpdateEvent)
        {
            if (oSUpdateEvent.OS == null) return;

            if (!_warnedAboutDisconnect && Session == null)
            {
                HacknetAPCore.SpeakAsSystem("No Archipelago session is currently active!! Something went VERY wrong!");
                oSUpdateEvent.OS.warningFlash();
                oSUpdateEvent.OS.beepSound.Play();
                IsConnected = false;
                _warnedAboutDisconnect = true;
            }
            else if (!_warnedAboutDisconnect && !Session.Socket.Connected)
            {
                HacknetAPCore.SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                HacknetAPCore.SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                HacknetAPCore.SpeakAsSystem("Lost connection to Archipelago! Please reconnect with 'rearchi'");
                oSUpdateEvent.OS.warningFlash();
                oSUpdateEvent.OS.beepSound.Play();
                Logger.LogError("Lost connection to Archipelago!");
                IsConnected = false;
                _warnedAboutDisconnect = true;
            }
            else
            {
                IsConnected = true;
                _warnedAboutDisconnect = false;
            }
        }

        internal static LoginResult ConnectToArchipelago(string uri, string slotName, string password = "")
        {
            var session = ArchipelagoSessionFactory.CreateSession(uri);
            session.Items.ItemReceived -= ReceiveArchipelagoItem;
            session.Items.ItemReceived += ReceiveArchipelagoItem;
            var result = session.TryConnectAndLogin(GameString,
                        slotName,
                        ItemsHandlingFlags.AllItems,
                        null,
                        [],
                        null,
                        password, true);
            if (result.Successful)
            {
                var sessionData = (LoginSuccessful)result;
                SlotData = new HacknetAPSlotData(sessionData.SlotData);
                PlayerSlot = session.ConnectionInfo.Slot;
                if (SlotData.DeathLink)
                {
                    SetupDeathLink();
                }
                Session = session;
                IsConnected = true;
                Logger.LogInfo("Successfully (re-)connected to Archipelago!");
                RetrieveDataFromServer();
                LocationManager.SendCachedLocations();
            }
            return result;
        }

        internal static async void DisconnectFromArchipelago()
        {
            if (Session != null)
            {
                if (OS.currentInstance != null) UpdateServerData();

                await Session.Socket.DisconnectAsync();
                Session = null;
                SlotData = null;
                DLService = null;
                Logger.LogInfo("Successfully disconnected from Archipelago.");
            }
        }

        internal static void AttemptSendVictory()
        {
            if (!HasReachedGoal()) return;

            Session.SetGoalAchieved();
            HacknetAPCore.SpeakAsSystem("!!! VICTORY !!!", true);
        }

        public static bool HasReachedGoal()
        {
            switch(SlotData.PlayerGoal)
            {
                case HacknetAPSlotData.VictoryCondition.Heartstopper:
                default:
                    return EventManager.BrokeHeart;
                case HacknetAPSlotData.VictoryCondition.AltitudeLoss:
                    return EventManager.LostAltitude;
                case HacknetAPSlotData.VictoryCondition.Veteran:
                    return EventManager.IsVeteran;
                case HacknetAPSlotData.VictoryCondition.VIP:
                    return EventManager.IsFullVIP;
                case HacknetAPSlotData.VictoryCondition.Completionist:
                    return EventManager.IsCompletionist;
            }
        }

        internal static void SendTextMessageToIRC(LogMessage message)
        {
            if (message.GetType() != typeof(ChatLogMessage)) return;

            var chatMessage = (ChatLogMessage)message;

            try
            {
                ArchipelagoIRCEntry entry = new(chatMessage.Player.Name, chatMessage.Message);
                ArchipelagoIRCDaemon.GlobalInstance.AddIRCEntry(entry);
            } catch(Exception e)
            {
                HacknetAPCore.Logger.LogError("Unable to add text log to IRC:\n" + e.ToString());
            }
        }

        internal static void UpdateServerDataOnClose(UnloadEvent unloadEvent)
        {
            if (Session == null) return;

            UpdateServerData();
        }

        internal static void UpdateServerData()
        {
            bool dataIsNull = Session.DataStorage[Scope.Slot, "userdata"] == null;
            if (dataIsNull)
            {
                Session.DataStorage[Scope.Slot, "userdata"] = JObject.FromObject(new HacknetArchipelagoUserData());
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
            Session.DataStorage[Scope.Slot, "userdata"] = JObject.FromObject(userData);
        }

        internal static void UpdateServerToggle(string toggleName, bool newValue)
        {
            Session.DataStorage[Scope.Slot, toggleName] = newValue;
        }

        internal async static Task<bool> GetServerToggle(string toggleName)
        {
            var data = await Session.DataStorage[Scope.Slot, toggleName].GetAsync();
            if(data.Type == JTokenType.Null)
            {
                Session.DataStorage[Scope.Slot, toggleName].Initialize(false);
                return false;
            } else
            {
                return (bool)data;
            }
        }

        internal static async void RetrieveDataFromServer()
        {
            var existingData = await Session.DataStorage[Scope.Slot, "userdata"].GetAsync();
            IsNewRun = !existingData.HasValues;
            Logger.LogDebug($"Is new run: {IsNewRun}");

            var defaultUserData = JToken.FromObject(new HacknetArchipelagoUserData());
            Session.DataStorage[Scope.Slot, "userdata"].Initialize(defaultUserData);
            if (IsNewRun) return;

            var storedUserData = Session.DataStorage[Scope.Slot, "userdata"].To<HacknetArchipelagoUserData>();

            _factionAccess = (FactionAccess)storedUserData.StoredFactionAccess;
            _shellLimit = storedUserData.StoredShellLimit;
            _ramLimit = storedUserData.StoredRAMLimit;
            _remainingMissionSkips = storedUserData.RemainingMissionSkips;
            _remainingForceHacks = storedUserData.RemainingForceHacks;

            if (OS.DEBUG_COMMANDS)
            {
                Logger.LogDebug($"Stored Faction Access: {storedUserData.StoredFactionAccess} / Local: {_factionAccess}");
                Logger.LogDebug($"Stored Shell Limit: {storedUserData.StoredShellLimit}");
                Logger.LogDebug($"Stored RAM Limit: {storedUserData.StoredRAMLimit}");
                Logger.LogDebug($"Stored Skips: {storedUserData.RemainingMissionSkips}");
                Logger.LogDebug($"Stored FHs: {storedUserData.RemainingForceHacks}");
            }

            EventManager.BrokeHeart = await GetServerToggle("achieved_heartstopper");
            EventManager.LostAltitude = await GetServerToggle("achieved_altitudeloss");
            EventManager.IsVeteran = await GetServerToggle("achieved_veteran");

            EventManager.IsEntropyVIP = await GetServerToggle("is_entropy_vip");
            EventManager.IsCSECVIP = await GetServerToggle("is_csec_vip");
        }

        public static void ForceCheckItemsCache()
        {
            if (_itemsCache == null) return;
            ReceiveArchipelagoItem(_itemsCache);
        }

        private static void ReceiveArchipelagoItem(ReceivedItemsHelper receivedItemsHelper)
        {
            if (OS.currentInstance == null)
            {
                _itemsCache = receivedItemsHelper;
                return;
            }
            _itemsCache = null;

            while (receivedItemsHelper.Any() && OS.currentInstance != null)
            {
                var item = receivedItemsHelper.PeekItem();
                if(PlayerAlreadyCollectedItem(item))
                {
                    receivedItemsHelper.DequeueItem();
                    continue;
                }

                if (OS.DEBUG_COMMANDS) Logger.LogDebug($"Received Item: {item.ItemDisplayName} ({item.ItemId})");
                CollectArchipelagoItem(receivedItemsHelper.PeekItem(), true);
                receivedItemsHelper.DequeueItem();
            }

            UpdateServerData();
        }

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

        private const int RAM_UPGRADE_AMOUNT = 50;

        internal static void CollectArchipelagoItem(ItemInfo itemInfo, bool logUnknownItems = false,
            bool ignoreNonExecutables = false)
        {
            if(PlayerAlreadyCollectedItem(itemInfo))
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogDebug($"Player already received item {itemInfo.ItemDisplayName} ({itemInfo.ItemId}) " +
                        $"from {itemInfo.Player.Name} at location ID {itemInfo.LocationId}. Skipping...");
                }
                return;
            }
            AddNewItem(itemInfo);

            var itemID = (int)itemInfo.ItemId;
            string itemName = itemInfo.ItemName;

            HacknetAPCore.Logger.LogDebug($"Received Item: {itemInfo.ItemDisplayName} ({itemInfo.ItemId})");

            if (itemInfo.Player.Slot != PlayerSlot)
            {
                HacknetAPCore.SpeakAsSystem($"Received {itemInfo.ItemDisplayName}!");
            }

            if (_junkItems.Contains(itemName) || PlayerHasItem(itemName) || _eventNames.Contains(itemName))
            {
                if (OS.DEBUG_COMMANDS && PlayerHasItem(itemName))
                {
                    Logger.LogDebug($"Not notifying user of item {itemName} - user already has item");
                }
                return;
            }

            try
            {
                ArchipelagoItemIRCEntry itemIRCEntry = new(itemInfo.Player.Name, PlayerName,
                    itemInfo.ItemDisplayName, itemInfo.LocationDisplayName, itemInfo.Flags);
                ArchipelagoIRCDaemon.GlobalInstance.AddIRCEntry(itemIRCEntry);
            } catch(Exception e)
            {
                HacknetAPCore.Logger.LogError("Failed to log received archi item to IRC:\n" + e.ToString());
            }

            bool isCommonItem = ArchipelagoItems.ArchipelagoItemToData.ContainsKey(itemID);

            if (isCommonItem)
            {
                HandleCommonItem(itemInfo);
            }
            else if (_specialItems.Contains(itemName))
            {
                if (OS.DEBUG_COMMANDS) Logger.LogDebug($"Handling special item {itemName} ({itemID})");
                HandleSpecialItem(itemName, itemID);
            }
            else if (_trapNames.Contains(itemName))
            {
                HandleTrap(itemInfo.Player.Name, itemID);
            }
            else if (itemName.StartsWith("PointClicker"))
            {
                PointClickerManager.HandlePointClickerUpgrade(itemName);
            }
            else if (logUnknownItems)
            {
                Logger.LogError($"Received unknown Archipelago item \"{itemName}\" with ID of {itemID}. Skipping...");
            }
        }

        private static void HandleCommonItem(ItemInfo itemInfo)
        {
            var item = ArchipelagoItems.ArchipelagoItemToData[(int)itemInfo.ItemId];
            foreach (var key in item.Keys)
            {
                var filename = key + ".exe";
                var filedata = item[key];
                PlayerManager.AddItemFileToPlayerComputer(filename, filedata);
            }
            AddToInventory(itemInfo.ItemDisplayName, itemInfo.Player.Name);
        }

        private static void HandleSpecialItem(string itemName, int itemID)
        {
            switch (itemID)
            {
                case 119: // Progressive Faction Access
                    _factionAccess++;
                    switch (_factionAccess)
                    {
                        case FactionAccess.Entropy:
                            HacknetAPCore.SpeakAsSystem("You can now take on missions from Entropy!");
                            break;
                        case FactionAccess.LabyrinthsOrCSEC:
                            bool labsShuffled = SlotData.ShuffleLabyrinths;
                            string message = labsShuffled ?
                                "You can now take on The Kaguya Trials!" :
                                "You can now take on missions from CSEC!";
                            HacknetAPCore.SpeakAsSystem(message);
                            break;
                        case FactionAccess.CSEC:
                            HacknetAPCore.SpeakAsSystem("You can now take on missions from CSEC!");
                            break;
                        default:
                            HacknetAPCore.SpeakAsSystem("Progressive Faction Access Received!");
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

        private static void HandleTrap(string itemSentBy, int itemID)
        {
            switch (itemID)
            {
                case 666: // ETAS Trap
                    PlayerManager.ActivateETAS();
                    HacknetAPCore.SpeakAsSystem("ETAS TRAP ACTIVATED : PREPARE FOR SYSTEM REBOOT");
                    break;
                case 667: // Fake Connection
                    PlayerManager.FlashFakeConnection();
                    break;
                case 668: // Reset PointClicker Points
                    PointClickerManager.ChangePointClickerPoints(-1);
                    break;
                case 669: // ForkBomb
                    PlayerManager.ForkbombPlayer(itemSentBy);
                    break;
            }
        }
    }

    public class GoalManager
    {
        public bool BrokeHeart = false;
        public bool LostAltitude = false;
        public bool IsVeteran = false;

        public bool IsEntropyVIP = false;
        public bool IsCSECVIP = false;

        public bool IsCompletionist => BrokeHeart && LostAltitude && IsVeteran &&
            IsEntropyVIP && IsCSECVIP;

        public bool IsFullVIP => IsEntropyVIP && IsCSECVIP;
    }
}
