using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.WorldGen;
using SadConsole;
using SadConsole.Components;
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
        private bool _isDragging = false;

        private readonly FontWindow _fontWindow;
        private readonly SmoothMove _smoothMove;

        public Point WorldSourceFontSize { get { return Font.GetFontSize(IFont.Sizes.One); } }
        public Point CameraPosition { get; private set; } = new(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
        public bool IsMoving { get { return _smoothMove.IsMoving; } }

        /// <summary>
        /// If true will auto pan the camera into one direction forever, to be shown on the main menu.
        /// </summary>
        public bool MainMenuCamera { get; set; } = false;

        private readonly InventoryScreen _inventoryScreen;

        public WorldScreen(World world, bool isMainMenu) : base(world.Width, world.Height)
        {
            Position = new Point(-1, -1);
            // Set 16x16 font for the overworld
            Font = Game.Instance.Fonts[Constants.Fonts.WorldFonts.WorldFont];

            // Make world zoom in
            var zoomFactor = isMainMenu ? 1f : Constants.WorldGeneration.WorldZoomFactor;
            FontSize = new Point((int)(Font.GlyphWidth * zoomFactor), (int)(Font.GlyphHeight * zoomFactor));

            // Resize world screen to fit the new fontsize
            if (zoomFactor != 1f)
            {
                Point newSize = ((int)Math.Ceiling(Width / Constants.WorldGeneration.WorldZoomFactor) + 2, (int)Math.Ceiling(Height / Constants.WorldGeneration.WorldZoomFactor) + 2);
                world.ResizeViewport(newSize.X, newSize.Y);
                Resize(newSize.X, newSize.Y, false);
            }
            else
            {
                var newSize = new Point(Width + 2, Height + 2);
                world.ResizeViewport(newSize.X, newSize.Y);
                Resize(newSize.X, newSize.Y, false);
            }

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

            // Add smooth move for player movement
            _smoothMove = new SmoothMove(TimeSpan.FromMilliseconds(Constants.PlayerData.SmoothMoveTransition));
            SadComponents.Add(_smoothMove);

            _inventoryScreen = new InventoryScreen();
            Children.Add(_inventoryScreen);
        }

        public void DisableSmoothMove()
        {
            if (_smoothMove.IsMoving) return;
            SadComponents.Remove(_smoothMove);
        }

        public void EnableSmoothMove()
        {
            if (SadComponents.Contains(_smoothMove)) return;
            SadComponents.Add(_smoothMove);
        }

        private readonly CellDecorator[] _objectArrayCache = new CellDecorator[1];
        public void OnCellUpdate(object sender, CellUpdateArgs<byte, Tile> args)
        {
            if (Width != _world.Width) return;

            var cell = args.Cell;
            int index = args.ScreenY * Width + args.ScreenX;

            if (cell == null)
            {
                Surface[index].IsVisible = false;
                return;
            }

            Surface[index].CopyAppearanceFrom(cell, false);
            Surface[index].IsVisible = cell.IsVisible;

            if (cell.Object != null)
            {
                if (Surface[index].Decorators != null && Surface[index].Decorators.Count > 0)
                    Surface.ClearDecorators(index, 1);

                if (cell.Object.IsVisible)
                {
                    _objectArrayCache[0] = new CellDecorator(cell.Object.Foreground, cell.Object.Glyph, cell.Object.Mirror);
                    Surface.AddDecorator(index, _objectArrayCache);
                }
            }
            else
            {
                if (Surface[index].Decorators != null && Surface[index].Decorators.Count > 0)
                    Surface.ClearDecorators(index, 1);
            }

            Surface.IsDirty = true;
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

            foreach (var entity in GameplayScene.Instance.EntityManager.EntityComponent)
            {
                if (entity.UseKeyboard)
                    entity.ProcessKeyboard(keyboard);
            }

            return base.ProcessKeyboard(keyboard);
        }

        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (MainMenuCamera || !IsVisible || !state.IsOnScreenObject)
                return false;

            Point mousePos = state.CellPosition;

            if (state.Mouse.LeftButtonDown && !_isDragging)
            {
                // Mouse button is pressed, start tracking the drag operation
                _isDragging = true;
                startDragPos = mousePos;
            }

            if (_isDragging)
            {
                // Do camera movement
                MoveCamera(startDragPos, mousePos);

                // Update the starting position to the current mouse position
                startDragPos = mousePos;
            }

            if (!state.Mouse.LeftButtonDown && _isDragging)
            {
                // Mouse button was released after dragging, stop tracking the drag operation
                _isDragging = false;
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

            // Set player position on screen where he is on the camera
            var player = GameplayScene.Instance.Player;
            if (player != null)
            {
                if (_world.IsWorldCoordinateOnViewPort(player.WorldPosition))
                {
                    var screenCoordinate = _world.WorldToScreenCoordinate(player.WorldPosition);
                    player.SmoothMoveEnabled = false;
                    player.Position = screenCoordinate;
                    player.SmoothMoveEnabled = true;
                    player.IsVisible = true;
                }
                else
                {
                    player.IsVisible = false;
                }
            }
        }

        internal void SetCameraPosition(Point position)
        {
            if (_isDragging) return;
            CameraPosition = position;
        }
    }
}
