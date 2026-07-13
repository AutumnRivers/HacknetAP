using System;
using Hacknet;
using Hacknet.Gui;
using Hacknet.Effects;
using Pathfinder.Daemon;
using Pathfinder.Meta.Load;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.GUI;

namespace HacknetArchipelago.Daemons;

[Daemon]
public class ArchipelagoMissionListingDaemon : BaseDaemon
{
    public ArchipelagoMissionListingDaemon(Computer computer, string serviceName, OS opSystem) :
        base(computer, serviceName, opSystem) {}

    public override string Identifier => "Archipelago Mission Listing";
    public string MissionSourceFolderPath = "Content/Missions/MainHub/FirstSet/";

    public BarcodeEffect SideBarcode = new(50);
    public const float BARCODE_SIZE_MULTIPLIER = 0.1f;

    public enum ArchipelagoMissionListingState
    {
        Unauthenticated,
        MissionListing,
        ViewMission
    }

    public ArchipelagoMissionListingState State { get; set; } = ArchipelagoMissionListingState.Unauthenticated;
    public MissionListingEntry CurrentMission { get; set; }

    public List<MissionListingEntry> Missions { get; set; } = [];

    public static MissionListingEntry ReadMissionFile(string filepath)
    {
        var mission = (ActiveMission)ComputerLoader.readMission(filepath);
        if(mission == null) return null;
        return new MissionListingEntry(mission);
    }

    public void AddMission(MissionListingEntry mission)
    {
        if (Missions.All(m => m.Mission != mission.Mission))
        {
            Missions.Add(mission);
        }
    }

    public void RefreshAllMissionStatuses()
    {
        foreach (var missionListingEntry in Missions)
        {
            missionListingEntry.RefreshCanBeClaimed();
        }
    }

    public void InitMissionListing()
    {
        Missions.Clear();

        var missionFiles = new DirectoryInfo(MissionSourceFolderPath).GetFiles().ToList();

        foreach (var mission in
                 missionFiles.Select(missionFile => ReadMissionFile(MissionSourceFolderPath + missionFile.Name))
                     .Where(mission => mission != null))
        {
            AddMission(mission);
        }
    }

    public void ClaimMission(MissionListingEntry missionListingEntry)
    {
        Folder listingsFolder = comp.getFolderFromPath("missions", true);
        Missions.Remove(missionListingEntry);
        CurrentMission = null;
        OS.currentInstance.currentMission = missionListingEntry.Mission;
        if (missionListingEntry.ButtonIndex > -1)
        {
            PFButton.ReturnID(missionListingEntry.ButtonIndex);
            missionListingEntry.ButtonIndex = -1;
        }

        var missionListingFile = missionListingEntry.RelatedFile;
        if (listingsFolder.containsFile(missionListingFile.name))
        {
            listingsFolder.files.Remove(missionListingFile);
        }
    }

    public void AddMissionToListing(ActiveMission mission, int desiredIndex = -1)
    {
        var missionListingEntry = new MissionListingEntry(mission);
        var missionFile = new FileEntry(MissionSerializer.generateMissionFile(mission),
            mission.postingTitle.Replace(' ', '_'));
        var listingsFolder = comp.getFolderFromPath("missions", true);
        if (Missions.All(m => m.Mission != mission))
        {
            if (desiredIndex > -1)
            {
                Missions.Insert(desiredIndex, missionListingEntry);
                listingsFolder.files.Insert(desiredIndex, missionFile);
            }
            else
            {
                Missions.Add(missionListingEntry);
                listingsFolder.files.Add(missionFile);
            }
        }
    }

    public void AddCompleteMissionToListing(MissionListingEntry entry)
    {
        if (Missions.All(m => m.Mission != entry.Mission))
        {
            Missions.Add(entry);
        }
    }

    private void AddMissionFromFile(FileEntry missionFile)
    {
        var mission = (ActiveMission)MissionSerializer.restoreMissionFromFile(missionFile.data, out _);
        var parsedMissionEntry = new MissionListingEntry(mission)
        {
            RelatedFile = missionFile
        };
        
        AddCompleteMissionToListing(parsedMissionEntry);
    }

    public override void navigatedTo()
    {
        base.navigatedTo();

        State = ArchipelagoMissionListingState.Unauthenticated;
        RefreshAllMissionStatuses();
    }

    public override void initFiles()
    {
        base.initFiles();
        
        os.delayer.Post(ActionDelayer.NextTick(), InitMissionListing);
        SideBarcode.leftRightBias = true;
    }

