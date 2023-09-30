using SadRogue.Primitives;

namespace Astralis
{
    public class Constants
    {
        public const bool DebugMode = true;

        public static readonly Point FontSize = new(8, 16);

        /// <summary>
        /// The perceived or effective resolution of the game, which can differ from the physical or native resolution of the display device itself.
        /// </summary>
        public static readonly Point VirtualResolution = new(1280, 720); // new(1280, 720);

        public const bool FullScreen = false;
        public static int ScreenWidth { get { return VirtualResolution.X / FontSize.X; } }
        public static int ScreenHeight { get { return VirtualResolution.Y / FontSize.Y; } }

        public static readonly Color GameTitleColor = Color.White;
        public static readonly Color GameTitleShadowColor = Color.Lerp(Color.Red, Color.Black, 0.75f);
        public const string GameTitle = "Astralis";
        public const string GameTitleFancy = @"
     _            _                    _   _       
    / \     ___  | |_   _ __   __ _  | | (_)  ___ 
   / _ \   / __| | __| | '__| / _` | | | | | / __|
  / ___ \  \__ \ | |_  | |   | (_| | | | | | \__ \
 /_/   \_\ |___/  \__| |_|    \__,_| |_| |_| |___/";

        public static class Fonts
        {
            public const string Aesomatica = "Resources/Tilesets/Aesomatica_16x16.font";
        }

        public static class WorldGeneration
        {
            public const bool DrawBordersOnDebugMode = false;
            public const int ExtraChunkRadius = 2;
            public const int ChunkSize = 50;
        }

        public static class Configuration
        {
            public const string JsonFilesPath = "Configuration/Data";
        }
    }
}
