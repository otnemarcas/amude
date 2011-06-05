using System.Collections.Generic;
using Amude.Core;
using Amude.Domain;
using Amude.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Amude.Global;

/*
 * TODO: Esta classe não está genérica.
 * Ela encara por padrão que a modificações de cores é de preto para vermelho.
 * Refatorar esta classe de forma que ela possa receber os valores de cores de "Item selecionado" e "Item não selecionado".
 */

namespace Amude.Screen.Core.MenuItem
{
    internal class ImageMenuItem : Animation, IMenuItem
    {
        protected bool selected;
        protected int modifier = 0;
        protected bool increase;

        public string Name { get; set; }
        public string RootDirectory { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ModifyColor { get; set; }
        
        
        
        public ImageMenuItem(string name, string rootDirectory)
           : base(AnimationType.StaticRight)
        {
            Name = name;
            RootDirectory = rootDirectory;
            LoadContent();
            Selected = false;
            base.LayerDepth = Constants.LD_INFO;
            ModifyColor = true;
        }

        protected void LoadContent()
        {
            List<Texture2D> sprites = 
                new List<Texture2D>() { IO.LoadSingleTexture(RootDirectory + Name) };
            Width = sprites[0].Width;
            Height = sprites[0].Height;

            base.Sprites = sprites;
            base.Duration = Animation.INFINITY;
            IsCyclic = false;
            base.Start();
        }

        #region IMenuItem Members

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                modifier = 0;
                increase = true;
                if (selected && ModifyColor)
                    ColorModifier = Color.Red;
                else
                    ColorModifier = Color.White;
            }
        }

        public override void Update(float passedTime)
        {
            if (Selected && ModifyColor)
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

                Color color = ColorModifier;
                color.R = (byte)modifier;
                color.G = 0;
                color.B = 0;

                ColorModifier = color;
            }
            else
            {
                if (ModifyColor)
                {
                    ColorModifier = Color.Black;
                }
                else
                {
                    ColorModifier = Color.White;
                }
            }

            base.Update(passedTime);
        }

        #endregion
    }
}