    public override void loadInit()
    {
        base.loadInit();
        
        var listingsFolder = comp.getFolderFromPath("missions", true);
        if (listingsFolder == null)
        {
            HacknetAPCore.Logger.LogWarning("Archi Mission Listing -- Listings folder not found!\n" +
                                            "If this is the initial save load, you can safely ignore this.");
            return;
        }

        var missionFiles = listingsFolder.files;
        foreach (var missionFile in missionFiles)
        {
            AddMissionFromFile(missionFile);
        }
    }

    public override void draw(Rectangle bounds, SpriteBatch sb)
    {
        base.draw(bounds, sb);
        
        SideBarcode.Update(OS.currentInstance.lastGameTime.ElapsedGameTime.Seconds);
        DrawBarcode(bounds);

        if (!comp.PlayerHasAdminPermissions())
        {
            DrawUnauthenticatedView(bounds);
            return;
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (State)
        {
            case ArchipelagoMissionListingState.MissionListing:
            default:
                DrawMissionListing(bounds);
                break;
            case ArchipelagoMissionListingState.ViewMission:
                DrawViewMission(bounds);
                break;
        }
    }

    public const int BUTTON_HEIGHT = 35;
    public const int PANEL_MARGIN = 10;
    
    private static int LoginButtonId { get; } = PFButton.GetNextID();
    
    private static void DrawUnauthenticatedView(Rectangle bounds)
    {
        var login = Button.doButton(LoginButtonId,
            bounds.X + PANEL_MARGIN, bounds.Y + PANEL_MARGIN,
            bounds.Width / 5, BUTTON_HEIGHT, "Login",
            OS.currentInstance.brightLockedColor);

        if (login)
        {
            OS.currentInstance.runCommand("login");
        }
    }

    // ReSharper disable once InconsistentNaming
    public int ScrollPanelID { get; private set; } = PFButton.GetNextID();
    private float _scroll = 0f;

    private void DrawMissionListing(Rectangle bounds)
    {
        GuiData.spriteBatch.DrawString(GuiData.font, "Mission Listing",
            new Vector2(bounds.X + PANEL_MARGIN, bounds.Y + PANEL_MARGIN),
            Color.White);

        var titleSize = GuiData.font.MeasureString("Mission Listing");
        Rectangle listBounds = new(
            bounds.X + 10,
            bounds.Y + PANEL_MARGIN + (int)titleSize.Y + 10,
            bounds.Width - PANEL_MARGIN - (int)(bounds.Width * BARCODE_SIZE_MULTIPLIER),
            bounds.Height - (PANEL_MARGIN * 2) - (int)titleSize.Y - 10
        );

        var finalHeight = (Missions.Count * (BUTTON_HEIGHT + 5)) +
                          ((Missions.Count - 1) * PANEL_MARGIN);
        var needsScroll = finalHeight > listBounds.Height;
        var xOffset = bounds.X + PANEL_MARGIN;
        var yOffset = listBounds.Y;
        if (needsScroll)
        {
            var drawbounds = listBounds;
            drawbounds.Height = finalHeight;
            drawbounds.Width += drawbounds.Width / 10;
            
            ScrollablePanel.beginPanel(ScrollPanelID, drawbounds, new(0, _scroll));

            xOffset = 0;
            yOffset = 0;
        }

        foreach (var mission in Missions)
        {
            DrawMissionButton(mission, new Vector2(xOffset, yOffset), bounds);
            yOffset += BUTTON_HEIGHT + 5;
        }

        if (needsScroll)
        {
            var maxScroll = Math.Max(finalHeight, listBounds.Height - finalHeight);
            var scroll = ScrollablePanel.endPanel(ScrollPanelID,
                new Vector2(0, _scroll), listBounds, maxScroll);
            _scroll = scroll.Y;
        }
    }

    private void DrawMissionButton(MissionListingEntry missionListingEntry,
        Vector2 position, Rectangle bounds)
    {
        if (missionListingEntry.ButtonIndex == -1)
        {
            missionListingEntry.ButtonIndex = PFButton.GetNextID();
        }

        var buttonColor = missionListingEntry.CanBeClaimed ? Color.Orange : OS.currentInstance.lightGray;

        var buttonWidth = (bounds.Width / 2) - 10;
        var x = (int)(Math.Floor(position.X));
        var y = (int)(Math.Floor(position.Y));

        var pressed = Button.doButton(
            missionListingEntry.ButtonIndex, x, y, buttonWidth, BUTTON_HEIGHT,
            missionListingEntry.PostingTitle, buttonColor);

        if (!pressed) return;
        CurrentMission = missionListingEntry;
        State = ArchipelagoMissionListingState.ViewMission;
    }
    
    private int AcceptMissionButtonId { get; } = PFButton.GetNextID();
    private int GoBackToMissionListingButtonId { get; } = PFButton.GetNextID();

    private void DrawViewMission(Rectangle bounds)
    {
        GuiData.spriteBatch.DrawString(GuiData.font, CurrentMission.PostingTitle,
            new Vector2(bounds.X + PANEL_MARGIN, bounds.Y + PANEL_MARGIN),
            Color.White);
        var postingTitleSize = GuiData.font.MeasureString(CurrentMission.PostingTitle);

        var postingWidth = bounds.Width - PANEL_MARGIN - (int)(bounds.Width * BARCODE_SIZE_MULTIPLIER);

        var postingBody = Utils.SuperSmartTwimForWidth(CurrentMission.Mission.postingBody,
            postingWidth,
            GuiData.smallfont);
        GuiData.spriteBatch.DrawString(GuiData.smallfont, postingBody,
            new Vector2(bounds.X + PANEL_MARGIN, bounds.Y + PANEL_MARGIN + postingTitleSize.Y + 10),
            Color.White);

        if (!CurrentMission.CanBeClaimed)
        {
            GuiData.spriteBatch.DrawString(GuiData.smallfont,
                "Unavailable.",
                new Vector2(bounds.X + PANEL_MARGIN, bounds.Y + bounds.Height - (BUTTON_HEIGHT * 2) - PANEL_MARGIN),
                Color.Red);
        }
        else
        {
            var acceptMission = Button.doButton(AcceptMissionButtonId,
                bounds.X + PANEL_MARGIN, bounds.Y + bounds.Height - BUTTON_HEIGHT - PANEL_MARGIN,
                (postingWidth / 2) - 10, BUTTON_HEIGHT, "Accept", Color.Green);
            if (acceptMission)
            {
                ClaimMission(CurrentMission);
                return;
            }
        }
        
        var goBack = Button.doButton(GoBackToMissionListingButtonId,
            bounds.X + PANEL_MARGIN, bounds.Y + bounds.Height - BUTTON_HEIGHT - PANEL_MARGIN,
            (postingWidth / 2) - 10, BUTTON_HEIGHT, "Go Back", Color.Red);
        if (!goBack) return;
        State = ArchipelagoMissionListingState.MissionListing;
        CurrentMission = null;
    }

    private void DrawBarcode(Rectangle bounds)
    {
        SideBarcode.Draw(
            bounds.X + bounds.Width - (int)(bounds.Width * BARCODE_SIZE_MULTIPLIER),
            bounds.Y,
            (int)(bounds.Width * BARCODE_SIZE_MULTIPLIER),
            bounds.Height,
            GuiData.spriteBatch,
            OS.currentInstance.highlightColor
            );
    }
}

public class MissionListingEntry
{
    public ActiveMission Mission { get; set; }
    public FileEntry RelatedFile { get; set; }
    public bool CanBeClaimed { get; private set; } = false;
    public int ButtonIndex { get; set; } = -1;

