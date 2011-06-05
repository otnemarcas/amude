using System;
using System.Collections.Generic;
using Amude.Core;
using Amude.Domain;
using Amude.Domain.Attribute;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Network.Messages.Data;
using Amude.Global;
using Microsoft.Xna.Framework.Input;
using Amude.Screen.Core;

namespace Amude.Screen.Component.Game
{
    enum InfoBoxMode
    {
        CharacterDetails,
        CharacterList
    }

    internal class InfoBox
    {
        private const string ROOT_IMAGES = "Image/Menu/GameScreen/InfoBox/";
        public const int INFOBOX_START_X = 984;
        public const int INFOBOX_START_Y = 12;
        public const int INFOBOX_WIDTH = 284;
        public const int INFOBOX_HEIGHT = 776;

        private InfoBoxMode mode;
        private CharacterDetailsView characterDetails;
        private CharactersListView characterList;
        private IInfoBoxMode infoBoxMode;
        private Character selectedCharacter;

        private Texture2D background;
        private Vector2 backgroudPosition = new Vector2(0, 0);
        private Rectangle backgroundBounds;

        public Character SelectedCharacter
        {
            get
            {
                return selectedCharacter;
            }
            set
            {
                selectedCharacter = value;
                if (selectedCharacter != null)
                {
                    Mode = InfoBoxMode.CharacterDetails;
                    characterDetails.SelectedCharacter = value;
                }
                else
                {
                    Mode = InfoBoxMode.CharacterList;
                }
            }
        }

