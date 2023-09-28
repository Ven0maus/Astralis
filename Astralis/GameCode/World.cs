using Astralis.Configuration;
using Astralis.Configuration.Models;
using SadConsole;
using SadRogue.Primitives;
using System;
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

        private readonly int _seed;
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
            tile.Glyph = '▓';
            tile.Background = chunkData.GetBiomeColor(x, y);
            var obj = (ObjectType)chunkData.GetObject(x, y);
            if (obj != ObjectType.None)
            {
                var decorator = GetObject(obj, true);
                if (decorator != null)
                {
                    tile.Decorators = new[] { decorator.Value };
                }
            }

            return tile;
        }

        private static CellDecorator? GetObject(ObjectType objectType, bool throwExceptionOnMissingConfiguration = true)
        {
            if (!ObjectData.Get.Objects.TryGetValue(objectType, out var value))
            {
                if (throwExceptionOnMissingConfiguration)
                    throw new Exception("Missing world object configuration: " + Enum.GetName(objectType));
                return null;
            }
            return new CellDecorator(value.Color, value.Glyph, Mirror.None);
        }
    }
}
