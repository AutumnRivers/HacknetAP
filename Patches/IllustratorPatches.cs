using HarmonyLib;
using Hacknet;
using Hacknet.Gui;
using HacknetArchipelago;
using HacknetArchipelago.Managers;
using Microsoft.Xna.Framework;
using Pathfinder.GUI;

namespace HacknetAPClient.Patches;

[HarmonyPatch]
public class IllustratorPatches
{
    private static int _reconnectId = PFButton.GetNextID();
    private static int _mainMenuID = PFButton.GetNextID();
    private static bool _reconnectButtonActive = true;
    
    private static string _lostConnectionSub = "You can either attempt a reconnection, or return to the main menu.";
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(OS), "drawModules")]
    public static bool DrawImportantMessages(OS __instance)
    {
        if (ArchipelagoManager.IsConnected) return true;

        var screenSize = __instance.fullscreen;
        var screenWidth = screenSize.Width;
        var screenHeight = screenSize.Height;
        
        RenderedRectangle.doRectangle(0, 0,
            (int)screenWidth, (int)screenHeight, Color.Black);

        const string lostConnectionMsg = "Lost connection to Archipelago!";
        const string reconnectText = "Attempt Reconnect";
        const string attemptText = "Reconnecting...";
        const string mainMenuText = "Main Menu";
        const int buttonHeight = 35;

        if (HacknetAPCore.CachedConnectionDetails.Item1 == null)
        {
            _lostConnectionSub = "No connection details were saved! You must return to the main menu.";
            _reconnectButtonActive = false;
        }

        var titleSize = GuiData.font.MeasureString(lostConnectionMsg);
        var subSize = GuiData.smallfont.MeasureString(_lostConnectionSub);
        
        GuiData.spriteBatch.DrawString(GuiData.font, lostConnectionMsg,
            new Vector2((screenWidth / 2) - (titleSize.X / 2),
                (screenHeight / 2) - titleSize.Y - subSize.Y - 20),
            Color.LightPink);
        GuiData.spriteBatch.DrawString(GuiData.smallfont, _lostConnectionSub,
            new Vector2((screenWidth / 2) - (subSize.X / 2),
                (screenHeight / 2) - titleSize.Y - 10),
            Color.White);

        if (_reconnectButtonActive)
        {
            var reconnectButton = Button.doButton(_reconnectId,
                (screenWidth / 2) - (screenWidth / 4) - 5,
                (screenHeight / 2) + 10,
                screenWidth / 4, buttonHeight,
                reconnectText, Color.Orange);
            if (reconnectButton)
            {
                var conDetails = HacknetAPCore.CachedConnectionDetails;
                var conResults = ArchipelagoManager.ConnectToArchipelago(conDetails.Item1, conDetails.Item2, conDetails.Item3);

                if (conResults.Successful)
                {
                    ArchipelagoManager.IsConnected = true;
                }
                else
                {
                    _lostConnectionSub = "Failed to reconnect! Wait a minute, and then try again.";
                }
                return false;
            }
        }

        var mainMenuButton = Button.doButton(_mainMenuID,
            (screenWidth / 2) + 5,
            (screenHeight / 2) + 10,
            screenWidth / 4, buttonHeight,
            mainMenuText, Color.DarkRed);
        if (mainMenuButton)
        {
            __instance.quitGame(null, null);
        }

        return false;
    }
}