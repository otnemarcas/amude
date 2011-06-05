/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Outubro/2010
 */

using System;
using System.Net;
using Amude.Domain;
using Amude.Graphics;
using Amude.Network.Messages;
using Amude.Screen;
using Amude.Screen.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Amude.Global;
using Amude.Domain.Attribute;
using Amude.Screen.Component.Game.Handler;
using Amude.Network.Messages.Data;

namespace Amude.Core.MessageProcessors
{
    internal class ClientMessageProcessor
    {
        enum ClientStatus
        {
            WaitingApproval,
            WaitingPlayers,
            CharacterSelection,
            WaitingCharacterSelection,
            Game
        }

        ClientStatus clientStatus;
        Controller controller;

        public ClientMessageProcessor()
        {
            controller = Controller.GetInstance();
            clientStatus = ClientStatus.WaitingApproval;
        }

        public void DataArrival(String address, AbstractMessage message)
        {
            if (message is SelectServerMessage)
            {
                HandleSelectServerMessage(address, message);
            }
            else if (message is CharacterSelectionMessage)
            {
                HandleCharacterSelectionMessage(address, message);
            }
            else if (message is WaitingOponentMessage)
            {
                HandleWaitingOponentMessage(address, message);
            }
            else if (message is StartGameMessage)
            {
                HandleStartGameMessage(address, message);
            }
            else if (message is CharacterPositionMessage)
            {
                HandleCharacterPositionMessage(address, message);
            }
            else if (message is StartBattleMessage)
            {
                HandleStartBattleMessage(address, message);
            }
            else if (message is UpdateMessage)
            {
                HandleUpdateMessage(address, message);
            }
            else if (message is TurnMessage)
            {
                HandleTurnMessage(address, message);
            }
            else if (message is AffectsMessage)
            {
                HandleAffectsMessage(address, message);
            }
            else if (message is ChatMessage)
            {
                HandleChatMessage(address, message);
            }
            else if(message is EndGameMessage)
            {
                HandleEndGameMessage(address, message);
            }
            else if (message is PingMessage)
            {
                HandlePingMessage(address, message);
            }
            else if (message is InvalidGuidMessage)
            {
                HandleInvalidGuidMessage(address, message);
            }
            else if (message is LeaveMessage)
            {
                HandleLeaveMessage(address, message);
            }
        }

        public void InitializeLocalPlayer()
        {

            controller.Client.SetServerAddress(IPAddress.Loopback);

            HandleSelectServerMessage(IPAddress.Loopback.ToString(), new SelectServerMessage() { Success = true });
        }

        private void HandleSelectServerMessage(String address, AbstractMessage message)
        {
            if (clientStatus == ClientStatus.WaitingApproval)
            {
                SelectServerMessage selectServerMessage = (SelectServerMessage)message;

                if (selectServerMessage.Success)
                {
                    if (!controller.IsServer)
                    {
                        controller.GetLocalPlayer().Name = selectServerMessage.PlayerName;

                        Player player = new Player(IPAddress.Parse(address));
                        player.Name = selectServerMessage.RemotePlayerName;
                        controller.AddPlayer(player.IPAddress, player);
                    }

                    clientStatus = ClientStatus.WaitingPlayers;

                    if (address != IPAddress.Loopback.ToString())
                    {
                        controller.SupressUpdate();
                    }
                    ScreenManager.GetInstance().RemoveLastScreen();
                    ScreenManager.GetInstance().AddScreen(new WaitingPlayerScreen());
                    controller.ReleaseUpdate();
                }
            }

        }

        private void HandleCharacterSelectionMessage(String address, AbstractMessage message)
        {
            CharacterSelectionMessage characterSelectionMessage = (CharacterSelectionMessage)message;

            clientStatus = ClientStatus.CharacterSelection;

            controller.SupressUpdate();

            ScreenManager.GetInstance().RemoveLastScreen();
            ScreenManager.GetInstance().AddScreen(new CharacterSelectionScreen());

            controller.ReleaseUpdate();
        }

        private void HandleWaitingOponentMessage(String address, AbstractMessage message)
        {
            clientStatus = ClientStatus.WaitingCharacterSelection;

            controller.SupressUpdate();

            ScreenManager.GetInstance().RemoveLastScreen();
            ScreenManager.GetInstance().AddScreen(new WaitingPlayerScreen());

            controller.ReleaseUpdate();
        }

