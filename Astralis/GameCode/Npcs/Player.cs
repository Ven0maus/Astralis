using Astralis.Configuration.Models;
using Astralis.Scenes;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;

namespace Astralis.GameCode.Npcs
{
    internal class Player : Actor
    {
        public static Player Instance { get; private set; }

        public Player(Point worldPosition, Gender gender, Race race, Class @class, IEnumerable<NpcTrait> traits)
            : base(worldPosition, Constants.PlayerData.PlayerForwardGlyph, gender, race, @class, traits)
        {
            Instance = this;
            UseKeyboard = true;
            OnWorldPositionChanged += Player_OnWorldPositionChanged;

            // TODO: Implement custom smooth move for player
            // The sadconsole surface needs to lerp its position using pixelpositions
            // Then when the lerp is finished, the flowvitae grid should center on the new location
            // The sadconsole surface should position back into the original position
            // This means the worldscreen should be atleast 2x2 larger (1 in each direction)
            // And ofcourse the world should match the worldscreen so it can render it.
            SadComponents.Remove(_smoothMove);

            base.SetPosition(new Point(GameplayScene.Instance.World.Width / 2, GameplayScene.Instance.World.Height / 2));
        }

        private void Player_OnWorldPositionChanged(object sender, PositionChangedArgs e)
        {
            GameplayScene.Instance.World.Center(WorldPosition);
            GameplayScene.Instance.SetCameraPosition(WorldPosition);
        }

        /// <summary>
        /// Player always stays centered, the world moves instead.
        /// </summary>
        /// <param name="new"></param>
        protected override void SetPosition(Point @new)
        { }

        /// <summary>
        /// Puts the player nearby with enough space, like an unstuck function.
        /// </summary>
        public void AdjustWorldPositionToValidLocation()
        {
            // Calculate a suitable position to spawn the player
            var position = FindSuitableSpawnLocation(WorldPosition);
            var prev = WorldPosition;
            MoveTowards(position, true);
            // Extra center, in case the position happens to be the same as initial position
            if (prev == position)
                GameplayScene.Instance.World.Center(WorldPosition);
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
