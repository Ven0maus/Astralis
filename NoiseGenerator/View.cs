using Astralis;
using Astralis.Extended;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;

namespace NoiseGenerator
{
    internal class View : ControlsConsole
    {
        private readonly Panel _panel;
        private Point startDragPos;
        private bool isDragging = false;

        private Point _cameraPosition;
        public Point CameraPosition
        {
            get { return _cameraPosition; }
            set 
            { 
                _cameraPosition = value;
                _panel.SetOffset(value);
            }
        }

        public View(int width, int height) : base(width, height)
        {
            Font = Game.Instance.Fonts[Constants.Fonts.LCD];

            _panel = new Panel();
            _panel.OnGenerate += (sender, args) => UpdateNoise(args);
            _panel.OnResetOffset += (sender, args) => CameraPosition = (0, 0);
            _panel.InvokeGenerate();
            Children.Add(_panel);
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.P))
            {
                _panel.IsVisible = !_panel.IsVisible;
                return true;
            }
            return base.ProcessKeyboard(keyboard);
        }

        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (!IsVisible || !state.IsOnScreenObject)
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
        }

        private void UpdateNoise(Panel.GenerateArgs args)
        {
            NoiseHelper noise = new(args.Seed);
            noise.SetNoiseType(args.NoiseType);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float xValue = x + args.Offset.X;
                    float yValue = y + args.Offset.Y;
                    float value = noise.GetNoise(xValue, yValue, args.Octaves, args.Scale, args.Persistence, args.Lacunarity, !args.SharpRidges);
                    
                    if (args.SharpRidges)
                        value = Mathf.Remap(-1 * Math.Abs(value), -1f, 1f, 0f, 1f);
                    
                    Surface.SetBackground(x, y, Color.Lerp(Color.White, Color.Black, value));
                }
            }
            Surface.IsDirty = true;
        }
    }
}
