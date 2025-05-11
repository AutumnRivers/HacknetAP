using Hacknet;
using Hacknet.Gui;

using HarmonyLib;

using BepInEx;
using BepInEx.Hacknet;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

using System;
using System.IO;

using Pathfinder.Replacements;
using Pathfinder.Util;

using TextBox = HacknetArchipelago.Replacements.ArchipelagoTextBox;
using Pathfinder.GUI;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ArchipelagoMainMenu
    {
        static string archiURI = "archipelago.gg";
        static string archiPort = "38281";
        static string archiSlot = "";
        static string archiPassword = "";

        static bool isConnected = false;
        static bool hasError = false;
        static bool hasReadLoginFile = false;

        static Color archiLogoColor = Color.White;

        public static Texture2D archiLogo;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenu),nameof(MainMenu.DrawBackgroundAndTitle))]
        static void Prefix(MainMenu __instance)
        {
            if (File.Exists("./archipelago.txt") && !hasReadLoginFile)
            {
                hasReadLoginFile = true;

                string[] archiDetails = File.ReadAllLines("./archipelago.txt");

                for(int x = 0; x < archiDetails.Length; x++)
                {
                    switch(x)
                    {
                        case 0:
                            archiURI = archiDetails[x];
                            break;
                        case 1:
                            archiSlot = archiDetails[x];
                            break;
                        case 2:
                            archiPassword = archiDetails[x];
                            break;
                        default:
                            continue;
                    }
                }
            }

            var screenManager = __instance.screenManager;
            int rightOffset = 600;

            TextItem.doFontLabel(new Vector2(520, 147), "Archipelago Edition", GuiData.font, Color.Orange);

            if(!DLC1SessionUpgrader.HasDLC1Installed)
            {
                TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 100), "You don't have Labyrinths installed. Things might break.", GuiData.smallfont, Color.OrangeRed);
            }
            TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 75), "Extensions are disabled while the Archipelago mod is installed.", GuiData.smallfont, Color.Red);
            TextItem.doFontLabel(new Vector2(50, screenManager.GraphicsDevice.Viewport.Height - 50), "For support, please contact @ohanamatsumae in the Archipelago Discord in #future-game-planning.", GuiData.smallfont, Color.White);

            Rectangle logoRect = new Rectangle()
            {
                Width = 225,
                Height = 225,
                X = (screenManager.GraphicsDevice.Viewport.Width - rightOffset) + 40,
                Y = 185
            };

            if (archiLogo != null) { 
                GuiData.spriteBatch.Draw(archiLogo, logoRect, archiLogoColor * 0.3f);
            } else
            {
                GraphicsDevice userGraphics = GuiData.spriteBatch.GraphicsDevice;

                FileStream logoStream = File.OpenRead("./BepInEx/plugins/assets/aplogo.png");
                archiLogo = Texture2D.FromStream(userGraphics, logoStream);
                logoStream.Dispose();
            }

            TextItem.doLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 135), "Archipelago Options", Color.White);

            if(!isConnected)
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 200), "Archipelago URI:", GuiData.smallfont, Color.White);
                archiURI = TextBox.doTextBox(11111, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 220, 300, 1, archiURI, GuiData.smallfont);

                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 250), "Archipelago Slot Name:", GuiData.smallfont, Color.White);
                archiSlot = TextBox.doTextBox(11113, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 270, 300, 1, archiSlot, GuiData.smallfont);

                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 300), "Archipelago Room Pass:", GuiData.smallfont, Color.White);
                archiPassword = TextBox.doTextBox(11114, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 320, 300, 1, archiPassword, GuiData.smallfont);
            } else
            {
                TextItem.doLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 200), "Connected", Color.Green);
                TextItem.doSmallLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 230), $"Slot : {archiSlot}", Color.White);
                TextItem.doSmallLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 250),
                    $"URI : {archiURI}", Color.White);
            }

            bool connectButton = Button.doButton(11115, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 425, 250, 40,
                isConnected ? "Disconnect from Archipelago" : "Connect to Archipelago",
                isConnected ? Color.Red : Color.Orange);

            if(isConnected)
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Successfully connected to Archipelago.", GuiData.smallfont, Color.Green);
            } else if (hasError)
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Failed to connect to Archipelago.", GuiData.smallfont, Color.Red);
            } else
            {
                TextItem.doFontLabel(new Vector2(screenManager.GraphicsDevice.Viewport.Width - rightOffset, 475), "Waiting to connect...", GuiData.smallfont, Color.Orange);
            }

            if(connectButton && !isConnected)
            {
                if(archiURI == "" || archiSlot == "")
                {
                    
                    HacknetAPCore.Logger.LogWarning("You left either the Archipelago URI or Archipelago Slot field empty, don't do that.");
                } else
                {
                    LoginResult archiLogin = HacknetAPCore.ConnectToArchipelago(archiURI, archiSlot, archiPassword);

                    if(archiLogin.Successful)
                    {
                        Console.WriteLine("[Hacknet_Archipelago] Connected to the Archipelago session.");
                        archiLogoColor = Color.Green;
                        isConnected = true;
                        hasError = false;

                        hasReadLoginFile = true;

                        try
                        {
                            StreamWriter archiLoginFile = new StreamWriter("./archipelago.txt", false);
                            archiLoginFile.WriteLineAsync(archiURI).Wait();
                            archiLoginFile.WriteLineAsync(archiSlot).Wait();
                            archiLoginFile.WriteLineAsync(archiPassword).Wait();
                            archiLoginFile.FlushAsync().Wait();
                        } catch(Exception err)
                        {
                            HacknetAPCore.Logger.LogError("Failed to write to Archipelago login file:\n" +
                                err.ToString() + "\n" +
                                "Execution will continue, but the player will have to re-enter their login data " +
                                "when they next re-connect to Archipelago.");
                        }
                    } else
                    {
                        LoginFailure failure = (LoginFailure)archiLogin;

                        string errorMessage = $"Failed to connect to {archiURI}:{archiPort} as {archiSlot}:";

                        foreach (string error in failure.Errors)
                        {
                            errorMessage += $"\n    {error}";
                        }
                        foreach (ConnectionRefusedError error in failure.ErrorCodes)
                        {
                            errorMessage += $"\n    {error}";
                        }

                        HacknetAPCore.Logger.LogError(errorMessage);
                        hasError = true;
                        isConnected = false;
                    }
                }
            } else if(connectButton)
            {
                HacknetAPCore.DisconnectFromArchipelago();
                isConnected = false;
                archiLogoColor = Color.White;
            }

            bool skipBootTextCheckbox = CheckBox.doCheckBox(11116, screenManager.GraphicsDevice.Viewport.Width - rightOffset, 550,
                HacknetAPCore.SkipBootIntroText, Color.White, "Skip Boot Intro Text");
            HacknetAPCore.SkipBootIntroText = skipBootTextCheckbox;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenu),nameof(MainMenu.drawMainMenuButtons))]
        static bool MainMenuButtons_Prefix(MainMenu __instance)
        {
            if (isConnected) return true;

            Vector2 basePosition = new(180, 200);
            string notConnectedTitle = "Not Connected to Archipelago";
            string notConnected = "To start a new Hacknet session,\nyou must first connect to Archipelago.";

            TextItem.doLabel(basePosition, notConnectedTitle, Color.Red);
            var offset = GuiData.font.MeasureString(notConnectedTitle);

            TextItem.doSmallLabel(new Vector2(180, 200 + offset.Y), notConnected, Color.White);
            var smallOffset = GuiData.smallfont.MeasureString(notConnected);

            var buttonsOffset = 200 + offset.Y + 15 + smallOffset.Y + 50;

            Color buttonColor = new Color(124, 137, 149);
            bool settingsButton = Button.doButton(13373, 180, (int)buttonsOffset, 450, 50, "Settings", buttonColor);

            if(settingsButton)
            {
                __instance.ScreenManager.AddScreen(new OptionsMenu(), __instance.ScreenManager.controllingPlayer);
            }

            buttonsOffset += 60;
            bool exitButton = Button.doButton(133715, 180, (int)buttonsOffset, 450, 28, "Exit", Color.Red);

            if(exitButton)
            {
                MusicManager.stop();
                Game1.threadsExiting = true;
                Game1.getSingleton().Exit();
            }

            return false;
        }
    }
}