        private void HandleStartGameMessage(string address, AbstractMessage message)
        {
            StartGameMessage startGameMessage = (StartGameMessage)message;

            MapEngine.Initialize(IO.LoadMap("amude"));
            Camera.Initialize(MapEngine.GetInstance().Map);

            DefaultDirection charactersDirection;
            Point charactersPosition;
            AnimationType charactersAnimation;

            if (controller.IsServer)
            {
                Camera.GetInstance().FocusOn(new Point(0, MapEngine.GetInstance().Map.Height / 2));
                charactersDirection = DefaultDirection.Right;
                charactersPosition = new Point(0, (MapEngine.GetInstance().Map.Height / 2) - 3);
                charactersAnimation = AnimationType.StaticRight;
            }
            else
            {
                Camera.GetInstance().FocusOn(new Point(MapEngine.GetInstance().Map.Width, MapEngine.GetInstance().Map.Height / 2));
                charactersDirection = DefaultDirection.Left;
                charactersPosition = new Point(MapEngine.GetInstance().Map.Width - 1, (MapEngine.GetInstance().Map.Height / 2) - 3);
                charactersAnimation = AnimationType.StaticLeft;
            }

            controller.SupressUpdate();

            foreach (Character character in controller.GetLocalPlayer().GetCharacters())
            {
                character.ClearAnimatedMovements();
                character.DefaultDirection = charactersDirection;
                character.MapLocation = charactersPosition;
                character.AddAnimatedMovement(charactersAnimation, charactersPosition);

                MapEngine.GetInstance().SetObject(character);
                charactersPosition.Y += 1;
            }

            ScreenManager.GetInstance().RemoveLastScreen();
            ScreenManager.GetInstance().AddScreen(GameScreen.GetInstance());

            controller.ReleaseUpdate();
        }

        private void HandleCharacterPositionMessage(String address, AbstractMessage message)
        {
            CharacterPositionMessage characterPositionMessage = (CharacterPositionMessage)message;
            Player oponent = controller.GetPlayer(characterPositionMessage.PlayerName);
            DefaultDirection oponentDefaultDirection;
            AnimationType oponentDefaultAnimation;

            if (controller.IsServer)
            {
                oponentDefaultDirection = DefaultDirection.Left;
                oponentDefaultAnimation = AnimationType.StaticLeft;

                controller.SupressUpdate();

                foreach (Character character in oponent.GetCharacters())
                {
                    character.ClearAnimatedMovements();
                    character.DefaultDirection = oponentDefaultDirection;
                    character.MapLocation = characterPositionMessage.CharactersPosition[character.RootName];
                    character.AddAnimatedMovement(oponentDefaultAnimation, character.MapLocation);

                    MapEngine.GetInstance().SetObject(character);
                }

                controller.ReleaseUpdate();
            }
            else
            {
                oponentDefaultDirection = DefaultDirection.Right;
                oponentDefaultAnimation = AnimationType.StaticRight;

                controller.SupressUpdate();

                foreach (String characterName in characterPositionMessage.CharactersPosition.Keys)
                {
                    Character oponentCharacter = Bundle.Characters[characterName];
                    oponentCharacter.ClearAnimatedMovements();
                    oponentCharacter.DefaultDirection = oponentDefaultDirection;
                    oponentCharacter.MapLocation = characterPositionMessage.CharactersPosition[characterName];
                    oponentCharacter.AddAnimatedMovement(oponentDefaultAnimation, oponentCharacter.MapLocation);
                    oponent.AddCharacter(oponentCharacter);

                    MapEngine.GetInstance().SetObject(oponentCharacter);
                }

                controller.ReleaseUpdate();
            }

            Camera.GetInstance().UpdateLocations = true;
        }

        private void HandleStartBattleMessage(String address, AbstractMessage message)
        {
            GameScreen gameScreen = GameScreen.GetInstance();
            StartBattleMessage startBattleMessage = (StartBattleMessage)message;
            Turn actualTurn = startBattleMessage.Turn;

            controller.SupressUpdate();

            gameScreen.ActualTurn = actualTurn;
            UpdateAffects(startBattleMessage.Affects);
            gameScreen.DoAffects(actualTurn);
            
            Character actualCharacter = controller.GetPlayer(actualTurn.PlayerName).GetCharacter(actualTurn.CharacterName);
            Camera.GetInstance().FocusOn(actualCharacter.MapLocation);
            gameScreen.MapCursor.MapLocation = Camera.GetInstance().CenterLocation;
            gameScreen.MapCursor.IsVisible = gameScreen.SelectedCharacter == null;
            gameScreen.Handlers.Peek().Watch(actualCharacter);
            gameScreen.Handlers.Peek().SetTrigger(Trigger.TriggerActionBox);

            controller.ReleaseUpdate();
        }

        private void HandleUpdateMessage(String address, AbstractMessage message)
        {
            GameScreen gameScreen = GameScreen.GetInstance();
            UpdateMessage updateMessage = (UpdateMessage)message;
            Player player = controller.GetPlayer(updateMessage.Action.PlayerName);
            Character character = null;

            if (updateMessage.Action is ActionMessageWalk)
            {
                ActionMessageWalk walkMessage = (ActionMessageWalk)updateMessage.Action;
                character = player.GetCharacter(walkMessage.Actor);
                
                controller.SupressUpdate();

                gameScreen.MoveCharacter(character, walkMessage.Point);

                controller.ReleaseUpdate();

                IInputHandler handler = gameScreen.Handlers.Peek();
                handler.Watch(character);
                handler.SetTrigger(Trigger.SendConfirmUpdate);
            }
            else if (updateMessage.Action is ActionMessageAttack)
            {
                ActionMessageAttack attackMessage = (ActionMessageAttack)updateMessage.Action;
                character = player.GetCharacter(attackMessage.Actor);
                Player targetPlayer = controller.GetPlayer(attackMessage.TargetPlayer);
                Character target = targetPlayer.GetCharacter(attackMessage.Target);

                controller.SupressUpdate();

                gameScreen.DoBattle(character, target, attackMessage.Damage, 
                    attackMessage.CounterAttack, attackMessage.CounterDamage);

                IInputHandler handler = gameScreen.Handlers.Peek();
                handler.Watch(character);
                handler.Watch(target);
                handler.SetTrigger(Trigger.SendConfirmUpdate);

                controller.ReleaseUpdate();

                
            }
            else if (updateMessage.Action is ActionMessageFinish)
            {
                gameScreen.ActualTurn = null;
                ConfirmUpdateMessage confirmUpdateMessage = new ConfirmUpdateMessage();
                confirmUpdateMessage.ConfirmationType = ConfirmationType.UpdateMessage;
                Controller.GetInstance().Client.SendData(confirmUpdateMessage);
            }
        }

