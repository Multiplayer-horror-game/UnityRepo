using System;
using System.Collections.Generic;
using Fase1.MeshComponents;
using UnityEngine;

namespace Fase1
{
    public interface IMeshComponent
    {
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition,int verticesCount,float physicalSize);
        
    }
}
