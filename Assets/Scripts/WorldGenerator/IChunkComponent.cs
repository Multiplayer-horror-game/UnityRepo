using UnityEngine;

namespace Fase1
{
    public interface IChunkComponent
    {
        public void ImplementChildren(WorldGenerator worldGenerator, GameObject parent, int xChunk, int yChunk);
    }
}