using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain;
using Amude.Graphics;
using Amude.Motion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Core;
using Microsoft.Xna.Framework.Content;
using Amude.Screen.Core.MenuItem;
using Amude.Global;

namespace Amude.Screen.Core
{
    internal class Pointer : AnimatedMovement
    {
        public const float POINTER_SPEED = 500f;
        public static string POINTER_PATH = AbstractScreen.MENU_ROOT_DIRECTORY + "pointer";

        protected List<IMenuItem> menuItems;
        private int selection;

        public Pointer(List<IMenuItem> menuItems)
            : base(new Animation(AnimationType.StaticRight),
            MovementProvider.GetDirectMovement(menuItems[0].Position, menuItems[0].Position, POINTER_SPEED))
        {
            Animation.IsCyclic = false;
            List<Texture2D> sprites = new List<Texture2D>() { IO.LoadSingleTexture(POINTER_PATH) };
            Animation.Sprites = sprites;
            Animation.LayerDepth = Constants.LD_INTERFACE_0;
            selection = 0;
            this.menuItems = menuItems;
            base.Start();
        }

        public override bool IsVisible
        {
            get
            {
                return base.IsVisible;
            }
            set
            {
                if (!value)
                {
                    foreach (IMenuItem item in menuItems)
                    {
                        item.Selected = false;
                    }
                }

                base.IsVisible = value;
            }
        }

        public virtual int Selection
        {
            get
            {
                return selection;
            }
            set
            {
                if (value < 0 || value >= menuItems.Count)
                {
                    return;
                }
                Movement.Origin = Movement.Position;
                Movement.Destiny = menuItems[value].Position;
                menuItems[selection].Selected = false;

                if (IsVisible)
                {
                    menuItems[value].Selected = true;
                }
                
                if (selection != value)
                {
                    AudioManager.PlaySound("menu_select");
                }

                selection = value;
                base.Start();
            }
        }
    }
}
