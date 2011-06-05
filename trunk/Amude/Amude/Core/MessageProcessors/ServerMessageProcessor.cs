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
using System.Linq;
using System.Net;
using Amude.Domain;
using Amude.Global;
using Amude.Network.Messages;
using Amude.Network.Messages.Data;
using System.Collections.Generic;
using Amude.Screen;
using Amude.Screen.Core;
using Amude.Domain.Attribute;

namespace Amude.Core.MessageProcessors
{
    internal class ServerMessageProcessor
    {
        enum ServerStatus
        {
            WaitingPlayer,
            WaitingCharacterSelection,
            WaitingCharacterPosition,
            Game
        }

        ServerStatus serverStatus;
        Controller controller;

        public ServerMessageProcessor()
        {
            controller = Controller.GetInstance();
            serverStatus = ServerStatus.WaitingPlayer;
        }

        public void DataArrival(String address, AbstractMessage message)
        {
            if (!VerifyGuid(address, message))
            {
                return;
            }

            if (message is SelectServerMessage)
            {
                HandleSelectServerMessage(address, message);
            }
            else if (message is CharacterSelectionMessage)
            {
                HandleCharacterSelectionMessage(address, message);
            }
            else if (message is CharacterPositionMessage)
            {
                HandleCharacterPositionMessage(address, message);
            }
            else if (message is ActionMessage)
            {
                HandleActionMessage(address, message);
            }
            else if (message is ConfirmUpdateMessage)
            {
                HandleConfirmUpdateMessage(address, message);
            }
            else if (message is ChatMessage)
            {
                HandleChatMessage(address, message);
            }
            else if (message is PingMessage)
            {
                HandlePingMessage(address, message);
            }
            else if (message is LeaveMessage)
            {
                HandleLeaveMessage(address, message);
            }
        }

        private bool VerifyGuid(String address, AbstractMessage message)
        {
            if (!(message is CloseSocketMessage) &&
                !(message is SelectServerMessage) &&
                message.Guid != controller.Server.Guid)
            {
                controller.Server.SendData(IPAddress.Parse(address), new InvalidGuidMessage());
                return false;
            }

            return true;
        }

        private void HandleSelectServerMessage(String address, AbstractMessage message)
        {
            if (serverStatus == ServerStatus.WaitingPlayer)
            {
                SelectServerMessage selectServerMessage = (SelectServerMessage)message;

                Player player = new Player(IPAddress.Parse(address));

                if (selectServerMessage.PlayerName == controller.GetLocalPlayer().Name)
                {
                    selectServerMessage.PlayerName += "2";
                }

                player.Name = selectServerMessage.PlayerName;

                controller.AddPlayer(player.IPAddress, player);

                selectServerMessage.RemotePlayerName = controller.GetLocalPlayer().Name;
                selectServerMessage.Success = true;

                controller.Server.SendData(player.IPAddress, selectServerMessage);

                if (controller.GetPlayers().Count == Constants.MAX_PLAYERS)
                {
                    serverStatus = ServerStatus.WaitingCharacterSelection;

                    controller.Server.StopSendBroadCast();
                    controller.Server.SendToAll(new CharacterSelectionMessage());
                    controller.Server.StartPing();
                }
            }
        }

        private void HandleCharacterSelectionMessage(String address, AbstractMessage message)
        {
            CharacterSelectionMessage characterSelectionMessage = (CharacterSelectionMessage)message;

            Player currentPlayer = controller.GetPlayer(IPAddress.Parse(address));
            currentPlayer.ClearCharacters();
            foreach (String rootName in characterSelectionMessage.SelectedCharacters)
            {
                currentPlayer.AddCharacter(Bundle.Characters[rootName]);
            }

            foreach (Player player in controller.GetPlayers())
            {
                if (player.GetCharacters().Count == 0)
                {
                    controller.Server.SendData(currentPlayer.IPAddress, new WaitingOponentMessage());
                    return;
                }
            }

            BattleEngine.Initialize(controller.GetLocalPlayer(), controller.GetRemotePlayer());
            StartGameMessage startGameMessage = new StartGameMessage();

            serverStatus = ServerStatus.WaitingCharacterPosition;

            controller.Server.SendToAll(startGameMessage);
        }

