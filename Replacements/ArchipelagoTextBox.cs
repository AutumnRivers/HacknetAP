using System;

using Hacknet;
using Hacknet.Localization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HacknetArchipelago.Replacements
{
    public static class ArchipelagoTextBox
    {
        public const float DELAY_BEFORE_KEY_REPEAT_START = 0.44f;

        public const float KEY_REPEAT_DELAY = 0.04f;

        public const int OUTLINE_WIDTH = 2;

        public static Keys lastHeldKey;

        public static float keyRepeatDelay = 0.44f;

        public static int LINE_HEIGHT = 25;

        public static int cursorPosition = 0;

        public static int textDrawOffsetPosition = 0;

        public static int FramesSelected = 0;

        public static bool MaskingText = false;

        public static bool BoxWasActivated = false;

        public static bool UpWasPresed = false;

        public static bool DownWasPresed = false;

        public static bool TabWasPresed = false;

        public static string doTextBox(int myID, int x, int y, int width, int lines, string str, SpriteFont font)
        {
            string text = str;
            if (font == null)
            {
                font = GuiData.smallfont;
            }

            BoxWasActivated = false;
            Rectangle tmpRect = GuiData.tmpRect;
            tmpRect.X = x;
            tmpRect.Y = y;
            tmpRect.Width = width;
            tmpRect.Height = lines * LINE_HEIGHT;
            if (tmpRect.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
            }
            else if (GuiData.hot == myID)
            {
                GuiData.hot = -1;
            }

            if (GuiData.mouseWasPressed())
            {
                if (GuiData.hot == myID)
                {
                    if (GuiData.active == myID)
                    {
                        int num = GuiData.mouse.X - x;
                        bool flag = false;
                        for (int i = 1; i <= str.Length; i++)
                        {
                            if (font.MeasureString(str.Substring(0, i)).X > (float)num)
                            {
                                cursorPosition = i - 1;
                                flag = true;
                                break;
                            }

                            if (!flag)
                            {
                                cursorPosition = str.Length;
                            }
                        }
                    }
                    else
                    {
                        GuiData.active = myID;
                        cursorPosition = str.Length;
                    }
                }
                else if (GuiData.active == myID)
                {
                    GuiData.active = -1;
                }
            }

            if (GuiData.active == myID)
            {
                GuiData.willBlockTextInput = true;
                text = getFilteredStringInput(text, GuiData.getKeyboadState(), GuiData.getLastKeyboadState());
                if (GuiData.getKeyboadState().IsKeyDown(Keys.Enter) && GuiData.getLastKeyboadState().IsKeyDown(Keys.Enter))
                {
                    BoxWasActivated = true;
                    GuiData.active = -1;
                }
            }

            FramesSelected++;
            tmpRect.X = x;
            tmpRect.Y = y;
            tmpRect.Width = width;
            tmpRect.Height = lines * LINE_HEIGHT;
            GuiData.spriteBatch.Draw(Utils.white, tmpRect, (GuiData.active == myID) ? Color.White : ((GuiData.hot == myID) ? GuiData.Default_Selected_Color : GuiData.Default_Dark_Background_Color));
            tmpRect.X += 2;
            tmpRect.Y += 2;
            tmpRect.Width -= 4;
            tmpRect.Height -= 4;
            GuiData.spriteBatch.Draw(Utils.white, tmpRect, GuiData.Default_Light_Backing_Color);
            float num2 = ((float)LINE_HEIGHT - font.MeasureString(text).Y) / 2f;
            GuiData.spriteBatch.DrawString(font, text, new Vector2(x + 2, (float)y + num2), Color.White);
            if (GuiData.active == myID)
            {
                tmpRect.X = (int)((float)x + font.MeasureString(text.Substring(0, cursorPosition)).X) + 3;
                tmpRect.Y = y + 2;
                tmpRect.Width = 1;
                tmpRect.Height = LINE_HEIGHT - 4;
                GuiData.spriteBatch.Draw(Utils.white, tmpRect, (FramesSelected % 60 < 40) ? Color.White : Color.Gray);
            }

            return text;
        }

        public static string getFilteredStringInput(string s, KeyboardState input, KeyboardState lastInput)
        {
            char[] filteredKeys = GuiData.getFilteredKeys();
            foreach (char c in filteredKeys)
            {
                string text = s.Substring(0, cursorPosition) + c;
                s = text + s.Substring(cursorPosition);
                cursorPosition++;
            }

            Keys[] pressedKeys = input.GetPressedKeys();
            if (pressedKeys.Length == 1 && lastInput.IsKeyDown(pressedKeys[0]))
            {
                if (pressedKeys[0] == lastHeldKey && IsSpecialKey(pressedKeys[0]))
                {
                    keyRepeatDelay -= GuiData.lastTimeStep;
                    if (keyRepeatDelay <= 0f)
                    {
                        s = forceHandleKeyPress(s, pressedKeys[0], input, lastInput);
                        keyRepeatDelay = 0.04f;
                    }
                }
                else
                {
                    lastHeldKey = pressedKeys[0];
                    keyRepeatDelay = 0.44f;
                }
            }
            else
            {
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    if (lastInput.IsKeyDown(pressedKeys[i]) || !IsSpecialKey(pressedKeys[i]))
                    {
                        continue;
                    }

                    switch (pressedKeys[i])
                    {
                        case Keys.Delete:
                            if (s.Length > 0 && cursorPosition < s.Length)
                            {
                                string text = s.Substring(0, cursorPosition);
                                s = text + s.Substring(cursorPosition + 1);
                            }

                            break;
                        case Keys.Back:
                        case Keys.OemClear:
                            if (s.Length > 0 && cursorPosition > 0)
                            {
                                string text = s.Substring(0, cursorPosition - 1);
                                s = text + s.Substring(cursorPosition);
                                cursorPosition--;
                            }

                            break;
                        case Keys.Left:
                            cursorPosition--;
                            if (cursorPosition < 0)
                            {
                                cursorPosition = 0;
                            }

                            break;
                        case Keys.Right:
                            cursorPosition++;
                            if (cursorPosition > s.Length)
                            {
                                cursorPosition = s.Length;
                            }

                            break;
                        case Keys.Home:
                            cursorPosition = 0;
                            break;
                        case Keys.End:
                            cursorPosition = (cursorPosition = s.Length);
                            break;
                        case Keys.Up:
                            UpWasPresed = true;
                            break;
                        case Keys.Down:
                            DownWasPresed = true;
                            break;
                        case Keys.Tab:
                            TabWasPresed = true;
                            break;
                    }
                }
            }

            return s;
        }

        public static string forceHandleKeyPress(string s, Keys key, KeyboardState input, KeyboardState lastInput)
        {
            switch (key)
            {
                case Keys.Back:
                case Keys.Delete:
                case Keys.OemClear:
                    if (s.Length > 0 && cursorPosition > 0)
                    {
                        string text2 = s.Substring(0, cursorPosition - 1);
                        s = text2 + s.Substring(cursorPosition);
                        cursorPosition--;
                    }

                    break;
                case Keys.Left:
                    cursorPosition--;
                    if (cursorPosition < 0)
                    {
                        cursorPosition = 0;
                    }

                    break;
                case Keys.Right:
                    cursorPosition++;
                    if (cursorPosition > s.Length)
                    {
                        cursorPosition = s.Length;
                    }

                    break;
                case Keys.Up:
                    UpWasPresed = true;
                    break;
                case Keys.Down:
                    DownWasPresed = true;
                    break;
                case Keys.Tab:
                    TabWasPresed = true;
                    break;
            }

            return s;
        }

        public static bool IsSpecialKey(Keys key)
        {
            if ((key >= Keys.A && key <= Keys.Z) || (key >= Keys.D0 && key <= Keys.D9) || key == Keys.Space || key == Keys.OemPeriod || key == Keys.OemComma || key == Keys.OemTilde || key == Keys.OemMinus || key == Keys.OemPipe || key == Keys.OemOpenBrackets || key == Keys.OemCloseBrackets || key == Keys.OemQuotes || key == Keys.OemQuestion || key == Keys.OemPlus || key == Keys.OemSemicolon)
            {
                return false;
            }

            return true;
        }
    }
}
