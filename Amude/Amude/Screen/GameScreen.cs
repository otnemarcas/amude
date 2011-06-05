/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using System;
using System.Collections.Generic;
using Amude.Core;
using Amude.Domain;
using Amude.Global;
using Amude.Network.Messages;
using Amude.Screen.Component.Game;
using Amude.Screen.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Screen.Component.Game.Handler;
using Amude.Network.Messages.Data;
using Amude.Domain.Attribute;
using System.Threading;


namespace Amude.Screen
{
    internal enum GameScreenMode
    {
        TacticsMode,
        BattleMode
    }

    internal class GameScreen : AbstractScreen
    {
        public const string ACTIONBOX_SET = "Posicionar";
        public const string ACTIONBOX_NEXT = "Próximo";
        public const string ACTIONBOX_READY = "Estou Pronto!";
        public const string ACTIONBOX_WALK = "Caminhar";
        public const string ACTIONBOX_ATTACK = "Atacar";
        public const string ACTIONBOX_WAIT = "Esperar";
        public const string ACTIONBOX_FINISH = "Terminar Turno";
        public const string ACTIONBOX_EXIT = "Sair";
        public const string ACTIONBOX_CONTINUE = "Voltar";

        public static string GAME_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "GameScreen/";

        private const string TACTICS_TITLE = "Modo Tática";
        private const string BATTLE_TITLE = "Modo Batalha";

        List<Player> players = Controller.GetInstance().GetPlayers();
        private Stack<IInputHandler> handlers;
        private MapEngine mapEngine;
        private Camera camera;
        private GameScreenMode mode;
        private TileMark[,] tileMarks;

        private Turn actualTurn;

        private ActionBox actionBox;
        private Texture2D actionBoxTexture;

        private InfoBox infoBox;
        private ChatBox chatBox;

        private MapCursor mapCursor;

        private SpriteFont titleFont;
        private SpriteFont messageFont;
        private SpriteFont actionFont;

        private Vector2 topEdgePosition;
        private Vector2 bottomEdgePosition;
        private Vector2 leftEdgePosition;
        private Vector2 midleEdgePosition;
        private Vector2 rightEdgePosition;
        private Texture2D hEdgeTexture;
        private Texture2D vEdgeTexture;

        private Vector2 tacticsTitlePosition;
        private Vector2 battleTitlePosition;
        private Color titleColor;
        private bool showTitle;
        private Character selectedCharacter;

        #region Singleton

        private static GameScreen instance;

        private GameScreen()
        {
            handlers = new Stack<IInputHandler>();
            mapCursor = new MapCursor();

            titleFont = IO.LoadFont("Data/Global/TitleFont");
            actionFont = IO.LoadFont("Data/Global/ActionFont");
            messageFont = IO.LoadFont("Data/Global/MessageFont");

            topEdgePosition = Vector2.Zero;
            bottomEdgePosition = new Vector2(0, 788);
            leftEdgePosition = new Vector2(0, 12);
            midleEdgePosition = new Vector2(980, 12);
            rightEdgePosition = new Vector2(1268, 12);

            hEdgeTexture = IO.LoadSingleTexture(GAME_ROOT_DIRECTORY + "horizontaledge");
            vEdgeTexture = IO.LoadSingleTexture(GAME_ROOT_DIRECTORY + "verticaledge");

            tacticsTitlePosition = new Vector2(183, 339);
            battleTitlePosition = new Vector2(137, 339);
            titleColor = new Color(0, 0, 0, 255);

            actionBoxTexture = IO.LoadSingleTexture(GAME_ROOT_DIRECTORY + "actionbox");
            actionBox = new ActionBox(actionFont, actionBoxTexture);
            infoBox = new InfoBox();
            chatBox = new ChatBox();
        }

        public static GameScreen GetInstance()
        {
            if (instance == null)
            {
                instance = new GameScreen();
            }

            return instance;
        }

        public static void ClearInstance()
        {
            if (instance != null)
            {
                instance.camera.Dispose();
                instance.mapEngine.Dispose();
            }
            instance = null;
        }

        #endregion

        public Stack<IInputHandler> Handlers
        {
            get
            {
                return handlers;
            }
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }
        }

