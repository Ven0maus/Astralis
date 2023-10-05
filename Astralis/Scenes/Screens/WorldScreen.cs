using Astralis.Extended.SadConsole;
using Astralis.GameCode.WorldGen;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using Venomaus.FlowVitae.Grids;

namespace Astralis.Scenes.Screens
{
    internal class WorldScreen : ScreenSurface
    {
        private readonly World _world;
        private Point startDragPos;
        private bool isDragging = false;

        private readonly FontWindow _fontWindow;
        private readonly ScreenSurface _objectsLayer;

        public Point CameraPosition { get; private set; } = new(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);

        /// <summary>
        /// If true will auto pan the camera into one direction forever, to be shown on the main menu.
        /// </summary>
        public bool MainMenuCamera { get; set; } = false;

        public WorldScreen(World world) : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            // Set 16x16 font for the overworld
            Font = Game.Instance.Fonts[Constants.Fonts.LCD];

            // Make world zoom in
            var zoomFactor = Constants.WorldGeneration.WorldZoomFactor;
            FontSize = new Point((int)(Font.GlyphWidth * zoomFactor), (int)(Font.GlyphHeight * zoomFactor));

            _objectsLayer = new ScreenSurface(Width, Height)
            {
                Font = Game.Instance.Fonts[Constants.Fonts.WorldObjects],
                FontSize = FontSize,
                UseMouse = false,
                UseKeyboard = false
            };
            foreach (var cell in _objectsLayer.Surface)
                cell.IsVisible = false;

            _objectsLayer.Surface.IsDirty = true;
            Children.Add(_objectsLayer);

            _fontWindow = new FontWindow(Font);
            Children.Add(_fontWindow);

            _fontWindow.OnClick += (sender, index) => System.Console.WriteLine($"Char ({index}) '{(char)index}'");
            _fontWindow.DrawFontSurface();
            _fontWindow.IsVisible = Constants.DebugMode && !MainMenuCamera;

            // Setup world object
            _world = world;
            _world.OnCellUpdate += OnCellUpdate;
            _world.UpdateScreenCells();

            // Set screen properties
            UseMouse = true;
            IsFocused = true;
        }

        public ScreenSurface[] GetSurfaces()
        {
            return new[] { this, _objectsLayer };
        }

        public void OnCellUpdate(object sender, CellUpdateArgs<byte, Tile> args)
        {
            var surface = Surface;
            var cell = args.Cell;
            if (cell == null)
            {
                surface[args.ScreenX, args.ScreenY].IsVisible = false;
                return;
            }

            surface[args.ScreenX, args.ScreenY].CopyAppearanceFrom(cell, false);
            surface[args.ScreenX, args.ScreenY].IsVisible = cell.IsVisible;
            surface.IsDirty = true;

            if (cell.Object != null)
            {
                _objectsLayer.Surface[args.ScreenX, args.ScreenY].CopyAppearanceFrom(cell.Object, false);
                _objectsLayer.Surface[args.ScreenX, args.ScreenY].IsVisible = cell.IsVisible;
                _objectsLayer.Surface.IsDirty = true;
            }
            else
            {
                var prev = _objectsLayer.Surface[args.ScreenX, args.ScreenY].IsVisible;
                _objectsLayer.Surface[args.ScreenX, args.ScreenY].IsVisible = false;

                if (prev != false)
                    _objectsLayer.Surface.IsDirty = true;
            }
        }

        private double _timePassed;
        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            if (MainMenuCamera)
            {
                if (_timePassed >= 250)
                {
                    _timePassed = 0;
                    MoveCamera(CameraPosition, CameraPosition + Direction.Right);
                }
                else
                {
                    _timePassed += delta.TotalMilliseconds;
                }
            }
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (MainMenuCamera) return false;
            if (keyboard.IsKeyPressed(Keys.P))
            {
                _fontWindow.IsVisible = !_fontWindow.IsVisible;
            }
            return base.ProcessKeyboard(keyboard);
        }

        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (MainMenuCamera || !IsVisible || !state.IsOnScreenObject)
                return false;

            Point mousePos = state.CellPosition;

            if (state.Mouse.LeftButtonDown && !isDragging)
            {
                // Mouse button is pressed, start tracking the drag operation
                isDragging = true;
                startDragPos = mousePos;
            }

            if (isDragging)
            {
                // Do camera movement
                MoveCamera(startDragPos, mousePos);

                // Update the starting position to the current mouse position
                startDragPos = mousePos;
            }

            if (!state.Mouse.LeftButtonDown && isDragging)
            {
                // Mouse button was released after dragging, stop tracking the drag operation
                isDragging = false;
            }

            // If not dragging, call the base method to handle other mouse events
            return base.ProcessMouse(state);
        }

        private void MoveCamera(Point startPosition, Point targetPosition)
        {
            // Calculate the distance the mouse was dragged
            int deltaX = startPosition.X - targetPosition.X;
            int deltaY = startPosition.Y - targetPosition.Y;

            // Apply a scaling factor to control camera speed
            deltaX = -1 * deltaX;
            deltaY = -1 * deltaY;

            // Apply smoothing to camera movement
            var cameraVelocity = new Point(deltaX, deltaY);

            // Update the camera position based on velocity
            CameraPosition -= cameraVelocity;

            // Move the viewport based on the camera position
            _world.Center(CameraPosition.X, CameraPosition.Y);
        }
    }
}
