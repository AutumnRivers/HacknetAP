using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using Hacknet;
using Hacknet.Gui;
using Hacknet.UIUtils;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Daemon;

namespace HacknetArchipelago.Daemons
{
    public class ArchipelagoIRCDaemon : BaseDaemon
    {
        public ArchipelagoIRCDaemon(Computer c, string _, OS os) : base(c, "Archipelago IRC", os)
        {
            GlobalInstance = this;
        }

        public override string Identifier => "Archipelago IRC Text";

        public static List<ArchipelagoIRCEntry> ArchipelagoEntries = [];

        public static ArchipelagoIRCDaemon GlobalInstance;

        public string PlayerName => ArchipelagoManager.PlayerName;

        public ScrollableSectionedPanel entriesPanel = new(0, GuiData.spriteBatch.GraphicsDevice);

        public static readonly Dictionary<ItemFlags, Color> ItemColors = new()
        {
            { ItemFlags.Advancement, Color.Plum },
            { ItemFlags.NeverExclude, Color.LightSkyBlue },
            { ItemFlags.None, Color.Cyan },
            { ItemFlags.Trap, Color.LightPink }
        };

        public static readonly Color LocationColor = Color.LightGreen;
        public static readonly Color OwnColor = Color.Violet;
        public static readonly Color TextColor = Color.WhiteSmoke;
        public static readonly Color OtherPlayerColor = Color.Moccasin;

        public void AddIRCEntry(ArchipelagoIRCEntry entry)
        {
            bool isItemEntry = entry.GetType() == typeof(ArchipelagoItemIRCEntry);

            if(isItemEntry)
            {
                var itemEntry = (ArchipelagoItemIRCEntry)entry;
                AddItemIRCEntry(itemEntry.Sender, itemEntry.Receiver, itemEntry.ItemName,
                    itemEntry.ItemLocation, itemEntry.ItemClassification);
            } else { AddSimpleIRCEntry(entry.Sender, entry.Content); }
        }

        public void AddSimpleIRCEntry(string name, string content)
        {
            ArchipelagoIRCEntry entry = new(name, content);
            if (ArchipelagoEntries.Contains(entry)) return;
            ArchipelagoEntries.Add(entry);
        }

        public void AddItemIRCEntry(string sender, string receiver, string itemName, string location, ItemFlags classification)
        {
            ArchipelagoItemIRCEntry entry = new(sender, receiver, itemName, location, classification);
            if (ArchipelagoEntries.Contains(entry)) return;
            ArchipelagoEntries.Add(entry);
        }

        public const string SYSTEM_SENDER = "Archi";
        public const string SENDER_TEMPLATE = "{0}> ";
        public const string LOCATION_TEMPLATE = " ({0})";
        public const string SEPERATOR = " ";

        public float SeperatorWidth = GuiData.smallfont.MeasureString(SEPERATOR).X;

        public int MessagesSent = 0;

        private const string FULL_CHARACTERS = "abcdefghijklmnopqrstuvwxyz0123456789";
        private const int SENDER_OFFSET = 75;

        public void DrawIRCEntry(ArchipelagoIRCEntry entry, Rectangle bounds)
        {
            bool isItemEntry = entry.GetType() == typeof(ArchipelagoItemIRCEntry);
            
            if(isItemEntry)
            {
                DrawItemIRCEntry((ArchipelagoItemIRCEntry)entry, bounds);
                return;
            }

            string sender = string.Format(SENDER_TEMPLATE, entry.Sender);
            bool isSentByPlayer = entry.Sender == PlayerName;

            int index = ArchipelagoEntries.IndexOf(entry);

            string messageContent = Utils.SmartTwimForWidth(entry.Content, (int)(bounds.Width - SeperatorWidth), GuiData.smallfont);

            float yOffset = GuiData.smallfont.MeasureString(FULL_CHARACTERS).Y * (ArchipelagoEntries.Count - index);
            bool willHitTop = yOffset + GuiData.smallfont.MeasureString(messageContent).Y >= bounds.Height;

            if (willHitTop) return;

            TextItem.doSmallLabel(new(0, yOffset), sender, isSentByPlayer ? OwnColor : OtherPlayerColor);
            TextItem.doSmallLabel(new(SENDER_OFFSET, yOffset), messageContent, TextColor);
        }

