using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amude.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Amude.Global
{
    internal static class SharedFunctions
    {

        public static List<String> LayoutString(String value, int charactersPerLine)
        {
            List<String> ret = new List<String>();
            String line = "";

            for (int i = 0; i < value.Length; i++)
            {
                line += value[i];

                if (i == value.Length - 1)
                {
                    ret.Add(line);
                    break;                    
                }

                if (value[i] == '\n')
                {
                    line.Remove(line.Length - 1);
                    ret.Add(line);
                    line = "";
                }

                if (line.Length == charactersPerLine)
                {
                    for (int j = line.Length -1; j > 0; j--)
                    {
                        if (line[j] == ' ')
                        {
                            break;
                        }
                        else
                        {
                            i--;
                            line = line.Remove(line.Length - 1);
                        }
                    }

                    ret.Add(line);
                    line = "";
                }
            }

            return ret;
        }

        public static List<String> LayoutString(String value, int width, SpriteFont font)
        {
            List<String> ret = new List<String>();
            String line = "";

            for (int i = 0; i < value.Length; i++)
            {
                line += value[i];

                if (i == value.Length - 1)
                {
                    ret.Add(line);
                    break;
                }

                if (value[i] == '\n')
                {
                    line.Remove(line.Length - 1);
                    ret.Add(line);
                    line = "";
                }

                if (font.MeasureString(line).X >= width)
                {
                    for (int j = line.Length - 1; j > 0; j--)
                    {
                        if (line[j] == ' ')
                        {
                            break;
                        }
                        else
                        {
                            i--;
                            line = line.Remove(line.Length - 1);
                        }
                    }

                    ret.Add(line);
                    line = "";
                }
            }

            return ret;
        }

        public static String GetText(Keys keys)
        {
            switch (keys)
            {
                case Keys.A:
                case Keys.B:
                case Keys.C:
                case Keys.D:
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.E:
                case Keys.F:
                case Keys.G:
                case Keys.H:
                case Keys.I:
                case Keys.J:
                case Keys.K:
                case Keys.L:
                case Keys.M:
                case Keys.N:
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.O:
                case Keys.P:
                case Keys.Q:
                case Keys.R:
                case Keys.S:
                case Keys.T:
                case Keys.U:
                case Keys.V:
                case Keys.W:
                case Keys.X:
                case Keys.Y:
                case Keys.Z:
                    return keys.ToString().Substring(keys.ToString().Length - 1, 1);

                case Keys.Space:
                    return " ";
                case Keys.Decimal:
                    return "#";
                case Keys.Divide:
                    return "/";
                case Keys.Multiply:
                    return "*";
                case Keys.OemPlus:
                    return "+";
                case Keys.OemComma:
                    return ",";
                case Keys.OemMinus:
                    return "-";
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemQuestion:
                    return "?";
                case Keys.OemTilde:
                    return "~";
                case Keys.OemOpenBrackets:
                    return "(";
                case Keys.OemPipe:
                    return "|";
                case Keys.OemCloseBrackets:
                    return ")";
                case Keys.OemQuotes:
                    return "'";

                default:
                    return "";
            }

        }

        public static float GetCharacterLayerDepth(int screenX, int screenY)
        {
            if (screenY < 0)
            {
                return Constants.LD_CHARACTER_L0;
            }
            else if (screenY > Camera.CAMERA_HEIGHT + 1) 
            {
                return Constants.LD_CHARACTER_L9;
            }
            FieldInfo value = (typeof(Constants)).GetField("LD_CHARACTER_L" + screenY);
            return (float) value.GetValue(null);
        }

        public static float GetEffectLayerDepth(int screenX, int screenY)
        {
            if (screenY < 0)
            {
                return Constants.LD_EFFECT_L0;
            }
            else if (screenY > Camera.CAMERA_HEIGHT - 1)
            {
                return Constants.LD_EFFECT_L9;
            }
            FieldInfo value = (typeof(Constants)).GetField("LD_EFFECT_L" + screenY);
            return (float)value.GetValue(null);
        }
    }
}
