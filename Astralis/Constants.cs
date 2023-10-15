using Astralis.Extended;
using Astralis.GameCode.Npcs;
using SadRogue.Primitives;
using System;

namespace Astralis
{
    public class Constants
    {
        public static int GameSeed = 601;
        public const bool DebugMode = false;
        public const bool FullScreen = false;

        public static Random Random;

        public static int ScreenWidth { get { return Resolution.ScreenCellsX; } }
        public static int ScreenHeight { get { return Resolution.ScreenCellsY; } }

        public static readonly Color GameTitleColor = Color.Ivory;
        public static readonly Color GameTitleShadowColor = Color.Lerp(Color.Black, GameTitleColor, 0.35f);
        public const string GameTitle = "Astralis";
        public const string GameTitleFancy = @"
     _            _                   _   _       
    / \     ___  | |_   _ __   __ _  | | (_)  ___ 
   / _ \   / __| | __| | '__| / _` | | | | | / __|
  / ___ \  \__ \ | |_  | |   | (_| | | | | | \__ \
 /_/   \_\ |___/  \__| |_|    \__,_| |_| |_| |___/";

        public static class Fonts
        {
            /// <summary>
            /// Fonts that are created and used during gameplay are stored here.
            /// </summary>
            public const string DynamicFontsPath = $"{Configuration.GamedataPath}/Fonts";

            public static class UserInterfaceFonts
            {
                public const string Anno = "Resources/Tilesets/Anno_16x16.font";
            }

            public static class WorldFonts
            {
                public const string WorldObjects = "Resources/Tilesets/WorldObjects_16x16.font";
            }

            public static class NpcFonts
            {
                public const string NpcFont = "Resources/Npcs/Npcs_16x16.font";
                public const string CharacterCreationBaseFont = "Resources/Npcs/BaseNpc_16x16.font";
                /// <summary>
                /// Font that is modified with the player glyphs and stored as gamedata
                /// </summary>
                public const string GamedataNpcFont = $"{DynamicFontsPath}/Npcs_16x16.font";

                internal static Color[] PredefinedColors = new Color[]
                {
                    Color.Coral,
                    Color.Green,
                    Color.Blue,
                    Color.DarkOrange,
                    Color.OliveDrab,
                    Color.AnsiYellowBright,
                    Color.Cyan,
                    Color.Magenta,
                    Color.Brown,
                    Color.Teal,
                    Color.Gray,
                    Color.Lime,
                    Color.Thistle,
                    Color.DarkRed,
                    Color.Indigo
                };

                internal static Color[] GetSkinColors(Race race)
                {
                    switch (race)
                    {
                        case Race.Orc:
                            return new[] { "#4D2600".HexToColor(), "#006600".HexToColor() };
                        case Race.Human:
                        case Race.Elf:
                        case Race.Dwarf:
                            return new[] { "#e6bc98".HexToColor(), "#3b2219".HexToColor() };
                        default:
                            throw new NotImplementedException($"Skin color for race '{race}' not implemented.");
                    }
                }
            }
        }

        public static class WorldGeneration
        {
            public const float WorldZoomFactor = 1.75f;
            public const bool DrawBordersOnDebugMode = false;
            public const int ExtraChunkRadius = 2;
            public const int ChunkSize = 32;
        }

        public static class PlayerData
        {
            public const int PlayerForwardGlyph = 2;
        }

        public static class Configuration
        {
            public const string JsonFilesPath = "Configuration/Data";
            public const string GamedataPath = "Gamedata";
            public const int NpcTraitsStartingPoints = 4;
        }
    }
}
