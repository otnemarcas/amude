using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Core;
using Microsoft.Xna.Framework.Input;
using Amude.Global;
using Microsoft.Xna.Framework;
using Amude.Network.Messages;
using Amude.Domain;
using Amude.Graphics;
using System.Threading;

namespace Amude.Screen.Component.Game.Handler
{
    internal class TacticsInputHandler : IInputHandler
    {
        private GameScreen gameScreen;
        private int selectedCharacter;

        public TacticsInputHandler(GameScreen gameScreen)
        {
            this.gameScreen = gameScreen;
            this.FillOptions();
            selectedCharacter = 0;
            Character selected = Controller.GetInstance().GetLocalPlayer().GetCharacters()[selectedCharacter];
            gameScreen.SelectedCharacter = selected;
            gameScreen.TileMarks[selected.MapLocation.X, selected.MapLocation.Y] =
                new TileMark(selected.MapLocation) { Selected = true };
        }

        #region AbstractHandler Members

        public void Update(float passedTime)
        {
            gameScreen.InfoBox.Update(passedTime);
            gameScreen.ChatBox.Update(passedTime);

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Space))
            {
                ChatInputHandler chatInputHandler = new ChatInputHandler(gameScreen);
                gameScreen.ChatBox.ChatInputHandler = chatInputHandler;
                gameScreen.Handlers.Push(chatInputHandler);
                gameScreen.ChatBox.Show();
                return;
            }

            if (gameScreen.ActionBox.IsVisible)
            {
                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
                {
                    gameScreen.ActionBox.ClearOptions();
                    gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_CONTINUE);
                    gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_EXIT);
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
                {
                    gameScreen.ActionBox.Selection++;
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
                {
                    gameScreen.ActionBox.Selection--;
                }
                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
                {
                    switch (gameScreen.ActionBox.SelectedValue)
                    {
                        case GameScreen.ACTIONBOX_SET:
                            HandleActionWalk();
                            break;
                        case GameScreen.ACTIONBOX_NEXT:
                            HandleActionNext();
                            break;
                        case GameScreen.ACTIONBOX_READY:
                            HandleActionFinish();
                            break;
                        case GameScreen.ACTIONBOX_EXIT:
                            HandleActionExit();
                            break;
                        case GameScreen.ACTIONBOX_CONTINUE:
                            HandleActionContinue();
                            break;
                    }
                    gameScreen.ActionBox.Selection = 0;
                }
            }
            else
            {
                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
                {
                    gameScreen.ClearTileMarks();
                    gameScreen.MapCursor.IsVisible = false;
                    gameScreen.Camera.FocusOn(gameScreen.SelectedCharacter.MapLocation);
                    this.FillOptions();
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.Y--;
                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.Y++;
                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.X++;
                    if (location.X >= Constants.TACTICS_MODE_WIDTH && Controller.GetInstance().IsServer)
                    {
                        location.X = Constants.TACTICS_MODE_WIDTH - 1;
                    }

                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.X--;
                    if (location.X < gameScreen.MapEngine.Map.MapWidth - Constants.TACTICS_MODE_WIDTH && 
                        !Controller.GetInstance().IsServer)
                    {
                        location.X = gameScreen.MapEngine.Map.MapWidth - Constants.TACTICS_MODE_WIDTH;
                    }

                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
                {
                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {                        
                        gameScreen.MapEngine.RealocateObject(gameScreen.SelectedCharacter, gameScreen.MapCursor.MapLocation);
                        gameScreen.Camera.FocusOn(gameScreen.SelectedCharacter.MapLocation);                        
                        gameScreen.ClearTileMarks();
                        gameScreen.MapCursor.IsVisible = false;
                        FillOptions();
                    }
                }
            }
        }

        public void FillOptions()
        {
            gameScreen.ActionBox.ClearOptions();
            gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_SET);
            gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_NEXT);
            gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_READY);
        }

        public void Watch(Character character)
        {
            // Os personagens não andam, são movidos automaticamente, atualização automática.
        }

        public void SetTrigger(Trigger trigger) 
        {
            // Os personagens não andam, são movidos automaticamente, atualização automática.
        }

        #endregion

        private void HandleActionWalk()
        {
            gameScreen.ActionBox.ClearOptions();
            gameScreen.UpdateMarks(false);
            gameScreen.MapCursor.MapLocation = gameScreen.SelectedCharacter.MapLocation;
            gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
            gameScreen.MapCursor.IsVisible = true;
        }

        private void HandleActionNext()
        {
            selectedCharacter++;
            selectedCharacter %= Constants.MAX_CHARACTERS;

            gameScreen.TileMarks[gameScreen.SelectedCharacter.MapLocation.X,
                gameScreen.SelectedCharacter.MapLocation.Y] = null;
            Character selected = Controller.GetInstance().GetLocalPlayer().GetCharacters()[selectedCharacter];
            gameScreen.SelectedCharacter = selected;
            gameScreen.TileMarks[selected.MapLocation.X, selected.MapLocation.Y] =
                new TileMark(selected.MapLocation) { Selected = true };
            gameScreen.Camera.FocusOn(selected.MapLocation);
        }

        private void HandleActionFinish()
        {
            gameScreen.SelectedCharacter = null;
            gameScreen.ChangeMode(GameScreenMode.BattleMode);

            CharacterPositionMessage message = new CharacterPositionMessage();
            message.PlayerName = Controller.GetInstance().GetLocalPlayer().Name;
            foreach (Character character in Controller.GetInstance().GetLocalPlayer().GetCharacters())
            {
                message.AddCharacter(character.RootName, character.MapLocation);
            }

            Controller.GetInstance().Client.SendData(message);
        }

        private void HandleActionExit()
        {
            Thread exitingThread = new Thread(new ThreadStart(DoExit));
            exitingThread.Start();
        }

        private void DoExit()
        {
            Controller.GetInstance().SupressUpdate();

            Controller.GetInstance().QuitGame(true, true);

            Controller.GetInstance().ReleaseUpdate();
        }

        private void HandleActionContinue()
        {
            gameScreen.ActionBox.ClearOptions();
            FillOptions();
        }

    }
}
