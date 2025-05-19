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

        private const int SENDER_OFFSET = 150;

        private float _baseYOffset = 0;
        private const int SPACING = 2;

        public void DrawIRCEntry(ArchipelagoIRCEntry entry, Rectangle bounds)
        {
            if (ArchipelagoEntries.IndexOf(entry) == 0) _baseYOffset = bounds.Y;

            bool isItemEntry = entry.GetType() == typeof(ArchipelagoItemIRCEntry);
            
            if(isItemEntry)
            {
                DrawItemIRCEntry((ArchipelagoItemIRCEntry)entry, bounds);
                return;
            }

            string sender = string.Format(SENDER_TEMPLATE, entry.Sender);
            bool isSentByPlayer = entry.Sender == PlayerName;

            string messageContent = Utils.SmartTwimForWidth(entry.Content, (int)(bounds.Width - SENDER_OFFSET), GuiData.smallfont);
            float yOffset = _baseYOffset + SPACING;

            TextItem.doSmallLabel(new(bounds.X, yOffset), sender, isSentByPlayer ? OwnColor : OtherPlayerColor);
            TextItem.doSmallLabel(new(SENDER_OFFSET + bounds.X, yOffset), messageContent, TextColor);

            _baseYOffset += GuiData.smallfont.MeasureString(messageContent).Y;

            RenderedRectangle.doRectangle(bounds.X, (int)_baseYOffset, bounds.Width, 2, Color.WhiteSmoke * 0.15f);
        }

        public void DrawItemIRCEntry(ArchipelagoItemIRCEntry entry, Rectangle bounds)
        {
            if (entry.Sender == "Server") return;
 
            string sender = string.Format(SENDER_TEMPLATE, SYSTEM_SENDER);
            bool isSentByPlayer = entry.Sender == PlayerName;
            bool isReceivedByPlayer = entry.Receiver == PlayerName;
            bool isOwnItem = (isSentByPlayer && isReceivedByPlayer);

            StringBuilder offsetBuilder = new();

            string first = string.Format("{0}", entry.Sender);
            string second = isOwnItem ? " found their " : " found ";
            string third = isOwnItem ? "{0}" : "{0}'s ";
            string fourth = isOwnItem ? " (" : "{0}";
            string fifth = isOwnItem ? "{0}" : " (";
            string sixth = isOwnItem ? ")" : "{0}";
            string seventh = isOwnItem ? "" : ")";

            float xOffset = SENDER_OFFSET + bounds.X;
            float yOffset = _baseYOffset + SPACING;
            TextItem.doSmallLabel(new(bounds.X, yOffset), sender, _patternColor);

            float firstOffset = GuiData.smallfont.MeasureString(first).X;
            TextItem.doSmallLabel(new(xOffset, yOffset), first, isSentByPlayer ? OwnColor : OtherPlayerColor);
            xOffset += firstOffset;
            offsetBuilder.Append(first);

            TextItem.doSmallLabel(new(xOffset, yOffset), second, TextColor);
            xOffset += GuiData.smallfont.MeasureString(second).X;
            offsetBuilder.Append(second);

            third = string.Format(third, isOwnItem ? entry.ItemName : entry.Receiver);
            TextItem.doSmallLabel(new(xOffset, yOffset), third, isOwnItem ? ItemColors[entry.ItemClassification] :
                isReceivedByPlayer ? OwnColor : OtherPlayerColor);
            xOffset += GuiData.smallfont.MeasureString(third).X;
            offsetBuilder.Append(third);

            fourth = !isOwnItem ? string.Format(fourth, entry.ItemName) : fourth;
            TextItem.doSmallLabel(new(xOffset, yOffset), fourth, isOwnItem ? TextColor : ItemColors[entry.ItemClassification]);
            xOffset += GuiData.smallfont.MeasureString(fourth).X;
            offsetBuilder.Append(fourth);

            fifth = isOwnItem ? string.Format(fifth, entry.ItemLocation) : fifth;
            TextItem.doSmallLabel(new(xOffset, yOffset), fifth, isOwnItem ? LocationColor : TextColor);
            xOffset += GuiData.smallfont.MeasureString(fifth).X;
            offsetBuilder.Append(fifth);

            sixth = !isOwnItem ? string.Format(sixth, entry.ItemLocation) : sixth;
            TextItem.doSmallLabel(new(xOffset, yOffset), sixth, isOwnItem ? TextColor : LocationColor);
            xOffset += GuiData.smallfont.MeasureString(sixth).X;
            offsetBuilder.Append(sixth);

            TextItem.doSmallLabel(new(xOffset, yOffset), seventh, TextColor);
            offsetBuilder.Append(seventh);

            string finalString = Utils.SmartTwimForWidth(offsetBuilder.ToString(), bounds.Width - SENDER_OFFSET, GuiData.smallfont);
            _baseYOffset += GuiData.smallfont.MeasureString(finalString).Y;

            RenderedRectangle.doRectangle(bounds.X, (int)_baseYOffset, bounds.Width, 2, Color.WhiteSmoke * 0.15f);
        }

        private readonly List<Color> ArchipelagoColorCycle = new()
        {
            Color.LightSalmon,
            Color.SteelBlue,
            Color.PaleGoldenrod,
            Color.IndianRed,
            Color.PaleGreen,
            Color.Plum
        };
        private Color _currentColor = Color.LightSalmon;
        private Color _nextColor = Color.SteelBlue;
        private float _colorCycleProgress = 0f;
        private Color _patternColor = Color.LightSalmon;

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            _colorCycleProgress += (float)OS.currentInstance.lastGameTime.ElapsedGameTime.TotalSeconds / 10;
            if(_colorCycleProgress >= 1)
            {
                _colorCycleProgress = 0;
                int curIndex = _currentColor == ArchipelagoColorCycle.Last() ? 0 : ArchipelagoColorCycle.IndexOf(_currentColor) + 1;
                int nextIndex = _nextColor == ArchipelagoColorCycle.Last() ? 0 : ArchipelagoColorCycle.IndexOf(_nextColor) + 1;

                _currentColor = ArchipelagoColorCycle[curIndex];
                _nextColor = ArchipelagoColorCycle[nextIndex];
            }
            _patternColor = Color.Lerp(_currentColor, _nextColor, _colorCycleProgress);

            PatternDrawer.draw(bounds, 0.15f, Color.Transparent, _patternColor * 0.25f,
                sb, PatternDrawer.thinStripe);
            RenderedRectangle.doRectangle(SENDER_OFFSET - 4 + bounds.X, bounds.Y, 2, bounds.Height, Color.LightGray * 0.25f);

            entriesPanel.PanelHeight = (int)GuiData.smallfont.MeasureString("abcdef").Y;
            entriesPanel.NumberOfPanels = ArchipelagoEntries.Count + 1;

            Action<int, Rectangle, SpriteBatch> drawEntries = delegate (int index, Rectangle drawbounds, SpriteBatch spriteBatch)
            {
                if(index < ArchipelagoEntries.Count)
                {
                    DrawIRCEntry(ArchipelagoEntries[index], drawbounds);
                }
            };

            _baseYOffset = 0;
            entriesPanel.Draw(drawEntries, sb, bounds);
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
