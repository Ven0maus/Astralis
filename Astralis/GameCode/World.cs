using Astralis.Configuration;
using Astralis.Configuration.Models;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
{
    internal class World : GridBase<byte, Tile, WorldChunk>
    {
        public Tile this[Point pos] => GetCell(pos.X, pos.Y);
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, Width)];

        private readonly int _seed;

        /// <summary>
        /// Contains all data representing how to generate the world
        /// </summary>
        public static readonly WorldGeneration GenerationData = GameConfiguration.Load<WorldGeneration>();
        private readonly WorldGenerator _generator;

        public World(int width, int height, WorldGenerator generator)
            : base(width, height, chunkWidth: 200, chunkHeight: 200, generator)
        {
            _generator = generator;
            _seed = generator.Seed;
        }

        protected override Tile Convert(int x, int y, byte cellType)
        {
            var tile = base.Convert(x, y, cellType);
            if (_generator == null) return null;

            var chunkCoordinate = GetChunkCoordinate(x, y);
            var chunkData = GetChunkData(chunkCoordinate.x, chunkCoordinate.y);

            // Set biome color
            tile.Background = chunkData.Colors[(y - chunkCoordinate.y) * chunkData.Width + (x - chunkCoordinate.x)];

            return tile;
        }
    }
}
