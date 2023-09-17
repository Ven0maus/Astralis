using SadRogue.Primitives;

namespace Astralis
{
    internal class Constants
    {
        public static readonly Point FontSize = new(8, 16);
        public static readonly Point VirtualResolution = new(1280, 720);
        public static readonly Point DefaultResolution = new(1920, 1080);
        public static int ScreenWidth { get { return VirtualResolution.X / FontSize.X; } }
        public static int ScreenHeight { get { return VirtualResolution.Y / FontSize.Y; } }

        public static readonly Color GameTitleColor = Color.White;
        public static readonly Color GameTitleShadowColor = Color.Lerp(Color.Red, Color.Black, 0.75f);
        public const string GameTitle = "Astralis";
        public const string GameTitleFancy =
@"     _            _                    _   _       
    / \     ___  | |_   _ __   __ _  | | (_)  ___ 
   / _ \   / __| | __| | '__| / _` | | | | | / __|
  / ___ \  \__ \ | |_  | |   | (_| | | | | | \__ \
 /_/   \_\ |___/  \__| |_|    \__,_| |_| |_| |___/";

    }
}
