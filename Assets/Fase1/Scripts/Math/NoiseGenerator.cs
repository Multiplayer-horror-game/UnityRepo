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

        public NoiseGenerator(float scale, int xOffset, int yOffset, int vertices, float heightMultiplier, float heightOffset)
        {
            this.vertices = vertices;
            this.yOffset = yOffset;
            this.xOffset = xOffset;
            this.Scale = scale;
            this.heightMultiplier = heightMultiplier;
            this.heightOffset = heightOffset;
        }

        public float[,] GenerateNoiseChunk(int xChunk, int yChunk)
        {
            return GenerateNoiseChunk(xChunk, yChunk, this.xOffset, this.yOffset);
        }


        private float[,] GenerateNoiseChunk(int xChunk, int yChunk, float _xOffset, float _yOffset)
        {

            var noiseMap = new float[vertices,vertices];
            for (int x = 0; x < vertices; x++)
            {
                for (int y = 0; y < vertices; y++)
                {
                    int xPos = (xChunk * vertices) +  x - xChunk;
                    int yPos = (yChunk * vertices) +y - yChunk;
                    if (xChunk == 0)
                    {
                        xPos = x;
                    }
                    if (yChunk == 0)
                    {
                        yPos = y;
                    }
                    
                    noiseMap[x, y] = Mathf.PerlinNoise(xPos * Scale + _xOffset, yPos * Scale + _yOffset) * heightMultiplier - heightOffset;

                }
            }

            return noiseMap;
        }

        public float GetNoiseValue(float x, float y)
        {
            
            return Mathf.PerlinNoise(x * Scale / 10 + xOffset, y * Scale / 10 + yOffset) * heightMultiplier - heightOffset;
        }
        
        public NatureNoiseGenerator ConvertToNatureNoiseGenerator()
        {
            return new NatureNoiseGenerator(Scale,xOffset, yOffset,vertices, heightMultiplier, heightOffset);
        }
    }
}
