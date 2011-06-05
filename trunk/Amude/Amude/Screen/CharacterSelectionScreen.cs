using System;
using System.Collections.Generic;
/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using Amude.Core;
using Amude.Domain;
using Amude.Global;
using Amude.Graphics;
using Amude.Network.Messages;
using Amude.Screen.Component.CharacterSelection;
using Amude.Screen.Core;
using Amude.Screen.Core.MenuItem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Amude.Screen
{
    internal class CharacterSelectionScreen : AbstractScreen
    {
        public static string CHARACTERSELECTION_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "CharacterSelectionScreen/";

        private const int DEFAULT_CHARACTER_X = 170;
        private const int DEFAULT_CHARACTER_Y = 350;
        private const int DEFAULT_CHARACTER_INTERVAL_X = 100;
        private const int DEFAULT_CHARACTER_INTERVAL_Y = 150;

        private const int DEFAULT_DESCRIPTION_X = 700;
        private const int DEFAULT_DESCRIPTION_Y = 270;
        private const int DEFAULT_LINEWIDTH = 23;

        private const int DEFAULT_PROFILE_X = 845;
        private const int DEFAULT_PROFILE_Y = 150;

        private const int DEFAULT_MENUITEM_X = 380;
        private const int DEFAULT_MENUITEM_Y = 650;
        private const int DEFAULT_MENUITEM_SPACING = 230;

        private Texture2D containerTexture;
        private Vector2 containerPosition;

        private Texture2D selectedTexture;

        private List<Character>  characters;
        private List<Texture2D> characterProfiles;
        private Vector2 characterProfilesOrigin;
        private Vector2 profileTexturePosition;

        private SpriteFont spriteFont;
        private Vector2 titlePlayerPosition;
        private Vector2 netPlayerPosition;

        private List<String> selectedCharacters;
        List<IMenuItem> characterItems;
        private List<IMenuItem> menuItems;

        private int selection;
        private Pointer pointer;
        private CharacterPointer characterPointer;

        public CharacterSelectionScreen()
        {
            spriteFont = IO.LoadFont("Data/Global/AmudeFont");
            selectedCharacters = new List<String>();

            containerTexture = IO.LoadSingleTexture(CHARACTERSELECTION_ROOT_DIRECTORY + "container");
            containerPosition = new Vector2(100, 50);

            selectedTexture = IO.LoadSingleTexture(CHARACTERSELECTION_ROOT_DIRECTORY + "selected");

            profileTexturePosition = new Vector2(DEFAULT_PROFILE_X, DEFAULT_PROFILE_Y);
            titlePlayerPosition = new Vector2(170, 527);
            netPlayerPosition = new Vector2(350, 527);

            characterItems = new List<IMenuItem>();
            characters = new List<Character>();
            characterProfiles = new List<Texture2D>();
            int i = 0;
            foreach (string key in Bundle.Characters.Keys)
            {
                characters.Add(Bundle.Characters[key]);
                
                characterItems.Add(new ImageMenuItem(key + "-profile", AbstractScreen.CHARACTER_PROFILES_PATH) { 
                    Position = GetProfilePosition(i++), ModifyColor = false
                });

                characterProfiles.Add(IO.LoadSingleTexture(AbstractScreen.CHARACTER_PROFILES_PATH + key + "-profile"));
            }

            characterProfilesOrigin = new Vector2(0, characterProfiles[0].Height);

            characterPointer = new CharacterPointer(characterItems);

            menuItems = new List<IMenuItem>();
            menuItems.Add(new ImageMenuItem("imready", CHARACTERSELECTION_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("cancel", CHARACTERSELECTION_ROOT_DIRECTORY));

            for (i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].Position = new Vector2(DEFAULT_MENUITEM_X + (DEFAULT_MENUITEM_SPACING) * i,
                                                    DEFAULT_MENUITEM_Y);
            }

            pointer = new Pointer(menuItems);
            LoadContent();
        }

        private int Selection
        {
            get
            {
                return selection;
            }
            set
            {
                if (value < 10)
                {
                    pointer.IsVisible = false;
                    characterPointer.IsVisible = true;
                    characterPointer.Selection = value;
                }
                else if (value == 10)
                {
                    characterPointer.IsVisible = false;
                    pointer.IsVisible = true;
                    if (selectedCharacters.Count == Constants.MAX_CHARACTERS)
                    {
                        pointer.Selection = 0;
                    }
                    else
                    {
                        value++;
                        pointer.Selection = 1;    
                    }
                    
                }
                else if (value == 11)
                {
                    characterPointer.IsVisible = false;
                    pointer.IsVisible = true;
                    pointer.Selection = 1;
                }

                selection = value;
            }
        }

        public override void LoadContent()
        {
            AudioManager.PlayMusic("getReady", true);
            pointer.Selection = 0;
            pointer.IsVisible = false;

            characterPointer.Selection = 0;
            characterPointer.IsVisible = true;

            selection = 0;
        }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            characterPointer.Update(passedTime);
            pointer.Update(passedTime);

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                Controller.GetInstance().StopClient();
                Controller.GetInstance().StopServer();

                ScreenManager.GetInstance().RemoveLastScreen(true);
                AudioManager.PlaySound("menu_accept");
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                if (Selection < 10)
                {
                    if (selectedCharacters.Contains(characters[Selection].RootName))
                    {
                        selectedCharacters.Remove(characters[Selection].RootName);
                        AudioManager.PlaySound("menu_select");
                    }
                    else if (selectedCharacters.Count < Constants.MAX_CHARACTERS)
                    {
                        selectedCharacters.Add(characters[Selection].RootName);
                        AudioManager.PlaySound("menu_select");
                    }
                }

                else if (selectedCharacters.Count == Constants.MAX_CHARACTERS && Selection == 10)
                {
                    CharacterSelectionMessage message = new CharacterSelectionMessage();
                    Player localPlayer = Controller.GetInstance().GetLocalPlayer();

                    localPlayer.ClearCharacters();
                    foreach (String characterName in selectedCharacters)
                    {
                        if (!Controller.GetInstance().IsServer)
                        {
                            localPlayer.AddCharacter(Bundle.Characters[characterName]);
                        }

                        message.SelectedCharacters.Add(characterName);
                    }

                    Controller.GetInstance().Client.SendData(message);

                    AudioManager.PlaySound("menu_accept");
                }

                else if (Selection == 11)
                {
                    Controller.GetInstance().QuitGame(true, true);
                    Controller.GetInstance().StopServer();
                    Controller.GetInstance().StopClient();
                    AudioManager.PlaySound("menu_accept");
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
            {
                if (selection < 5)
                {
                    Selection += 5;
                }
                else if (selection < 10)
                {
                    if (selectedCharacters.Count == Constants.MAX_CHARACTERS)
                    {
                        Selection = 10;
                    }
                    else
                    {
                        Selection = 11;
                    }
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
            {
                if (selection >= 10)
                {
                    Selection = characterPointer.Selection;
                }
                else if (selection >= 5)
                {
                    Selection -= 5;
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
            {
                if (selection > 0 && selection < 10)
                {
                    Selection--;
                }
                else if (selection == 0)
                {
                    Selection = 9;
                }
                else if (selection == 11 && selectedCharacters.Count == Constants.MAX_CHARACTERS)
                {
                    Selection = 10;
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
            {
                if (selection < 9)
                {
                    Selection++;
                }
                else if (selection == 9)
                {
                    Selection = 0;
                }
                else
                {
                    Selection = 11;
                }
            }

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

            foreach (IMenuItem characterImage in characterItems)
            {
                characterImage.Update(passedTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            // Informações
            if (characterProfiles != null)
                spriteBatch.Draw(characterProfiles[characterPointer.Selection], profileTexturePosition, 
                    null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            
            List<String> description = SharedFunctions.LayoutString(characters[characterPointer.Selection].Name + ": " + 
                characters[characterPointer.Selection].Description, DEFAULT_LINEWIDTH);
            for (int i = 0; i < description.Count; i++)
            {
                spriteBatch.DrawString(spriteFont, description[i], new Vector2(DEFAULT_DESCRIPTION_X, DEFAULT_DESCRIPTION_Y + (i * 30)),
                    Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            }

            spriteBatch.DrawString(spriteFont, "Oponente:", titlePlayerPosition, Color.Black, 
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.DrawString(spriteFont, Controller.GetInstance().GetRemotePlayer().Name,
                netPlayerPosition, Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            // Personagens selecionados
            for (int i = 0; i < characters.Count; i++)
            {
                if (selectedCharacters.Contains(characters[i].RootName))
                {
                    spriteBatch.Draw(selectedTexture, characterItems[i].Position, null, Color.White, 0,
                        characterProfilesOrigin, 1, SpriteEffects.None, Constants.LD_INTERFACE_0);
                }
            }

            // Personagens
            foreach (IMenuItem characterImage in characterItems)
            {
                characterImage.Draw(spriteBatch);
            }

            // Opções de menu
            if (selectedCharacters.Count == Constants.MAX_CHARACTERS)
            {
                menuItems[0].Draw(spriteBatch);
            }
            menuItems[1].Draw(spriteBatch);

            pointer.Draw(spriteBatch);
            characterPointer.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        private Vector2 GetProfilePosition(int index)
        {
            int x = (index % 5);
            int y = index < 5 ? 0 : 1;

            return new Vector2(DEFAULT_CHARACTER_X + (x * DEFAULT_CHARACTER_INTERVAL_X),
                               DEFAULT_CHARACTER_Y + (y * DEFAULT_CHARACTER_INTERVAL_Y));

        }
    }
}