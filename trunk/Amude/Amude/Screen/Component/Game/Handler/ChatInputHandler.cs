using Amude.Core;
using Amude.Domain;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text;
using Amude.Network.Messages;
using System.Net;
using Amude.Global;

namespace Amude.Screen.Component.Game.Handler
{
    internal class ChatInputHandler : IInputHandler
    {
        private GameScreen gameScreen;
        private StringBuilder typedMessage;

        public String TypedMessage
        {
            get { return typedMessage.ToString(); }
        }

        public ChatInputHandler(GameScreen gameScreen)
        {
            typedMessage = new StringBuilder();
            this.gameScreen = gameScreen;            
        }

        #region AbstractHandler Members

        public void Update(float passedTime)
        {
            gameScreen.ChatBox.Update(passedTime);

            foreach (Keys key in KeyboardManager.Manager.TypedKeys)
            {
                if (gameScreen.ChatBox.MeasureString(TypedMessage) < ChatBox.MAX_STRING_SIZE && key != Keys.Back)
                {
                    string k = SharedFunctions.GetText(key);

                    if (k.Length > 0)
                    {
                        if (KeyboardManager.Manager.IsCapsLock ^ KeyboardManager.Manager.IsShift)
                            typedMessage.Append(k.Substring(k.Length - 1).ToUpper());
                        else
                            typedMessage.Append(k.Substring(k.Length - 1).ToLower());
                    }

                }

                if (key == Keys.Back && typedMessage.Length > 0)
                {
                    typedMessage.Remove(typedMessage.Length - 1, 1);
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            { 
                ChatMessage message = new ChatMessage();
                message.UserName = Controller.GetInstance().GetLocalPlayer().Name;
                message.Message = typedMessage.ToString();

                Controller.GetInstance().Client.SendData(message);

                gameScreen.ChatBox.HideInput();
                gameScreen.Handlers.Pop();
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                gameScreen.ChatBox.HideInput();
                gameScreen.Handlers.Pop();
            }
        }

        public void FillOptions()
        {
            // Delegue ao segundo Handler
            IInputHandler[] handlers = gameScreen.Handlers.ToArray();
            if (handlers.Length > 1)
            {
                handlers[1].FillOptions();
            }
        }

        public void Watch(Character character)
        {
            // Delegue ao segundo Handler
            IInputHandler[] handlers = gameScreen.Handlers.ToArray();
            if(handlers.Length > 1)
            {
                handlers[1].Watch(character);
            }
        }

        public void SetTrigger(Trigger trigger)
        {
            // Delegue ao segundo Handler
            IInputHandler[] handlers = gameScreen.Handlers.ToArray();
            if (handlers.Length > 1)
            {
                handlers[1].SetTrigger(trigger);
            }
        }

        #endregion
    }
}
