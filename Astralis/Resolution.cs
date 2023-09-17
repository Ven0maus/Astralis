using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astralis
{
    internal static class Resolution
    {
        private static GraphicsDeviceManager _Device = null;
        private static int _width, _height;
        private static bool _fullScreen = false;

        public static void Init(GraphicsDeviceManager device)
        {
            _Device = device;
        }

        public static void SetResolution(int width, int height, bool fullScreen)
        {
            _width = width;
            _height = height;
            _fullScreen = fullScreen;
            ApplyResolutionSettings();
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
                    _Device.PreferredBackBufferWidth = _width;
                    _Device.PreferredBackBufferHeight = _height;
                    _Device.IsFullScreen = _fullScreen;
                    _Device.ApplyChanges();
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
                        _Device.PreferredBackBufferWidth = _width;
                        _Device.PreferredBackBufferHeight = _height;
                        _Device.IsFullScreen = _fullScreen;
                        _Device.ApplyChanges();
                        break;
                    }
                }
            }

            _width = _Device.PreferredBackBufferWidth;
            _height = _Device.PreferredBackBufferHeight;
        }
    }
}
