using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Amude.Core;
using Microsoft.Xna.Framework;
using Amude.Network.Messages;
using Amude.Domain;
using System.Threading;
using Amude.Screen.Core;

namespace Amude.Screen.Component.Game.Handler
{
    internal class BattleInputHandler : IInputHandler
    {
        private GameScreen gameScreen;
        private String selectedOption;
        private List<Character> watchedCharacters;
        private Trigger trigger;

        public BattleInputHandler(GameScreen gameScreen)
        {
            this.gameScreen = gameScreen;
            gameScreen.MapCursor.IsVisible = false;
            watchedCharacters = new List<Character>();
            trigger = Trigger.DoNothing;
        }

        #region AbstractHandler Members

        public void Update(float passedTime)
        {
            bool isReady = true;

            gameScreen.InfoBox.Update(passedTime);
            gameScreen.ChatBox.Update(passedTime);

            if (watchedCharacters.Count != 0)
            {
                foreach (Character watchedCharacter in watchedCharacters)
                {
                    if (!watchedCharacter.IsIdle() || 
                        watchedCharacter.HealthVariation.IsVisible() || 
                        watchedCharacter.DrawingAffects || 
                        watchedCharacter.Target != null)
                    {
                        isReady = false;
                        break;
                    }
                }

                if (isReady)
                {
                    if (trigger == Trigger.SendConfirmUpdate)
                    {
                        SendConfirmUpdateMessage(ConfirmationType.UpdateMessage);
                        gameScreen.ActualTurn = null;
                    }
                    else if (trigger == Trigger.SendConfirmAffects)
                    {
                        SendConfirmUpdateMessage(ConfirmationType.AffectsMessage);
                    }
                    else if(trigger == Trigger.TriggerActionBox)
                    {
                        FillOptions();
                    }

                    watchedCharacters.Clear();
                    trigger = Trigger.DoNothing;
                }
            }

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
                    HideSelectedCharacter();
                    selectedOption = gameScreen.ActionBox.SelectedValue;
                    switch (gameScreen.ActionBox.SelectedValue)
                    {
                        case GameScreen.ACTIONBOX_WALK:
                            HandleActionWalk();
                            break;
                        case GameScreen.ACTIONBOX_ATTACK:
                            HandleActionAttack();
                            break;
                        case GameScreen.ACTIONBOX_WAIT:
                            HandleActionNext();
                            break;
                        case GameScreen.ACTIONBOX_FINISH:
                            HandleActionNext();
                            break;
                        case GameScreen.ACTIONBOX_EXIT:
                            HandleActionExit();
                            break;
                        case GameScreen.ACTIONBOX_CONTINUE:
                            HandleActionContinue();
                            break;
                    }
                }
            }
            else
            {
                gameScreen.MapCursor.IsVisible = isReady;

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
                {
                    if (gameScreen.SelectedCharacter != null)
                    {
                        gameScreen.ClearTileMarks();
                        gameScreen.MapCursor.IsVisible = false;
                        gameScreen.Camera.FocusOn(gameScreen.SelectedCharacter.MapLocation);
                    }
                    else
                    {                        
                        gameScreen.ActionBox.ClearOptions();
                        gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_EXIT);
                        gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_CONTINUE);                    
                    }

                    FillOptions();
                }

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
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

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
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

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.X++;
                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
                {
                    Point location = gameScreen.MapCursor.MapLocation;

                    if (gameScreen.TileMarks[location.X, location.Y] != null)
                    {
                        gameScreen.TileMarks[location.X, location.Y].Selected = false;
                    }

                    location.X--;
                    gameScreen.MapCursor.MapLocation = location;

                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y].Selected = true;
                    }

                    gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
                }

                if (isReady && KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
                {
                    if (gameScreen.TileMarks[gameScreen.MapCursor.MapLocation.X,
                        gameScreen.MapCursor.MapLocation.Y] != null)
                    {
                        AbstractMessage message = null;
                        Point target = gameScreen.MapCursor.MapLocation;

                        switch (selectedOption)
                        {
                            case GameScreen.ACTIONBOX_ATTACK:
                                message = new ActionMessageAttack();
                                ((ActionMessageAttack)message).TargetPlayer = Controller.GetInstance().GetRemotePlayer().Name;
                                ((ActionMessageAttack)message).Target =
                                    gameScreen.MapEngine.Map.Objects[target.X, target.Y].RootName;
                                break;

                            case GameScreen.ACTIONBOX_WALK:
                                message = new ActionMessageWalk();
                                ((ActionMessageWalk)message).Point = target;
                                break;
                        }

                        gameScreen.ClearTileMarks();

                        Controller.GetInstance().Client.SendData(message);
                    }
                }
            }
        }

        public void FillOptions()
        {
            if (gameScreen.ActualTurn != null &&
            gameScreen.ActualTurn.PlayerName == Controller.GetInstance().GetLocalPlayer().Name)
            {
                ShowSelectedCharacter();
                bool hasWalked = gameScreen.ActualTurn.HasWalked;

                gameScreen.ActionBox.ClearOptions();
                if (hasWalked)
                {
                    if (MapEngine.GetInstance().GetAttackPositions(gameScreen.SelectedCharacter).Count > 0)
                    {
                        gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_ATTACK);
                    }
                    gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_FINISH);
                }
                else
                {
                    if (MapEngine.GetInstance().GetAvaiablePositions(gameScreen.SelectedCharacter).Count > 0)
                    {
                        gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_WALK);
                    }
                    if (MapEngine.GetInstance().GetAttackPositions(gameScreen.SelectedCharacter).Count > 0)
                    {
                        gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_ATTACK);
                    }
                    gameScreen.ActionBox.AddOption(GameScreen.ACTIONBOX_WAIT);
                }
            }
        }

        public void Watch(Character character)
        {
            watchedCharacters.Add(character);
        }

        public void SetTrigger(Trigger trigger)
        {
            this.trigger = trigger;
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

        private void HandleActionAttack()
        {
            gameScreen.ActionBox.ClearOptions();
            gameScreen.UpdateMarks(true);
            gameScreen.MapCursor.MapLocation = gameScreen.SelectedCharacter.MapLocation;
            gameScreen.Camera.FocusOn(gameScreen.MapCursor.MapLocation);
            gameScreen.MapCursor.IsVisible = true;
        }

        private void HandleActionNext()
        {
            ActionMessageFinish message = new ActionMessageFinish();

            gameScreen.ActionBox.ClearOptions();
            gameScreen.ActualTurn = null;
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
        
        private void ShowSelectedCharacter()
        {
            if (gameScreen.SelectedCharacter != null)
            {
                Point position = gameScreen.SelectedCharacter.MapLocation;
                gameScreen.TileMarks[position.X, position.Y] = new TileMark(position) { Selected = true };
            }
        }

        private void HideSelectedCharacter()
        {
            if (gameScreen.SelectedCharacter != null)
            {
                Point position = gameScreen.SelectedCharacter.MapLocation;
                gameScreen.TileMarks[position.X, position.Y] = null;
            }
        }

        private void SendConfirmUpdateMessage(ConfirmationType confirmationType)
        {
            ConfirmUpdateMessage message = new ConfirmUpdateMessage();
            message.ConfirmationType = confirmationType;
            Controller.GetInstance().Client.SendData(message);
        }
    }
}
