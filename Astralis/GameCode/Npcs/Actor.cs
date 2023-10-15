using Astralis.Configuration.Models;
using Astralis.Extended;
using Astralis.Scenes;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Astralis.GameCode.Npcs
{
    internal class Actor : Entity
    {
        public event EventHandler<PositionChangedArgs> OnWorldPositionChanged;
        public ObservableCollection<NpcTrait> Traits { get; private set; }
        public Facing Facing { get; private set; }
        public Gender Gender { get; private init; }
        public Race Race { get; private init; }
        public Class Class { get; private init; }
        public Point WorldPosition { get; protected set; }
        public bool IsMoving { get { return _smoothMove.IsMoving; } }

        private readonly int _forwardGlyph;
        protected readonly SmoothMove _smoothMove;

        public Actor(Point worldPosition, int forwardGlyph, Gender gender, Race race, Class @class, IEnumerable<NpcTrait> traits = null, int zIndex = 1)
            : base(Color.White, Color.Transparent, forwardGlyph, zIndex)
        {
            WorldPosition = worldPosition;
            Gender = gender;
            Race = race;
            Class = @class;
            Traits = new ObservableCollection<NpcTrait>(traits ?? Enumerable.Empty<NpcTrait>());

            _forwardGlyph = forwardGlyph;

            // If the world position falls within the screenview, we update the entity position
            var worldPositionOnScreen = GameplayScene.Instance.World.IsWorldCoordinateOnViewPort(WorldPosition);
            if (worldPositionOnScreen)
            {
                var screenCoordinate = GameplayScene.Instance.World.WorldToScreenCoordinate(WorldPosition);
                SetPosition(screenCoordinate);

                // In screen, make visible
                IsVisible = true;
            }
            else
            {
                // Not in screen, make invisible
                IsVisible = false;
            }

            _smoothMove = new SmoothMove(GameplayScene.Instance.WorldFontSize, this is Player ? TimeSpan.FromMilliseconds(Constants.PlayerData.SmoothMoveTransition) : TimeSpan.FromMilliseconds(200));
            SadComponents.Add(_smoothMove);

            UseMouse = false;
            UseKeyboard = false;
        }

        public void MoveTowards(Direction dir, bool checkCollision = true)
        {
            MoveTowards(WorldPosition + dir, false, checkCollision);
        }

        public void MoveTowards(Point pos, bool canTeleport = false, bool checkCollision = true)
        {
            if (WorldPosition == pos || _smoothMove.IsMoving) return;
            SetFacing(CalculateFacingDirection(WorldPosition, pos));
            if (checkCollision && !CanMoveTowards(pos, canTeleport)) return;

            var oldWorldPos = WorldPosition;
            WorldPosition = pos;
            OnWorldPositionChanged?.Invoke(this, new PositionChangedArgs(oldWorldPos, pos));

            // If the world position falls within the screenview, we update the entity position
            var worldPositionOnScreen = GameplayScene.Instance.World.IsWorldCoordinateOnViewPort(WorldPosition);
            if (worldPositionOnScreen)
            {
                var screenCoordinate = GameplayScene.Instance.World.WorldToScreenCoordinate(WorldPosition);
                SetPosition(screenCoordinate);

                // In screen, make visible
                IsVisible = true;
            }
            else
            {
                // Not in screen, make invisible
                IsVisible = false;
            }
        }

        protected virtual void SetPosition(Point @new)
        {
            Position = @new;
        }

        public virtual bool CanMoveTowards(Point @new, bool canTeleport = false)
        {
            if (_smoothMove.IsMoving) return false;
            return IsPositionValid(WorldPosition, @new, canTeleport);
        }

        public bool CanMoveTowards(Direction dir)
        {
            return IsPositionValid(WorldPosition, WorldPosition + dir);
        }

        private static bool IsPositionValid(Point old, Point @new, bool canTeleport = false)
        {
            var moveOneTileOnly = true;
            if (!canTeleport)
            {
                int deltaX = Math.Abs(@new.X - old.X);
                int deltaY = Math.Abs(@new.Y - old.Y);

                moveOneTileOnly = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
            }
            var targetCell = GameplayScene.Instance.World.GetCell(@new.X, @new.Y);
            return moveOneTileOnly && targetCell.Walkable;
        }

        public static Point FindSuitableSpawnLocation(Point startSearchPosition)
        {
            int distance = 1;
            const int searchRadius = 3;

            while (true)
            {
                for (int dx = -distance; dx <= distance; dx++)
                {
                    for (int dy = -distance; dy <= distance; dy++)
                    {
                        Point pos = (startSearchPosition.X + dx, startSearchPosition.Y + dy);

                        bool valid = true;
                        var radiusPositions = pos.GetCirclePositions(searchRadius);
                        foreach (var position in radiusPositions)
                        {
                            if (!IsPositionValid(startSearchPosition, position, true))
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (valid)
                        {
                            return pos;
                        }
                    }
                }

                distance++; // Increase the search distance if no suitable spot is found.
            }
        }

        private static Facing CalculateFacingDirection(Point old, Point @new)
        {
            int deltaX = @new.X - old.X;
            int deltaY = @new.Y - old.Y;

            if (deltaX == 1 && deltaY == 0)
            {
                return Facing.Right;
            }
            else if (deltaX == -1 && deltaY == 0)
            {
                return Facing.Left;
            }
            else if (deltaX == 0 && deltaY == 1)
            {
                return Facing.Forward;
            }
            else if (deltaX == 0 && deltaY == -1)
            {
                return Facing.Backwards;
            }
            else
            {
                // When facing cannot be determined (eg, teleportation) then we just set to forward.
                return Facing.Forward;
            }
        }

        private void SetFacing(Facing facing)
        {
            if (Facing == facing) return;
            Facing = facing;

            switch (Facing)
            {
                case Facing.Forward:
                    AppearanceSingle.Appearance.Glyph = _forwardGlyph;
                    break;
                case Facing.Backwards:
                    AppearanceSingle.Appearance.Glyph = _forwardGlyph + 2;
                    break;
                case Facing.Left:
                    AppearanceSingle.Appearance.Glyph = _forwardGlyph + 1;
                    AppearanceSingle.Appearance.Mirror = Mirror.None;
                    break;
                case Facing.Right:
                    AppearanceSingle.Appearance.Glyph = _forwardGlyph + 1;
                    AppearanceSingle.Appearance.Mirror = Mirror.Horizontal;
                    break;
            }

            IsDirty = true;
        }

        public class PositionChangedArgs : EventArgs
        {
            public Point Old { get; }
            public Point New { get; }

            public PositionChangedArgs(Point old, Point @new)
            {
                Old = old;
                New = @new;
            }
        }
    }
}
