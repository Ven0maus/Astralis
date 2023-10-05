using SadRogue.Primitives;

namespace Astralis
{
    public class Constants
    {
        public const int GameSeed = 601;
        public const bool DebugMode = false;
        public const bool FullScreen = false;

        public static int ScreenWidth { get { return Resolution.ScreenCellsX; } }
        public static int ScreenHeight { get { return Resolution.ScreenCellsY; } }

        public static readonly Color GameTitleColor = Color.Ivory;
        public static readonly Color GameTitleShadowColor = Color.Lerp(Color.Black, Color.Gray, 0.1f);
        public const string GameTitle = "Astralis";
        public const string GameTitleFancy = @"
     _            _                   _   _       
    / \     ___  | |_   _ __   __ _  | | (_)  ___ 
   / _ \   / __| | __| | '__| / _` | | | | | / __|
  / ___ \  \__ \ | |_  | |   | (_| | | | | | \__ \
 /_/   \_\ |___/  \__| |_|    \__,_| |_| |_| |___/";

        public static class Fonts
        {
            public const string WorldObjects = "Resources/Tilesets/WorldObjects_16x16.font";
            public const string LCD = "Resources/Tilesets/LCD_Tileset_16x16.font";
            public const string LordNightmare = "Resources/Tilesets/Lord_Nightmare-Fixedsys-03_8x16.font";
        }

        public static class WorldGeneration
        {
            public const float WorldZoomFactor = 1.75f;
            public const bool DrawBordersOnDebugMode = false;
            public const int ExtraChunkRadius = 2;
            public const int ChunkSize = 32;
        }

        public static class Configuration
        {
            public const string JsonFilesPath = "Configuration/Data";
        }
    }
}
