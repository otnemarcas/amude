using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Amude.Graphics;
using Amude.Global;
using Amude.Screen.Core;
using Microsoft.Xna.Framework.Content;
using Amude.Screen.Core.MenuItem;
using Amude.Core;

namespace Amude.Screen.Component.CharacterSelection
{
    internal sealed class CharacterPointer : Pointer
    {
        private Color colorModifier;
        private int modifier = 0;
        private bool increase;

        public CharacterPointer(List<IMenuItem> menuItems)
            : base(menuItems)
        {
            colorModifier = new Color(150, 10, 10, 128);
            increase = true;
            List<Texture2D> sprites = new List<Texture2D>() { IO.LoadSingleTexture(CharacterSelectionScreen.CHARACTERSELECTION_ROOT_DIRECTORY + "selection") };
            Animation.Sprites = sprites;
            Animation.LayerDepth = Constants.LD_BACKLAYER_2;
        }

        public override int Selection
        {
            get
            {
                return base.Selection;
            }
            set
            {
                base.Movement.Speed = (menuItems[base.Selection].Position - menuItems[value].Position).Length() * 5;
                base.Selection = value;
            }
        }

        public override void Update(float passedTime)
        {
            if (increase && modifier <= 250)
            {
                modifier += 2;
                if (modifier > 250)
                    increase = false;
            }
            else
            {
                modifier -= 2;
                if (modifier < 150)
                    increase = true;
            }

            colorModifier.R = (byte)modifier;

            Animation.ColorModifier = colorModifier;

            base.Update(passedTime);
        }
    }
}
