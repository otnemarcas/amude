using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;
using System;

namespace Amude.Core
{
    internal class KeyboardManager
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode); 

        private List<Keys> typedKeys;
        private List<Keys> pressedKeys;

        private const int KEY_TOGGLED = 0x1000;
        private const int KEY_PRESSED = 0xffff;
        private const int VK_SHIFT = 0x10;
        private const int VK_CAPITAL = 0x14;

        #region Manager Singleton

        private static KeyboardManager manager = new KeyboardManager();
        public static KeyboardManager Manager
        {
            get { return manager; }
        }

        private KeyboardManager()
        {
            typedKeys = new List<Keys>();
            pressedKeys = new List<Keys>();
        }

        #endregion

        public List<Keys> TypedKeys
        {
            get
            {
                return typedKeys;
            }
        }

        public bool IsCapsLock
        {
            get
            {
                return (((ushort)GetKeyState(VK_CAPITAL)) & KEY_PRESSED) != 0;
            }
        }

        public bool IsShift
        {
            get
            {
                return (((ushort)GetKeyState(VK_SHIFT)) & KEY_TOGGLED) != 0;
            }
        }

        public void Update()
        {
            Keys[] state = Keyboard.GetState().GetPressedKeys();

            
            typedKeys.Clear();
            typedKeys.AddRange(state.Except(pressedKeys));
            pressedKeys = state.ToList<Keys>();
        }
    }
}
