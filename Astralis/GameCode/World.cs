using Astralis.Configuration;
using Astralis.Configuration.Models;
using Astralis.Extended;
using SadConsole;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
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
        }

        protected override Tile Convert(int x, int y, byte cellType)
        {
            var tile = base.Convert(x, y, cellType);
            if (_generator == null) return null;

            var chunkCoordinate = GetChunkCoordinate(x, y);
            var chunkData = GetChunkData(chunkCoordinate.x, chunkCoordinate.y);

            // Set biome color
            tile.Glyph = '▓';
            tile.Background = chunkData.GetBiomeColor(x, y);
            var obj = chunkData.GetObject(x, y);
            if (obj != null)
            {
                tile.Decorators = new[] { new CellDecorator(obj.Color, obj.Glyphs.Random(chunkData.Random), Mirror.None) };
            }

            return tile;
        }
    }
}