        private void HandleCharacterPositionMessage(String address, AbstractMessage message)
        {
            CharacterPositionMessage characterPositionMessage = (CharacterPositionMessage)message;
            Player currentPlayer = controller.GetPlayer(IPAddress.Parse(address));

            if (serverStatus != ServerStatus.WaitingCharacterPosition || currentPlayer.Ready)
            {
                return;
            }

            foreach (String characterName in characterPositionMessage.CharactersPosition.Keys)
            {
                currentPlayer.GetCharacter(characterName).MapLocation =
                    characterPositionMessage.CharactersPosition[characterName];
            }

            foreach (Player player in controller.GetPlayers())
            {
                if (player.IPAddress != currentPlayer.IPAddress)
                {
                    controller.Server.SendData(player.IPAddress, message);
                }
            }

            currentPlayer.Ready = true;
            bool ready = true;
            foreach (Player player in controller.GetPlayers())
            {
                if (!player.Ready)
                {
                    ready = false;
                }
            }
            if (ready)
            {
                StartBattleMessage startBattleMessage = new StartBattleMessage();
                BattleEngine battleEngine = BattleEngine.GetInstance();

                serverStatus = ServerStatus.Game;

                battleEngine.NextCharacterTurn();
                startBattleMessage.Turn = new Turn(battleEngine.ActualCharacter.RootName,
                    battleEngine.ActualPlayer, false);
                
                startBattleMessage.Affects = ConvertEffectsToArray(battleEngine.ProcessSpecialAbilities());

                controller.Server.SendToAll(startBattleMessage);
                battleEngine.ClearAffects();
            }
        }

        private void HandleActionMessage(String address, AbstractMessage message)
        {
            Player currentPlayer = controller.GetPlayer(IPAddress.Parse(address));
            BattleEngine battleEngine = BattleEngine.GetInstance();
            ActionMessage actionMessage = (ActionMessage)message;
            UpdateMessage updateMesssage = new UpdateMessage();

            // Valide o jogador: Caso não seja o turno do jogador, descarte a mensagem
            if (currentPlayer.Name != battleEngine.ActualPlayer)
            {
                return;
            }

            actionMessage.PlayerName = currentPlayer.Name;

            if (actionMessage is ActionMessageFinish)
            {
                ActionMessageFinish finishMessage = (ActionMessageFinish)actionMessage;
                battleEngine.LastAction = LastAction.Passed;
            }
            else if (actionMessage is ActionMessageWalk)
            {
                ActionMessageWalk walkMessage = (ActionMessageWalk)actionMessage;
                walkMessage.Actor = battleEngine.ActualCharacter.RootName;
                battleEngine.ActualCharacterHasWalked = true;
                battleEngine.LastAction = LastAction.Walked;
            }
            else if (actionMessage is ActionMessageAttack)
            {
                ActionMessageAttack attackMessage = (ActionMessageAttack)actionMessage;
                attackMessage.Actor = battleEngine.ActualCharacter.RootName;

                AttackEvent attackEvent = battleEngine.ProcessAttack(attackMessage.TargetPlayer, attackMessage.Target);
                attackMessage.Damage = attackEvent.Damage;
                attackMessage.CounterAttack = attackEvent.CounterAttack;
                attackMessage.CounterDamage = attackEvent.CounterDamage;

                battleEngine.LastAction = LastAction.Attacked;
            }

            actionMessage.PlayerName = currentPlayer.Name;
            updateMesssage.Action = actionMessage;

            controller.Server.SendToAll(updateMesssage);
            foreach (Player player in controller.GetPlayers())
            {
                player.Ready = false;
            }
        }

