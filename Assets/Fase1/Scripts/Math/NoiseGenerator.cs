using Fase1.Math;
using UnityEngine;

namespace Fase1
{
    public class NoiseGenerator
    {
        protected float Scale;
        protected int xOffset;
        protected int yOffset;
        protected int vertices;
        protected float heightMultiplier;
        protected float heightOffset;
        protected int physicalSize;

        public NoiseGenerator(float scale, int xOffset, int yOffset, int vertices, float heightMultiplier, float heightOffset, int physicalSize)
        {
            this.Scale = scale;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.vertices = vertices;
            this.heightMultiplier = heightMultiplier;
            this.heightOffset = heightOffset;
            this.physicalSize = physicalSize;
        }

        public float[,] GenerateNoiseChunk(int xChunk, int yChunk)
        {
            return GenerateNoiseChunk(xChunk, yChunk, this.xOffset, this.yOffset);
        }


        public float[,] GenerateNoiseChunk(int xChunk, int yChunk, float _xOffset, float _yOffset)
        {
            return GenerateNoiseChunk(xChunk, yChunk, _xOffset, _yOffset, vertices);
        }
        
        private float[,] GenerateNoiseChunk(int xChunk, int yChunk, float _xOffset, float _yOffset, int verts)
        {

            var noiseMap = new float[verts,verts];

            float vDistance = physicalSize / verts;
            for (int x = 0; x < verts; x++)
            {
                for (int y = 0; y < verts; y++)
                {

                    noiseMap[x, y] = GetNoiseValue((xChunk * physicalSize) + x * vDistance, (yChunk * physicalSize) + y * vDistance, _xOffset, _yOffset);

                }
            }

            return noiseMap;
        }

        public float GetNoiseValue(float x, float y)
        {
            
            return GetNoiseValue(x,y, xOffset, yOffset);
        }
        
        public float GetNoiseValue(float x, float y, float xOffset, float yOffset)
        {
            
            return Mathf.PerlinNoise(x * Scale + xOffset, y * Scale + yOffset) * heightMultiplier - heightOffset;
        }

        
        public NatureNoiseGenerator ConvertToNatureNoiseGenerator()
        {
            return new NatureNoiseGenerator(Scale,xOffset, yOffset,vertices, heightMultiplier, heightOffset, physicalSize);
        }
    }
}
