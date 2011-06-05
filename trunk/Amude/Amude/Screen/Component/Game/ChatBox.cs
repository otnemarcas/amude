using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Screen.Component.Game.Handler;
using Microsoft.Xna.Framework.Graphics;
using Amude.Core;
using Microsoft.Xna.Framework;
using Amude.Global;
using Amude.Network.Messages;

namespace Amude.Screen.Component.Game
{
    internal class MessageItem
    {
        public float LifeTime { get; set;}
        public ChatMessage Message { get; set; }
        public Color Color { get; set; }
    }

    internal class ChatBox
    {
        public const int MAX_STRING_SIZE = 740;
        public const int MESSAGE_LIFE_TIME = 8;
        public const int TOTAL_MESSAGES_TO_SHOW = 6;

        public ChatInputHandler ChatInputHandler { get; set; }
        
        private bool writing;
        private bool showReceivedMessages;

        private Texture2D background;
        private Vector2 inputChatScale;
        private Vector2 inputChatPosition;
        private Vector2 chatTextPosition;
        
        private Color backgroundColor;
        private Color textColor;

        private Vector2 messageScale;
        private Vector2 messagePosition;
        private Vector2 messageTextPosition;
        
        private SpriteFont font;
        private Queue<MessageItem> messages;

        public ChatBox()
        {
            background = IO.LoadSingleTexture("Image/Global/pixel");
            font = IO.LoadFont("Data/Global/ChatBoxFont");

            writing = false;
            showReceivedMessages = false;
            messages = new Queue<MessageItem>();

            inputChatPosition = new Vector2(40, 720);
            inputChatScale = new Vector2(900, 50);            
            chatTextPosition = new Vector2(60, 730);

            messagePosition = new Vector2(40, 660);
            messageScale = new Vector2(435, 50);            
            messageTextPosition = new Vector2(60, 670);

            textColor = Color.Black;
            textColor.A = 190;
            
            backgroundColor = Color.White;
            backgroundColor.A = 80;
        }

        public void Update(float passedTime)
        {
            if (showReceivedMessages)
            {
                if (messages.Count > 0)
                {
                    foreach (MessageItem item in messages)
                    {
                        item.LifeTime -= passedTime;
                    }

                    MessageItem message = messages.Peek();
                    if (message.LifeTime <=0)
                    {
                        messages.Dequeue();
                    }

                    while (messages.Count > TOTAL_MESSAGES_TO_SHOW)
                    {
                        messages.Dequeue();
                    }
                }
                if (messages.Count == 0)
                {
                    showReceivedMessages = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (writing)
            {
                spriteBatch.Draw(background,
                                 inputChatPosition,
                                 null,
                                 backgroundColor,
                                 0,
                                 Vector2.Zero,
                                 inputChatScale,
                                 SpriteEffects.None,
                                 Constants.LD_INTERFACE_4);


                spriteBatch.DrawString(font,
                                       "Mensagem: " + ChatInputHandler.TypedMessage.ToString(),
                                       chatTextPosition,
                                       textColor,
                                       0,
                                       Vector2.Zero,
                                       1,
                                       SpriteEffects.None,
                                       Constants.LD_INTERFACE_5);

            }

            if (showReceivedMessages)
            {
                List<MessageItem> messagesToShow = messages.ToList();
                messagesToShow.Reverse();
                for (int i = messagesToShow.Count - 1; i >= 0; i--)
                {
                    DrawMessageItem(spriteBatch, messagesToShow[i], i);
                }
            }
        }

        public void DrawMessageItem(SpriteBatch spriteBatch, MessageItem message, int i)
        {
            Vector2 position = messagePosition;            
            position.Y -= (50 * i);

            spriteBatch.Draw(background,
                             position,
                             null,
                             backgroundColor,
                             0,
                             Vector2.Zero,
                             messageScale,
                             SpriteEffects.None,
                             Constants.LD_INTERFACE_4);

            List<String> messageLines = SharedFunctions.LayoutString(message.Message.UserName + ":" + message.Message.Message,
                                                                     400,
                                                                     font);
            for (int line = 0; line < messageLines.Count; line++)
			{
                Vector2 textPosition = messageTextPosition;
                textPosition.Y -= (50 * i);
                if (messageLines.Count > 1)
                {
                    textPosition.Y += (line == 0 ? -10 : 13);
                }
                spriteBatch.DrawString(font,
                                 messageLines[line],
                                 textPosition,
                                 message.Color,
                                 0,
                                 Vector2.Zero,
                                 1,
                                 SpriteEffects.None,
                                 Constants.LD_INTERFACE_5);
			}

        }

        public void Show()
        {
            writing = true;
        }

        public void ShowMessage(ChatMessage message)
        {
            showReceivedMessages = true;

            MessageItem messageItem = new MessageItem();
            messageItem.LifeTime = MESSAGE_LIFE_TIME;
            messageItem.Message = message;

            if (message.UserName == Controller.GetInstance().GetLocalPlayer().Name)
            {
                if (Controller.GetInstance().IsServer)
                {
                    messageItem.Color = new Color(227, 4, 4);
                }
                else
                {
                    messageItem.Color = new Color(4, 4, 227);
                }
            }
            else
            {
                if (Controller.GetInstance().IsServer)
                {
                    messageItem.Color = Color.Blue;
                }
                else
                {
                    messageItem.Color = Color.Red;
                }
            }

            messages.Enqueue(messageItem);
        }

        public void HideInput()
        {
            writing = false;
        }

        public float MeasureString(String value)
        {
            return font.MeasureString(value).X;
        }
        
    }
}
