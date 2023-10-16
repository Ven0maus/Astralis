using Astralis.Configuration.Models;
using Astralis.Scenes;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Astralis.GameCode.Npcs
{
    internal class Player : Actor
    {
        public static Player Instance { get; private set; }
        private Direction _lastMovement;
        private bool _movementCompleted = false, _canMove = true;

        private Point _playerBasePosition;

        private bool _smoothMoveEnabled = true;
        public bool SmoothMoveEnabled 
        {
            get { return _smoothMoveEnabled; }
            set 
            {  
                if (_smoothMoveEnabled != value)
                {
                    _smoothMoveEnabled = value;
                    if (_smoothMoveEnabled)
                    {
                        SadComponents.Add(_smoothMove);
                    }
                    else
                    {
                        SadComponents.Remove(_smoothMove);
                    }
                }
            }
        }

        public Player(Point worldPosition, Gender gender, Race race, Class @class, IEnumerable<NpcTrait> traits)
            : base(worldPosition, Constants.PlayerData.PlayerForwardGlyph, gender, race, @class, traits)
        {
            Instance = this;
            UseKeyboard = true;
            _smoothMove.MoveEnded += OnMovementCompleted;
            IsVisible = true;
            OnWorldPositionChanged += ChangeWorldPosition;

            // Set starting position with no smooth movement
            SmoothMoveEnabled = false;
            base.SetPosition(_playerBasePosition = new Point(GameplayScene.Instance.World.Width / 2, GameplayScene.Instance.World.Height / 2));
            SmoothMoveEnabled = true;
        }

        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            // When the character finished its movement animation, wait until surface is done  too
            if (_movementCompleted && !GameplayScene.Instance.Display.IsMoving)
            {
                // Adjust center position of FlowVitae
                GameplayScene.Instance.World.Center(WorldPosition);

                // Reset position of the sadconsole surface
                GameplayScene.Instance.Display.DisableSmoothMove();
                GameplayScene.Instance.Display.Position += _lastMovement;
                GameplayScene.Instance.Display.EnableSmoothMove();

                // Adjust camera position to match FlowVitae
                GameplayScene.Instance.Display.SetCameraPosition(WorldPosition);

                // Set player back to the center without smooth movement
                SmoothMoveEnabled = false;
                Position -= _lastMovement;
                SmoothMoveEnabled = true;

                _movementCompleted = false;
                _canMove = true;
            }
        }

        private void OnMovementCompleted(object sender, EventArgs e)
        {
            _movementCompleted = true;
        }

        public void ResetCameraPosition()
        {
            SmoothMoveEnabled = false;
            Position = _playerBasePosition;
            SmoothMoveEnabled = true;

            // Fix camera view
            GameplayScene.Instance.World.Center(WorldPosition);
            GameplayScene.Instance.Display.SetCameraPosition(WorldPosition);
        }

        private void ChangeWorldPosition(object sender, EventArgs args)
        {
            if (Position != _playerBasePosition)
                ResetCameraPosition();
        }

        /// <summary>
        /// Player always stays centered, the world moves instead.
        /// </summary>
        /// <param name="new"></param>
        protected override void SetPosition(Point @new)
        {
            var prev = Position;
            int deltaX = Math.Abs(@new.X - prev.X);
            int deltaY = Math.Abs(@new.Y - prev.Y);
            var moveOneTileOnly = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
            if (!moveOneTileOnly) return;

            var direction = Direction.GetDirection(prev, @new);
            _lastMovement = direction;
            Position += direction;
            GameplayScene.Instance.Display.Position -= direction; // Move in opposite direction
            _canMove = false;
        }

        /// <summary>
        /// Puts the player nearby with enough space, like an unstuck function.
        /// </summary>
        public void AdjustWorldPositionToValidLocation()
        {
            // Calculate a suitable position to spawn the player
            WorldPosition = FindSuitableSpawnLocation(WorldPosition);
            GameplayScene.Instance.World.Center(WorldPosition);
            GameplayScene.Instance.Display.SetCameraPosition(WorldPosition);
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            foreach (var key in _playerMovements.Keys)
            {
                if (keyboard.IsKeyPressed(key))
                {
                    var moveDirection = _playerMovements[key];
                    MoveTowards(moveDirection);
                    return true;
                }
            }

            return base.ProcessKeyboard(keyboard);
        }

        public override bool CanMoveTowards(Point @new, bool canTeleport = false)
        {
            if (!_canMove) return false;
            return base.CanMoveTowards(@new, canTeleport);
        }

        private readonly Dictionary<Keys, Direction> _playerMovements =
            new()
        {
            {Keys.Z, Direction.Up},
            {Keys.S, Direction.Down},
            {Keys.Q, Direction.Left},
            {Keys.D, Direction.Right}
        };
    }
}