        public void DrawItemIRCEntry(ArchipelagoItemIRCEntry entry, Rectangle bounds)
        {
            int index = ArchipelagoEntries.IndexOf(entry);

            string sender = string.Format(SENDER_TEMPLATE, SYSTEM_SENDER);
            bool isSentByPlayer = entry.Sender == PlayerName;
            bool isReceivedByPlayer = entry.Receiver == PlayerName;
            bool isOwnItem = isSentByPlayer && isReceivedByPlayer;

            string first = string.Format("{0}", entry.Sender);
            string second = isOwnItem ? " found their " : " found ";
            string third = isOwnItem ? "{0}" : "{0}'s ";
            string fourth = isOwnItem ? " (" : "{0}";
            string fifth = isOwnItem ? "{0}" : " (";
            string sixth = isOwnItem ? ")" : "{0}";
            string seventh = isOwnItem ? "" : ")";

            float xOffset = SENDER_OFFSET;
            float yOffset = GuiData.smallfont.MeasureString(FULL_CHARACTERS).Y * (ArchipelagoEntries.Count - index);
            TextItem.doSmallLabel(new(0, yOffset), sender, TextColor);

            float firstOffset = GuiData.smallfont.MeasureString(first).X;
            TextItem.doSmallLabel(new(xOffset, yOffset), first, isSentByPlayer ? OwnColor : OtherPlayerColor);
            xOffset += firstOffset;

            TextItem.doSmallLabel(new(xOffset, yOffset), second, TextColor);
            xOffset += GuiData.smallfont.MeasureString(second).X;

            TextItem.doSmallLabel(new(xOffset, yOffset), third, isOwnItem ? ItemColors[entry.ItemClassification] :
                isReceivedByPlayer ? OwnColor : OtherPlayerColor);
            xOffset += GuiData.smallfont.MeasureString(third).X;

            TextItem.doSmallLabel(new(xOffset, yOffset), fourth, isOwnItem ? TextColor : ItemColors[entry.ItemClassification]);
            xOffset += GuiData.smallfont.MeasureString(fourth).X;

            TextItem.doSmallLabel(new(xOffset, yOffset), fifth, isOwnItem ? LocationColor : TextColor);
            xOffset += GuiData.smallfont.MeasureString(fifth).X;

            TextItem.doSmallLabel(new(xOffset, yOffset), sixth, isOwnItem ? TextColor : LocationColor);
            xOffset += GuiData.smallfont.MeasureString(sixth).X;

            TextItem.doSmallLabel(new(xOffset, yOffset), seventh, TextColor);
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            PatternDrawer.draw(bounds, 0, Color.Transparent, OS.currentInstance.defaultHighlightColor * 0.5f,
                sb, PatternDrawer.thinStripe);
            RenderedRectangle.doRectangle(SENDER_OFFSET - 4, bounds.Y, 2, bounds.Height, Color.LightGray * 0.25f);

            entriesPanel.PanelHeight = bounds.Height;
            entriesPanel.NumberOfPanels = ArchipelagoEntries.Count + 1;

            Action<int, Rectangle, SpriteBatch> drawEntries = delegate (int index, Rectangle drawbounds, SpriteBatch spriteBatch)
            {
                if(index + 1 < ArchipelagoEntries.Count)
                {
                    DrawIRCEntry(ArchipelagoEntries[index], drawbounds);
                }
            };
        }
    }

    public class ArchipelagoIRCEntry
    {
        public string Sender;
        public string Content;

        public ArchipelagoIRCEntry(string from, string content = null)
        {
            Sender = from;
            Content = content;
        }
    }

    public class ArchipelagoItemIRCEntry : ArchipelagoIRCEntry
    {
        public string Receiver;
        public ItemFlags ItemClassification;
        public string ItemName;
        public string ItemLocation;

        public ArchipelagoItemIRCEntry(string from, string to, string name, string location,
            ItemFlags classification) : base(from)
        {
            Sender = from;
            Receiver = to;
            ItemName = name;
            ItemLocation = location;
            ItemClassification = classification;
        }
    }
}