        public MapEngine MapEngine
        {
            get
            {
                return mapEngine;
            }
        }

        public GameScreenMode Mode
        {
            get
            {
                return mode;
            }
        }

        public ActionBox ActionBox
        {
            get
            {
                return actionBox;
            }
        }

        public InfoBox InfoBox
        {
            get
            {
                return infoBox;
            }
        }

        public ChatBox ChatBox
        {
            get
            {
                return chatBox;
            }
        }

        public MapCursor MapCursor
        {
            get
            {
                return mapCursor;
            }
        }

        public TileMark[,] TileMarks
        {
            get
            {
                return tileMarks;
            }
        }

        public Turn ActualTurn
        {
            get
            {
                return actualTurn;
            }
            set
            {
                actualTurn = value;

                if (actualTurn == null)
                {
                    SelectedCharacter = null;
                }
                else if (actualTurn.PlayerName != Controller.GetInstance().GetLocalPlayer().Name) 
                {
                    SelectedCharacter = null;
                }
                else if (actualTurn.PlayerName == Controller.GetInstance().GetLocalPlayer().Name)
                {
                    SelectedCharacter = Controller.GetInstance().GetLocalPlayer().GetCharacter(actualTurn.CharacterName);
                }
            }
        }

        public Character SelectedCharacter 
        {
            get 
            { 
                return selectedCharacter;
            }
            set 
            { 
                selectedCharacter = value;
                infoBox.SelectedCharacter = value;
            }
        }

        public KeyValuePair<CharacterKey, Affect[]>[] Affects { get; set; }

        public override void LoadContent()
        {
            AudioManager.PlayMusicSequence(new List<string>() { "battle", "battle2" }, true);
            mapEngine = MapEngine.GetInstance();
            camera = Camera.GetInstance();
            camera.UpdateLocations = true;
            tileMarks = new TileMark[mapEngine.Map.MapWidth, mapEngine.Map.MapHeight];
            ChangeMode(GameScreenMode.TacticsMode);
        }

        public override void UnloadContent() { }

        public void ChangeMode(GameScreenMode mode)
        {
            ActionBox.ClearOptions();
            ClearTileMarks();
            this.mode = mode;
            showTitle = true;
            titleColor.A = 255;
            handlers.Clear();

            if (mode == GameScreenMode.TacticsMode)
            {
                handlers.Push(new TacticsInputHandler(this));
            }
            else if (mode == GameScreenMode.BattleMode)
            {
                handlers.Push(new BattleInputHandler(this));
            }
        }

        public void UpdateMarks(bool attacking)
        {
            ClearTileMarks();

            if (mode == GameScreenMode.TacticsMode)
            {
                if (Controller.GetInstance().IsServer)
                {
                    for (int y = 1; y < mapEngine.Map.MapHeight; y++)
                    {
                        for (int x = 0; x < Constants.TACTICS_MODE_WIDTH; x++)
                        {
                            if (mapEngine.Map.Objects[x, y] == null)
                            {
                                tileMarks[x, y] = new TileMark(new Point(x, y));
                            }
                        }
                    }
                }
                else
                {
                    for (int y = 1; y < mapEngine.Map.MapHeight; y++)
                    {
                        for (int x = MapEngine.Map.MapWidth - Constants.TACTICS_MODE_WIDTH; x < MapEngine.Map.MapWidth; x++)
                        {
                            if (mapEngine.Map.Objects[x, y] == null)
                            {
                                tileMarks[x, y] = new TileMark(new Point(x, y));
                            }
                        }
                    }
                }
            }
            else
            {
                if (SelectedCharacter != null)
                {
                    List<Point> points;

                    if (attacking)
                    {
                        points = mapEngine.GetAttackPositions(SelectedCharacter);
                    }
                    else
                    {
                        points = mapEngine.GetAvaiablePositions(SelectedCharacter);
                    }

                    foreach (Point markPosition in points)
                    {
                        tileMarks[markPosition.X, markPosition.Y] =
                            new TileMark(new Point(markPosition.X, markPosition.Y));
                    }
                }
            }
        }

