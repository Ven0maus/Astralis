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
                public const string TemplateNpcFont = "Resources/Npcs/TemplateNpc_16x16.font";
                public const string BaseNpc = "Resources/Npcs/BaseNpc_16x16.font";
                public const string ProceduralNpcsFont = $"{DynamicFontsPath}/ProceduralNpcs_16x16.font";

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
            public const float WorldZoomFentity = 1.75f;
            public const bool DrawBordersOnDebugMode = false;
            public const int ExtraChunkRadius = 2;
            public const int ChunkSize = 32;
        }

        public static class Configuration
        {
            public const string JsonFilesPath = "Configuration/Data";
            public const string GamedataPath = "Gamedata";

            // Give each trait points (+1, -1) etc
            // The player starts with a specific amount of points they can use
            // In the end the player should be at 0 or positive points to start the game
            // TODO: Move traits and their points into JSON config
            // Define also a few presets that have some traits selected (positive and negative)
            // When displaying the traits, make sure the color is green or red based on positive/negative
            // Display points that the player has available
            // When selecting negative traits, the points should be added to the player available points
            // Also deducted when selecting a positive trait or removing a negative trait from selected traits

            public enum TraitPresets
            {
                None,
                Forester,
            }
        }
    }
}
