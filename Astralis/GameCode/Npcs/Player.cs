using Astralis.Configuration.Models;
using Astralis.Scenes;
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
            OnWorldPositionChanged += Player_OnWorldPositionChanged;
            base.SetPosition(new Point(GameplayScene.Instance.World.Width / 2, GameplayScene.Instance.World.Height / 2));
        }

        private void Player_OnWorldPositionChanged(object sender, PositionChangedArgs e)
        {
            GameplayScene.Instance.World.Center(WorldPosition.X, WorldPosition.Y);
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
                GameplayScene.Instance.World.Center(WorldPosition.X, WorldPosition.Y);
        }
    }
}
