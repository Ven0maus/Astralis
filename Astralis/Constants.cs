﻿using SadRogue.Primitives;

namespace Astralis
{
    public class Constants
    {
        public static int? GameSeed = 545;
        public const bool DebugMode = false;
        public const bool FullScreen = false;

        public static readonly Point FontSize = new(8, 16);

        /// <summary>
        /// The perceived or effective resolution of the game, which can differ from the physical or native resolution of the display device itself.
        /// </summary>
        public static readonly Point VirtualResolution = new(1280, 720); // new(1280, 720);
        public static int ScreenWidth { get { return VirtualResolution.X / FontSize.X; } }
        public static int ScreenHeight { get { return VirtualResolution.Y / FontSize.Y; } }

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
            public const string Aesomatica = "Resources/Tilesets/Aesomatica_16x16.font";
            public const string WorldObjects = "Resources/Tilesets/WorldObjects_16x16.font";
            public const string LordNightmare = "Resources/Tilesets/Lord_Nightmare-Fixedsys-03_8x16.font";
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
