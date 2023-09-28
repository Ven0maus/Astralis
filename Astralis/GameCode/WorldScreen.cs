using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
{
    internal class WorldScreen : ScreenSurface
    {
        private readonly World _world;
        private Point startDragPos, cameraPosition = new(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
        private bool isDragging = false;
        private const float cameraMoveSpeed = 0.3f;

        public WorldScreen(World world) : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            // Set Aesomatica font for the overworld
            Font = Game.Instance.Fonts[Constants.Fonts.Aesomatica];

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
            }

            if (isDragging)
            {
                // Calculate the distance the mouse was dragged
                int deltaX = startDragPos.X - mousePos.X;
                int deltaY = startDragPos.Y - mousePos.Y;

                if (deltaX != 0 || deltaY != 0)
                {
                    // Apply a scaling factor to control camera speed
                    deltaX = (int)(deltaX * cameraMoveSpeed);
                    deltaY = (int)(deltaY * cameraMoveSpeed);

                    // Apply smoothing to camera movement
                    var cameraVelocity = new Point(deltaX, deltaY); ;

                    // Update the camera position based on velocity
                    cameraPosition += cameraVelocity;

                    // Move the viewport based on the camera position
                    _world.Center(cameraPosition.X, cameraPosition.Y);

                    // Output camera position for debugging
                    System.Console.WriteLine($"Camera Position: {cameraPosition.X}, {cameraPosition.Y}");
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
