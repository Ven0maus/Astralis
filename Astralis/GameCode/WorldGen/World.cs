using Astralis.Configuration;
using Astralis.Configuration.Models;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode.WorldGen
{
    internal class World : GridBase<byte, Tile, WorldChunk>
    {
        public Tile this[Point pos] => GetCell(pos.X, pos.Y);
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, Width)];

        /// <summary>
        /// Contains all data that is used for biome generation
        /// </summary>
        public static readonly BiomeGeneration BiomeData = GameConfiguration.Load<BiomeGeneration>();
        /// <summary>
        /// Contains all data that is used to spawn objects
        /// </summary>
        public static readonly WorldObjects ObjectData = GameConfiguration.Load<WorldObjects>();

        public readonly int Seed;
        private readonly WorldGenerator _generator;

        public World(int width, int height, int chunkWidth, int chunkHeight, WorldGenerator generator)
            : base(width, height, chunkWidth, chunkHeight, generator, Constants.WorldGeneration.ExtraChunkRadius)
        {
            _generator = generator;
            Seed = generator.Seed;

            // Chunks are generated based on elevation, moisture, heat noise maps
            // which are combined into one map (additive), this means all these values
            // are no longer within 0-1 range, to remap this range we need to use
            // globalmin/globalmax values that are dynamically updated when new chunks are loaded.
            // The initial calculation is made during world init, which pre-load chunks.
            // But since these were made during the computation, it could be some chunks have visual artifacts
            // We clear the cache so the chunks are reloaded based on the new computations.
            ClearCache();
        }

        protected override Tile Convert(int x, int y, byte cellType)
        {
            var tile = base.Convert(x, y, cellType);
            if (_generator == null) return null;

            var chunkCoordinate = GetChunkCoordinate(x, y);
            var chunkData = GetChunkData(chunkCoordinate.x, chunkCoordinate.y);

            // Set biome color
            tile.Background = chunkData.GetBiomeColor(x, y);

            // Set object data
            var obj = WorldChunk.GetObjectTileById(chunkData.GetObject(x, y));
            if (obj != null)
            {
                tile.Object = obj;
            }
            return tile;
        }
    }
}
