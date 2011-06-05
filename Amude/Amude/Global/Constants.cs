using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Amude.Global
{
    internal class Constants
    {
        public static Color SERVER_COLOR = new Color(225, 80, 80);
        public static Color CLIENT_COLOR = new Color(80, 80, 225);
        public const int LOCALPLAYER_INDEX = 0;
        public const int NETPLAYER_INDEX = 1;

        public const int CONFIG_FILES_COUNT = 35;

        public const int TILE_SIZE = 97;
        public const int MAX_PLAYERS = 2;
        public const int MAX_CHARACTERS = 6;

        // LayerDepth de GameScreen
        public const float LD_BACKGROUND = 1f;
        public const float LD_BACKLAYER_0 = 0.998f;
        public const float LD_BACKLAYER_1 = 0.996f;
        public const float LD_BACKLAYER_2 = 0.994f;
        public const float LD_BACKLAYER_3 = 0.992f;
        public const float LD_TERRAIN = 0.99f;
        public const float LD_TERRAIN_TOP0 = 0.988f;
        public const float LD_TERRAIN_TOP1 = 0.986f;
        public const float LD_TERRAIN_TOP2 = 0.984f;
        public const float LD_TERRAIN_TOP3 = 0.982f;
        public const float LD_ENVIRONMENT = 0.98f;  // 0,99 - 0,97
        public const float LD_CHARACTER_L0 = 0.95f; // 0,96 - 0,94
        public const float LD_EFFECT_L0 = 0.92f;    // 0,93 - 0,91
        public const float LD_CHARACTER_L1 = 0.89f; // 0,90 - 0,88
        public const float LD_EFFECT_L1 = 0.86f;    // 0,87 - 0,85
        public const float LD_CHARACTER_L2 = 0.83f; // 0,84 - 0,82
        public const float LD_EFFECT_L2 = 0.80f;    // 0,81 - 0,79
        public const float LD_CHARACTER_L3 = 0.77f; // 0,78 - 0,76
        public const float LD_EFFECT_L3 = 0.74f;    // 0,75 - 0,73
        public const float LD_CHARACTER_L4 = 0.71f; // 0,72 - 0,70
        public const float LD_EFFECT_L4 = 0.68f;    // 0,69 - 0,67
        public const float LD_CHARACTER_L5 = 0.65f; // 0,66 - 0,64
        public const float LD_EFFECT_L5 = 0.62f;    // 0,63 - 0,61
        public const float LD_CHARACTER_L6 = 0.59f; // 0,60 - 0,58
        public const float LD_EFFECT_L6 = 0.56f;    // 0,57 - 0,55
        public const float LD_CHARACTER_L7 = 0.53f; // 0,54 - 0,52
        public const float LD_EFFECT_L7 = 0.50f;    // 0,51 - 0,49
        public const float LD_CHARACTER_L8 = 0.47f; // 0,48 - 0,46
        public const float LD_EFFECT_L8 = 0.44f;    // 0,45 - 0,43
        public const float LD_CHARACTER_L9 = 0.41f; // 0,42 - 0,40
        public const float LD_EFFECT_L9 = 0.38f;    // 0,39 - 0,37
        public const float LD_PROJECTILE = 0.35f;   // 0,36 - 0,34 
        public const float LD_INFO = 0.19f;         // 0,20 - 0,18
        public const float LD_MAPINFO_0 = 0.189f;
        public const float LD_MAPINFO_1 = 0.188f;
        public const float LD_MAPINFO_2 = 0.187f;
        public const float LD_MAPINFO_3 = 0.186f;
        public const float LD_MAPINFO_4 = 0.185f;
        public const float LD_MAPINFO_5 = 0.184f;
        public const float LD_INTERFACE_0 = 0.16f;    // 0,17 - 0,15
        public const float LD_INTERFACE_1 = 0.158f;
        public const float LD_INTERFACE_2 = 0.156f;
        public const float LD_INTERFACE_3 = 0.154f;
        public const float LD_INTERFACE_4 = 0.152f;
        public const float LD_INTERFACE_5 = 0.15f;
        public const float LD_FRONTLAYER_0 = 0.148f;
        public const float LD_FRONTLAYER_1 = 0.146f;
        public const float LD_FRONTLAYER_2 = 0.144f;
        public const float LD_FRONTLAYER_3 = 0.142f;

        public const int TACTICS_MODE_WIDTH = 3;
    }
}
