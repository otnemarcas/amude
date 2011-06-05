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
using System.Linq;
using System.Net;
using System.Threading;
using Amude.Domain;
using Amude.Global;
using Amude.Network;
using Amude.Network.Messages;
using Amude.Screen;
using Amude.Screen.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Amude.Core
{
    internal enum BlockType
    {
        Block,
        Unblock,
        Verify
    }

    internal class Controller
    {
        #region Singleton

        static Controller instance;

        internal static Controller GetInstance()
        {
            if (instance == null)
                instance = new Controller();

            return instance;
        }

        #endregion

        private Amude game;
        private Dictionary<IPAddress, Player> players;
        private Server server;
        private Client client;
        private bool isServer;

        protected volatile AutoResetEvent supressUpdate = null;

        private volatile bool updating, drawing;

        public GraphicsDeviceManager Graphics
        {
            get
            {
                return game.graphics;
            }
        }

        public bool IsServer
        {
            get
            {
                return isServer;
            }
        }

        public Server Server
        {
            get { return server; }
        }

        public Client Client
        {
            get { return client; }
        }

        private Controller()
        {
            players = new Dictionary<IPAddress, Player>();
            updating = drawing = false;
        }

        public void AddPlayer(IPAddress ip, Player player)
        {
            players.Add(ip, player);
        }

        public List<Player> GetPlayers()
        {
            return players.Values.ToList();
        }

        public Player GetPlayer(string name)
        {
            foreach (Player player in players.Values)
            {
                if (player.Name == name)
                {
                    return player;
                }
            }
            throw new ArgumentException(name + " não é um jogador válido.");
        }

        public Player GetPlayer(IPAddress address)
        {
            return players[address];
        }

        public Player GetLocalPlayer()
        {
            return players.Values.First();
        }

        public Player GetRemotePlayer()
        {
            return players.Values.ToList()[Constants.NETPLAYER_INDEX];
        }

        public void Initialize(Amude game)
        {
            using (Screen.LoadScreen loadScreen = new Screen.LoadScreen())
            {
                loadScreen.Visible = true;
                ProgressBarCallBack callback = loadScreen.GetProgressBarCallBack();
                this.game = game;

                IO.Initialize(game.Content);
                IO.VerifyConfigFiles();

                Bundle.Load(callback);

                Player player = new Player(IPAddress.Loopback);
                players.Add(player.IPAddress, player);

                loadScreen.Dispose();
            }
        }

        protected Boolean Block(BlockType blockType)
        {
            lock (this)
            {

                switch (blockType)
                {
                    case BlockType.Block:
                        supressUpdate = new AutoResetEvent(false);
                        break;
                    case BlockType.Unblock:
                        if (supressUpdate != null)
                        {
                            supressUpdate.Set();
                            supressUpdate = null;
                        }
                        break;
                    case BlockType.Verify:
                        return supressUpdate != null;
                }
                return true;
            }
        }

        public void Exit()
        {
            StopClient();

            StopServer();

            game.Exit();
        }

        public void Update(GameTime gameTime)
        {
            if (Block(BlockType.Verify))
            {
                supressUpdate.WaitOne();
            }

            updating = true;

            KeyboardManager.Manager.Update();
            AudioManager.Update();
            float passedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ScreenManager.GetInstance().Update(passedTime);

            updating = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Block(BlockType.Verify))
            {
                supressUpdate.WaitOne();
            }

            drawing = true;

            game.GraphicsDevice.Clear(Color.Gray);
            ScreenManager.GetInstance().Draw(spriteBatch);

            drawing = false;
        }

        public void ApplyChanges()
        {
            game.ApplyGraphicChanges();
        }

        public void InitializeClient()
        {
            while (players.Count > 1)
            {
                players.Remove(players.Last().Value.IPAddress);
            }
            client = new Client();
            client.Start();
        }

        public void StopClient()
        {
            if (client != null)
            {
                client.Stop();
                client = null;
            }
        }

        public void InitializeServer()
        {
            isServer = true;

            while (players.Count > 1)
            {
                players.Remove(players.Last().Value.IPAddress);
            }
            server = new Server();
            server.Start();

            InitializeClient();
        }

        public void StopServer()
        {
            isServer = false;
            if (server != null)
            {
                server.Stop();
                server = null;
            }

            StopClient();
        }

        public void QuitGame(bool sendNotification, bool updateNextScreen)
        {
            if (IsServer && server != null && sendNotification && players.Count > 1)
            {
                server.SendData(GetRemotePlayer().IPAddress, new LeaveMessage());
            }
            else if (client != null && sendNotification)
            {
                client.SendData(new LeaveMessage());
            }

            ClearGame();
            ScreenManager.GetInstance().RemoveLastScreen(updateNextScreen);           
        }

        public void ClearGame()
        {
            GameScreen.ClearInstance();

            players = new Dictionary<IPAddress, Player>();
            Player player = new Player(IPAddress.Loopback);
            players.Add(player.IPAddress, player);

            if (IsServer)
            {
                StopServer();
            }
            else
            {
                StopClient();
            }

            isServer = false;
        }

        public void SupressUpdate()
        {
            Block(BlockType.Block);
            while (updating || drawing) ;
        }

        public void ReleaseUpdate()
        {
            Block(BlockType.Unblock);
        }
    }

    public delegate void ProgressBarCallBack(int max);
}
