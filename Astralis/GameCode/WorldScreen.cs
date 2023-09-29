using Astralis.Extended.SadConsole;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
{
    internal class WorldScreen : ScreenSurface
    {
        private readonly World _world;
        private Point startDragPos, cameraPosition = new(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
        private bool isDragging = false;

        private readonly FontWindow _fontWindow;

        public WorldScreen(World world) : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            // Set Aesomatica font for the overworld
            Font = Game.Instance.Fonts[Constants.Fonts.Aesomatica];

            _fontWindow = new FontWindow(Font);
            _fontWindow.OnClick += (sender, index) => System.Console.WriteLine($"Char ({index}) '{(char)index}'");
            Children.Add(_fontWindow);
            _fontWindow.DrawFontSurface();
            _fontWindow.IsVisible = Constants.DebugMode;

            // Setup world object
            _world = world;
            _world.OnCellUpdate += OnCellUpdate;

            // Set screen properties
            UseMouse = true;
            IsFocused = true;
        }

        public void OnCellUpdate(object sender, CellUpdateArgs<byte, Tile> args)
        {
            var surface = Surface;
            surface[args.ScreenX, args.ScreenY].CopyAppearanceFrom(args.Cell);
            surface[args.ScreenX, args.ScreenY].IsVisible = args.Cell.IsVisible;
            surface.IsDirty = true;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.P))
            {
                _fontWindow.IsVisible = !_fontWindow.IsVisible;
            }
            return base.ProcessKeyboard(keyboard);
        }

        private Point initialCameraPosition;
        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (!IsVisible || !state.IsOnScreenObject)
                return base.ProcessMouse(state);

            Point mousePos = state.CellPosition;

            if (state.Mouse.LeftButtonDown && !isDragging)
            {
                // Mouse button is pressed, start tracking the drag operation
                isDragging = true;
                startDragPos = mousePos;
                initialCameraPosition = cameraPosition;
            }

            if (isDragging)
            {
                // Calculate the distance the mouse was dragged
                int deltaX = startDragPos.X - mousePos.X;
                int deltaY = startDragPos.Y - mousePos.Y;

                if (deltaX != 0 || deltaY != 0)
                {
                    // Apply a scaling factor to control camera speed
                    deltaX = -1 * deltaX;
                    deltaY = -1 * deltaY;

                    // Apply smoothing to camera movement
                    var cameraVelocity = new Point(deltaX, deltaY);

                    // Check if the camera has reached the same distance as the mouse drag
                    if (Math.Abs(cameraPosition.X - initialCameraPosition.X) < Math.Abs(startDragPos.X - mousePos.X) &&
                        Math.Abs(cameraPosition.Y - initialCameraPosition.Y) < Math.Abs(startDragPos.Y - mousePos.Y))
                    {
                        // Update the camera position based on velocity
                        cameraPosition -= cameraVelocity;

                        // Move the viewport based on the camera position
                        _world.Center(cameraPosition.X, cameraPosition.Y);
                    }
                    else
                    {
                        // Reached position, reset
                        initialCameraPosition = cameraPosition;
                        startDragPos = mousePos;
                    }
                }
            }

            if (!state.Mouse.LeftButtonDown && isDragging)
            {
                // Mouse button was released after dragging, stop tracking the drag operation
                isDragging = false;
            }

            // If not dragging, call the base method to handle other mouse events
            return base.ProcessMouse(state);
        }
    }
}