        private void HandleTurnMessage(String address, AbstractMessage message)
        {
            GameScreen gameScreen = GameScreen.GetInstance();
            TurnMessage turnMessage = (TurnMessage)message;

            gameScreen.ActualTurn = turnMessage.Turn;
            gameScreen.Handlers.Peek().FillOptions();

            if (gameScreen.SelectedCharacter == null)
            {
                gameScreen.MapCursor.MapLocation = Camera.GetInstance().CenterLocation;
                gameScreen.MapCursor.IsVisible = true;
            }
            else
            {
                Camera.GetInstance().FocusOn(gameScreen.SelectedCharacter.MapLocation);
                gameScreen.MapCursor.IsVisible = false;
            }
        }

        private void HandleAffectsMessage(String address, AbstractMessage message)
        {
            GameScreen gameScreen = GameScreen.GetInstance();
            AffectsMessage affectsMessage = (AffectsMessage)message;

            controller.SupressUpdate();

            UpdateAffects(affectsMessage.Affects);
            gameScreen.DoAffects(affectsMessage.Turn);
            if (affectsMessage.Turn == null)
            {
                ConfirmUpdateMessage confirmUpdateMessage = new ConfirmUpdateMessage();
                confirmUpdateMessage.ConfirmationType = ConfirmationType.AffectsMessage;
                Controller.GetInstance().Client.SendData(confirmUpdateMessage);    
            }
            else
            {
                Character actualCharacter = controller.GetPlayer(affectsMessage.Turn.PlayerName).GetCharacter(
                affectsMessage.Turn.CharacterName);
                Camera.GetInstance().FocusOn(actualCharacter.MapLocation);
                gameScreen.MapCursor.MapLocation = Camera.GetInstance().CenterLocation;
                gameScreen.MapCursor.IsVisible = gameScreen.SelectedCharacter == null;
                gameScreen.Handlers.Peek().Watch(actualCharacter);
                gameScreen.Handlers.Peek().SetTrigger(Trigger.SendConfirmAffects);
            }

            controller.ReleaseUpdate();
        }

        private void HandleChatMessage(String address, AbstractMessage message)
        {
            GameScreen.GetInstance().ChatBox.ShowMessage((ChatMessage)message);
        }

        private void HandleEndGameMessage(String address, AbstractMessage message)
        {
            EndGameMessage endGameMessage = (EndGameMessage)message;

            controller.SupressUpdate();

            ScreenManager.GetInstance().RemoveLastScreen();
            ScreenManager.GetInstance().AddScreen(new EndGameScreen(endGameMessage.Winner));
            Controller.GetInstance().ClearGame();

            controller.ReleaseUpdate();
        }

        private void HandlePingMessage(String address, AbstractMessage message)
        {
            if (controller.Client.LastPing == default(DateTime))
            {
                Controller.GetInstance().Client.StartPing();
            }
            controller.Client.LastPing = DateTime.Now;
            controller.Client.SendData(message);
        }

        private void HandleInvalidGuidMessage(String address, AbstractMessage message)
        {
            ErrorScreen errorScreen = new ErrorScreen("Falha de conexão",
                                                      "Perda de integridade com o servidor remoto.");

            controller.SupressUpdate();

            controller.QuitGame(false, false);
            ScreenManager.GetInstance().AddScreen(errorScreen);

            controller.ReleaseUpdate();

            controller.StopClient();
        }

        private void HandleLeaveMessage(String address, AbstractMessage message)
        {
            String description = String.Format("{0} deixou o jogo.",
                                               controller.GetRemotePlayer().Name);

            ErrorScreen errorScreen = new ErrorScreen("Fim de Jogo",
                                                      description);

            controller.SupressUpdate();

            controller.QuitGame(false, false);
            ScreenManager.GetInstance().AddScreen(errorScreen);

            controller.ReleaseUpdate(); 
        }

        private void UpdateAffects(KeyValuePair<CharacterKey, Affect[]>[] affects)
        {
            foreach (KeyValuePair<CharacterKey, Affect[]> kv in affects)
            {
                Character target = controller.GetPlayer(kv.Key.Owner).GetCharacter(kv.Key.Character);

                target.UpdateAffects(kv.Value);
            }
        }
    }
}