        public void ClearTileMarks()
        {
            for (int y = 0; y < mapEngine.Map.MapHeight; y++)
            {
                for (int x = 0; x < mapEngine.Map.MapWidth; x++)
                {
                    tileMarks[x, y] = null;
                }
            }
        }

        public override void Update(float passedTime)
        {
            foreach (Player player in players)
            {
                player.Update(passedTime);
            }

            foreach (TileMark tileMark in tileMarks)
            {
                if (tileMark != null)
                {
                    tileMark.Update();
                }
            }

            foreach (Entity entity in mapEngine.Map.Objects)
            {
                if (entity != null)
                {
                    entity.Update(passedTime);
                }
            }

            if (handlers.Count == 0)
            {
                ScreenManager.GetInstance().RemoveLastScreen(true);
                AudioManager.PlaySound("menu_accept");
            }
            else
            {
                handlers.Peek().Update(passedTime);
            }

            mapEngine.Update();
            camera.Update(passedTime);
            mapCursor.Update(passedTime);

            if (showTitle)
            {
                titleColor.A--;
                if (titleColor.A <= 0)
                {
                    titleColor.A = 255;
                    showTitle = false;
                }
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(hEdgeTexture, topEdgePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_3);

            spriteBatch.Draw(hEdgeTexture, bottomEdgePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_3);

            spriteBatch.Draw(vEdgeTexture, leftEdgePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_3);

            spriteBatch.Draw(vEdgeTexture, midleEdgePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_3);

            spriteBatch.Draw(vEdgeTexture, rightEdgePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_3);

            if (showTitle)
            {
                if (mode == GameScreenMode.TacticsMode)
                {
                    spriteBatch.DrawString(titleFont, TACTICS_TITLE, tacticsTitlePosition, titleColor,
                        0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_0);
                }
                else
                {
                    spriteBatch.DrawString(titleFont, BATTLE_TITLE, battleTitlePosition, titleColor,
                        0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_0);
                }
            }

            actionBox.Draw(spriteBatch);
            mapCursor.Draw(spriteBatch);
            infoBox.Draw(spriteBatch);
            chatBox.Draw(spriteBatch);

            for (int x = camera.Location.X; x < camera.Location.X + Camera.CAMERA_WIDTH; x++)
            {
                for (int y = camera.Location.Y; y < camera.Location.Y + Camera.CAMERA_HEIGHT; y++)
                {
                    if (tileMarks[x, y] != null)
                    {
                        tileMarks[x, y].Draw(spriteBatch);
                    }
                }
            }

            List<Player> players = Controller.GetInstance().GetPlayers();
            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }

            camera.Draw(spriteBatch);

        }

        public void MoveCharacter(Character character, Point point)
        {
            GameScreen.GetInstance().MapCursor.IsVisible = false;

            Point cameraLocation = new Point((character.MapLocation.X + point.X) / 2,
                (character.MapLocation.Y + point.Y) / 2);
            Camera.GetInstance().FocusOn(cameraLocation);

            MapEngine.GetInstance().Move(character, point);
            Camera.GetInstance().Follow(character);
        }

        public void DoBattle(Character actor, Character target, float damage, bool counterAttack, float counterDamage)
        {            
            GameScreen.GetInstance().MapCursor.IsVisible = false;

            if (actor.Attack.Type == AttackType.MELEE)
            {
                Camera.GetInstance().FocusOn(actor.MapLocation);
            }
            else
            {
                Point cameraLocation = new Point((actor.MapLocation.X + target.MapLocation.X) / 2,
                                                (actor.MapLocation.Y + target.MapLocation.Y) / 2);
                Camera.GetInstance().FocusOn(cameraLocation);
            }

            actor.AttackCharacter(target, damage, counterAttack, counterDamage);

            Camera.GetInstance().Watch(actor);
            Camera.GetInstance().Watch(target);
        }

        public void DoAffects(Turn turn)
        {
            foreach (Player player in players)
            {
                foreach (Character character in player.GetCharacters())
                {
                    if (turn != null && player.Name == turn.PlayerName && 
                        character.RootName == turn.CharacterName)
                    {
                        character.DoAffects(true);
                    }
                    else
                    {
                        character.DoAffects(false);
                    }
                }
            }
        }
    }
}