        private void HandleConfirmUpdateMessage(String address, AbstractMessage message)
        {
            ConfirmUpdateMessage confirmUpdateMessage = (ConfirmUpdateMessage)message;
            Player currentPlayer = controller.GetPlayer(IPAddress.Parse(address));
            BattleEngine battleEngine = BattleEngine.GetInstance();

            if (serverStatus != ServerStatus.Game || currentPlayer.Ready)
            {
                return;
            }

            currentPlayer.Ready = true;

            bool ready = true;
            foreach (Player player in controller.GetPlayers())
            {
                if (!player.Ready)
                {
                    ready = false;
                    break;
                }
            }

            if (ready)
            {
                AbstractMessage returnMessage = null;

                if (confirmUpdateMessage.ConfirmationType == ConfirmationType.UpdateMessage)
                {
                    if (battleEngine.LastAction == LastAction.Walked)
                    {
                        returnMessage = new AffectsMessage();
                        ((AffectsMessage)returnMessage).Affects =
                            ConvertEffectsToArray(battleEngine.ProcessSpecialAbilities());
                        foreach (Player player in controller.GetPlayers())
                        {
                            player.Ready = false;
                        }
                    }

                    else if (battleEngine.LastAction == LastAction.Attacked ||
                        battleEngine.LastAction == LastAction.Passed)
                    {
                        if (battleEngine.IsGameOver())
                        {
                            returnMessage = CreateEndGameMessage();
                        }
                        else
                        {
                            battleEngine.FinalizeCharacterTurn();
                            battleEngine.NextCharacterTurn();

                            returnMessage = new AffectsMessage();
                            ((AffectsMessage)returnMessage).Affects =
                                ConvertEffectsToArray(battleEngine.ProcessSpecialAbilities());
                            ((AffectsMessage)returnMessage).Turn = new Turn(battleEngine.ActualCharacter.RootName,
                                battleEngine.ActualPlayer, battleEngine.ActualCharacterHasWalked);

                            foreach (Player player in controller.GetPlayers())
                            {
                                player.Ready = false;
                            }
                        }
                    }
                }

                else if (confirmUpdateMessage.ConfirmationType == ConfirmationType.AffectsMessage)
                {
                    if (battleEngine.ActualCharacter.Health.IsDead)
                    {
                        if (battleEngine.IsGameOver())
                        {
                            returnMessage = CreateEndGameMessage();
                        }
                        else
                        {
                            returnMessage = new AffectsMessage();

                            battleEngine.FinalizeCharacterTurn();
                            battleEngine.NextCharacterTurn();

                            ((AffectsMessage)returnMessage).Affects =
                                ConvertEffectsToArray(battleEngine.ProcessSpecialAbilities());
                            ((AffectsMessage)returnMessage).Turn = new Turn(battleEngine.ActualCharacter.RootName,
                                battleEngine.ActualPlayer, battleEngine.ActualCharacterHasWalked);

                            foreach (Player player in controller.GetPlayers())
                            {
                                player.Ready = false;
                            }
                        }
                    }
                    else
                    {
                        returnMessage = new TurnMessage();
                        ((TurnMessage)returnMessage).Turn = new Turn(battleEngine.ActualCharacter.RootName,
                            battleEngine.ActualPlayer, battleEngine.ActualCharacterHasWalked);
                    }
                }
                controller.Server.SendToAll(returnMessage);
                battleEngine.ClearAffects();
            }
        }

        private void HandleChatMessage(String address, AbstractMessage message)
        {
            Controller.GetInstance().Server.SendToAll(message);
        }

        private void HandlePingMessage(String address, AbstractMessage message)
        {
            controller.Server.LastPing[IPAddress.Parse(address)] = DateTime.Now;
        }

        private void HandleLeaveMessage(String address, AbstractMessage message)
        {
            String description = String.Format("{0} deixou o jogo.",
                                               controller.GetRemotePlayer().Name);

            ErrorScreen errorScreen = new ErrorScreen("Fim de Jogo",
                                                      description);

            controller.QuitGame(false, false);
            ScreenManager.GetInstance().AddScreen(errorScreen);
        }


        private KeyValuePair<CharacterKey, Affect[]>[] ConvertEffectsToArray(
            Dictionary<CharacterKey, Dictionary<String, Affect>> dictionary)
        {
            List<KeyValuePair<CharacterKey, Affect[]>> ret = new List<KeyValuePair<CharacterKey, Affect[]>>();
            foreach (KeyValuePair<CharacterKey, Dictionary<String, Affect>> kv in dictionary)
            {
                ret.Add(new KeyValuePair<CharacterKey, Affect[]>(kv.Key, kv.Value.Values.ToArray()));
            }

            return ret.ToArray();
        }

        private EndGameMessage CreateEndGameMessage()
        {
            EndGameMessage ret = new EndGameMessage();
            List<Player> players = controller.GetPlayers();

            Player winner = null;
            foreach (Player player in players)
            {
                if (player.GetCharacters().Exists(q => !q.Health.IsDead))
                {
                    winner = player;
                    break;
                }
            }

            ret.Winner = winner.Name;
            return ret;
        }
    }
}