        private InfoBoxMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                switch (mode)
                {
                    case InfoBoxMode.CharacterDetails:
                        infoBoxMode = characterDetails;
                        break;
                    case InfoBoxMode.CharacterList:
                        infoBoxMode = characterList;
                        break;
                }
            }
        }

        public InfoBox()
        {
            characterDetails = new CharacterDetailsView();
            characterList = new CharactersListView();
            Mode = InfoBoxMode.CharacterList;
            background = IO.LoadSingleTexture(ROOT_IMAGES + "InfoBox");
            backgroundBounds = new Rectangle(INFOBOX_START_X, INFOBOX_START_Y, INFOBOX_WIDTH, INFOBOX_HEIGHT);
        }

        public void Update(float passedTime)
        {
            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.L))
            {
                if (Mode == InfoBoxMode.CharacterDetails)
                {
                    Mode = InfoBoxMode.CharacterList;
                }
                else if (Mode == InfoBoxMode.CharacterList && SelectedCharacter != null)
                {
                    Mode = InfoBoxMode.CharacterDetails;
                }
            }

            infoBoxMode.Update(passedTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundBounds, null, Color.White, 0, backgroudPosition,
                SpriteEffects.None, Constants.LD_INTERFACE_2);
            infoBoxMode.Draw(spriteBatch);
        }
    }

    interface IInfoBoxMode
    {
        void Update(float passedTime);
        void Draw(SpriteBatch spriteBatch);
    }

    internal sealed class CharacterDetailsView : IInfoBoxMode
    {
        private const string ROOT_IMAGES = "Image/Menu/GameScreen/InfoBox/";
        private const string EFFECTS_ICON_ROOT = ROOT_IMAGES + "Affects/";
        private const int LABELS_X = 1000;
        private const int VALUES_X = 1000;
        private const int INFORMATIONS_VERTICAL_DISTANCE = 60;
        private const int TITLE_X = 1000;
        private const int PROFILE_X = InfoBox.INFOBOX_START_X + ((InfoBox.INFOBOX_WIDTH / 2) - 48);

        private const int FRIENDLY_EFFECTS_ICON_X = 996;
        private const int EFFECTS_ICON_START_Y = 60;
        private const int UNFRIENDLY_EFFECTS_ICON_X = 1230;

        private Dictionary<String, Texture2D> affectsIcon;
        private Character selectedCharacter;
        private SpriteFont titleFont;
        private Vector2 titlePosition;
        private SpriteFont informationsFont;
        private Dictionary<String, Texture2D> profileTextures;
        private Vector2 profilePosition;
        private Color labelsColor;
        private Color informationsColor;
        private Color titleColor;
        private ProgressBar progressBar;

        public Character SelectedCharacter
        {
            get
            {
                return selectedCharacter;
            }
            set
            {
                selectedCharacter = value;
                progressBar = new HealthProgressBar(selectedCharacter.Health,
                    260, 30, new Vector2(1030, 75));
                progressBar.Value = selectedCharacter.Health.Value;
            }
        }

        public CharacterDetailsView()
        {
            affectsIcon = new Dictionary<String, Texture2D>();
            titleFont = IO.LoadFont("Data/Global/CharacterTitleFont");
            titlePosition = new Vector2(TITLE_X, 30);
            informationsFont = IO.LoadFont("Data/Global/CharacterInformationsFont");
            labelsColor = Color.White;
            informationsColor = Color.Yellow;
            titleColor = Color.White;
            profilePosition = new Vector2(PROFILE_X, 70);
            profileTextures = new Dictionary<string, Texture2D>();
            foreach (string key in Bundle.Characters.Keys)
            {
                profileTextures.Add(key, IO.LoadSingleTexture(AbstractScreen.CHARACTER_PROFILES_PATH + key + "-profile"));
            }

            foreach (SpecialAbility specialAbility in Bundle.SpecialAbilities.Keys)
            {
                affectsIcon.Add(specialAbility.RootName, IO.LoadSingleTexture(EFFECTS_ICON_ROOT + specialAbility.RootName));
            }
        }

        public void Update(float passedTime)
        {
            progressBar.Update(passedTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 friendlyAffectsPosition = new Vector2(FRIENDLY_EFFECTS_ICON_X, CharacterDetailsView.EFFECTS_ICON_START_Y);
            Vector2 unfriendlyAffectsPosition = new Vector2(UNFRIENDLY_EFFECTS_ICON_X, CharacterDetailsView.EFFECTS_ICON_START_Y);

            foreach (Affect affect in SelectedCharacter.Affects)
            {
                if (Bundle.GetSpecialAbility(affect.RootName).Friendly)
                {
                    spriteBatch.Draw(affectsIcon[affect.RootName], friendlyAffectsPosition, null,
                        Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
                    friendlyAffectsPosition.Y += 45;
                }
                else
                {
                    spriteBatch.Draw(affectsIcon[affect.RootName], unfriendlyAffectsPosition, null,
                        Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
                    unfriendlyAffectsPosition.Y += 45;
                }
            }

            int informationPosY = 220;

            spriteBatch.DrawString(titleFont, SelectedCharacter.Name, titlePosition, titleColor,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);

            spriteBatch.Draw(profileTextures[SelectedCharacter.RootName], profilePosition, null, Color.White,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);

            spriteBatch.DrawString(informationsFont, "Pontos de Vida:",
                new Vector2(LABELS_X, informationPosY), labelsColor, 0, Vector2.Zero, 1,
                SpriteEffects.None, Constants.LD_INTERFACE_3);

            informationPosY += 30;
            progressBar.Position = new Vector2(LABELS_X, informationPosY);
            progressBar.Draw(spriteBatch);

            informationPosY += 30;
            spriteBatch.DrawString(informationsFont, selectedCharacter.Health.ToString(),
                                   new Vector2(InfoBox.INFOBOX_START_X + (InfoBox.INFOBOX_WIDTH / 2f) -
                                       (informationsFont.MeasureString(selectedCharacter.Health.ToString()).X / 2), informationPosY),
                                   informationsColor, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
            informationPosY += 30;

            DrawInformation(spriteBatch, "Ataque:", SelectedCharacter.Attack.ToString(), informationPosY);
            informationPosY += INFORMATIONS_VERTICAL_DISTANCE;

            DrawInformation(spriteBatch, "Tipo de Ataque:",
                SelectedCharacter.Attack.Type == AttackType.MELEE ? "Corpo a Corpo" : "Longa Distância",
                informationPosY);
            informationPosY += INFORMATIONS_VERTICAL_DISTANCE;

            DrawInformation(spriteBatch, "Defesa:", SelectedCharacter.Defense.ToString(), informationPosY);
            informationPosY += INFORMATIONS_VERTICAL_DISTANCE;

            DrawInformation(spriteBatch, "Movimentação:", SelectedCharacter.Agility.ToString("0"),
                informationPosY);
            informationPosY += INFORMATIONS_VERTICAL_DISTANCE;

            DrawInformation(spriteBatch, "Iniciativa:", SelectedCharacter.Initiative.ToString("0"),
                informationPosY);
            informationPosY += INFORMATIONS_VERTICAL_DISTANCE;

            if (selectedCharacter.SpecialAbility != null)
            {
                DrawInformation(spriteBatch, "Habilidade:", selectedCharacter.SpecialAbility.Name,
                    informationPosY);
                informationPosY += INFORMATIONS_VERTICAL_DISTANCE;
            }
        }

        private void DrawInformation(SpriteBatch spriteBatch, String title, String value, int posY)
        {
            spriteBatch.DrawString(informationsFont, title, new Vector2(LABELS_X, posY), labelsColor,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);

            spriteBatch.DrawString(informationsFont, value,
                new Vector2(VALUES_X, posY + (INFORMATIONS_VERTICAL_DISTANCE / 2)), informationsColor,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
        }
    }

    internal sealed class CharactersListView : IInfoBoxMode
    {
        private const string ROOT_IMAGES = "Image/Menu/GameScreen/InfoBox/";
        private const float PROFILE_SCALE = 0.4f;
        private const int CHARACTERS_X = 1000;
        private const int LOCAL_PLAYER_CHARACTERS_Y = 60;
        private const int REMOTE_PLAYER_CHARACTERS_Y = 460;
        private const int CHARACTERS_VERTICAL_DISTANCE = 55;

        private SpriteFont titleFont;
        private SpriteFont charactersListFont;
        private Texture2D deadTexture;
        private Dictionary<String, Texture2D> profilesTexture;
        private Player localPlayer, remotePlayer;
        private Vector2 localPlayerNamePosition;
        private Vector2 remotePlayerNamePosition;
        private Color titleColor;
        private Color informationColor;
        private Color turnColor;
        private Dictionary<String, ProgressBar> progressBars;

        public CharactersListView()
        {
            titleFont = IO.LoadFont("Data/Global/CharacterTitleFont");
            charactersListFont = IO.LoadFont("Data/Global/CharacterListFont");
            deadTexture = IO.LoadSingleTexture(ROOT_IMAGES + "died");
            profilesTexture = new Dictionary<string, Texture2D>();

            localPlayer = Controller.GetInstance().GetLocalPlayer();
            remotePlayer = Controller.GetInstance().GetRemotePlayer();

            foreach (string key in Bundle.Characters.Keys)
            {
                profilesTexture.Add(key, IO.LoadSingleTexture(AbstractScreen.CHARACTER_PROFILES_PATH + key + "-profile"));
            }

            localPlayerNamePosition = new Vector2(1000, 20);
            remotePlayerNamePosition = new Vector2(1000, 420);

            titleColor = Color.White;
            informationColor = Color.Yellow;
            turnColor = Color.Red;

            progressBars = new Dictionary<string, ProgressBar>();
            foreach (Character character in localPlayer.GetCharacters())
            {
                progressBars.Add(localPlayer.Name + character.RootName, new HealthProgressBar(character.Health, 220, 10, Vector2.Zero));
            }

            foreach (Character character in remotePlayer.GetCharacters())
            {
                progressBars.Add(remotePlayer.Name + character.RootName, new HealthProgressBar(character.Health, 220, 10, Vector2.Zero));
            }
        }

        public void Update(float passedTime)
        {
            foreach (ProgressBar progressBar in progressBars.Values)
            {
                progressBar.Update(passedTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawInformations(spriteBatch, LOCAL_PLAYER_CHARACTERS_Y, localPlayer, localPlayerNamePosition);
            if (GameScreen.GetInstance().Mode != GameScreenMode.TacticsMode)
            {
                DrawInformations(spriteBatch, REMOTE_PLAYER_CHARACTERS_Y, remotePlayer, remotePlayerNamePosition);
            }
        }

        private void DrawInformations(SpriteBatch spriteBatch, int charactersY, Player player, Vector2 playerNamePosition)
        {
            Color color = informationColor;
            Turn actualTurn = GameScreen.GetInstance().ActualTurn;
            Vector2 charactersPosition = new Vector2(CHARACTERS_X, charactersY);
            Vector2 charactersInfoPosition = new Vector2(CHARACTERS_X + 40, charactersY);

            spriteBatch.DrawString(titleFont, player.Name, playerNamePosition, titleColor, 0, Vector2.Zero,
                1, SpriteEffects.None, Constants.LD_INTERFACE_3);

            foreach (Character character in player.GetCharacters())
            {
                spriteBatch.Draw(profilesTexture[character.RootName], charactersPosition, null,
                    Color.White, 0, Vector2.Zero, PROFILE_SCALE, SpriteEffects.None, Constants.LD_INTERFACE_3);

                if (character.Health.IsDead)
                {
                    spriteBatch.Draw(deadTexture, charactersPosition, null, Color.White, 0,
                        Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_4);
                }

                if (actualTurn != null &&
                    actualTurn.PlayerName == player.Name &&
                    actualTurn.CharacterName == character.RootName)
                {
                    color = turnColor;
                }
                else
                {
                    color = informationColor;
                }

                spriteBatch.DrawString(charactersListFont, character.Name, charactersInfoPosition, color,
                    0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);

                if (!progressBars.ContainsKey(player.Name + character.RootName))
                {
                    progressBars.Add(player.Name + character.RootName, new HealthProgressBar(character.Health, 220, 10, Vector2.Zero));
                }
                
                progressBars[player.Name + character.RootName].Position = new Vector2(CHARACTERS_X + 40, charactersY + 22);
                progressBars[player.Name + character.RootName].Draw(spriteBatch);

                spriteBatch.DrawString(charactersListFont, character.Health.ToString(),
                    new Vector2(CHARACTERS_X + 40, charactersY + 30), color, 0, Vector2.Zero,
                    1, SpriteEffects.None, Constants.LD_INTERFACE_3);

                charactersY += CHARACTERS_VERTICAL_DISTANCE;
                charactersPosition.Y += CHARACTERS_VERTICAL_DISTANCE;
                charactersInfoPosition.Y += CHARACTERS_VERTICAL_DISTANCE;
            }
        }
    }
}
