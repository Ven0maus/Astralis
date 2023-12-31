﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadRogue.Primitives;

namespace Astralis
{
    public static class Resolution
    {
        public static GraphicsDeviceManager Device = null;
        private static int _width, _height;
        private static bool _fullScreen = false;
        private static IFont _defaultFont;

        public static int ScreenCellsX { get; private set; }
        public static int ScreenCellsY { get; private set; }

        public static void Init(GraphicsDeviceManager device, IFont defaultFont = null)
        {
            Device = device;
            _defaultFont = defaultFont;
        }

        public static void SetResolution(int width, int height, bool fullScreen)
        {
            _width = width;
            _height = height;
            _fullScreen = fullScreen;
            ApplyResolutionSettings();
            AdjustScreenCellsToResolution();
        }

        public static void SetResolutionFromCurrentDisplayMonitor(bool fullScreen)
        {
            SetResolution(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                fullScreen);
        }

        private static void AdjustScreenCellsToResolution()
        {
            if (_defaultFont != null)
                SadConsole.Game.Instance.DefaultFont = _defaultFont;

            var font = SadConsole.Game.Instance.DefaultFont;
            var fontSize = SadConsole.Game.Instance.DefaultFontSize;
            var fontSizePoint = font.GetFontSize(fontSize);

            ScreenCellsX = _width / fontSizePoint.X;
            ScreenCellsY = _height / fontSizePoint.Y;

            SadConsole.Game.Instance.MonoGameInstance.ResizeGraphicsDeviceManager(
                fontSizePoint.ToMonoPoint(), ScreenCellsX, ScreenCellsY, 0, 0);
        }

        private static void ApplyResolutionSettings()
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (_fullScreen == false)
            {
                if (_width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                    _height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    Device.PreferredBackBufferWidth = _width;
                    Device.PreferredBackBufferHeight = _height;
                    Device.IsFullScreen = _fullScreen;
                    Device.ApplyChanges();
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate through the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if (dm.Width == _width && dm.Height == _height)
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        Device.PreferredBackBufferWidth = _width;
                        Device.PreferredBackBufferHeight = _height;
                        Device.IsFullScreen = _fullScreen;
                        Device.ApplyChanges();
                        break;
                    }
                }
            }

            _width = Device.PreferredBackBufferWidth;
            _height = Device.PreferredBackBufferHeight;
        }
    }
}