    public string PostingTitle => Mission.postingTitle;

    public MissionListingEntry(ActiveMission mission)
    {
        Mission = mission;
    }

    public void RefreshCanBeClaimed()
    {
        var subject = Mission.email.subject;
        var hasItemsRequired = ArchipelagoLocations.HasItemsForLocation(subject);
        if (!HacknetAPCore.SlotData.EnableFactionAccess)
        {
            CanBeClaimed = hasItemsRequired;
        }

        var archiLocation = ArchipelagoLocations.MissionToLocation[subject];
        var isEntropy = archiLocation.StartsWith("Entropy") &&
                        !archiLocation.Contains("Welcome") &&
                        !archiLocation.Contains("Confirmation");
        var factionAccess = InventoryManager.FactionAccess;
        var shuffleLabs = HacknetAPCore.SlotData.ShuffleLabyrinths;
        var hasEnoughFactionAccess =
            (isEntropy && factionAccess >= FactionAccess.Entropy) ||
            (!isEntropy && shuffleLabs && factionAccess >= FactionAccess.CSEC) ||
            (!isEntropy && !shuffleLabs && factionAccess >= FactionAccess.LabyrinthsOrCSEC);
        CanBeClaimed = hasItemsRequired && hasEnoughFactionAccess;
    }
}