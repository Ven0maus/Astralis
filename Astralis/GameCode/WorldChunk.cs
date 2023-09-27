using Venomaus.FlowVitae.Chunking;

namespace Astralis.GameCode
{
    internal class WorldChunk : IChunkData
    {
        public int Seed { get; set; }
        public (int x, int y) ChunkCoordinate { get; set; }

        public readonly float[] Elevation, Moisture;

        public WorldChunk(float[] elevation, float[] moisture)
        {
            Elevation = elevation;
            Moisture = moisture;
        }
    }
}
