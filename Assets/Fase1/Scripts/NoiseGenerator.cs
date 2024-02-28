using UnityEngine;

namespace Fase1
{
    public class NoiseGenerator
    {
        private readonly float _scale;
        private readonly int _xOffset;
        private readonly int _yOffset;
        private readonly int _vertices;
        private readonly float _heightMultiplier;
        private readonly float _heightOffset;

        public NoiseGenerator(float scale, int xOffset, int yOffset, int vertices, float heightMultiplier, float heightOffset)
        {
            this._vertices = vertices;
            this._yOffset = yOffset;
            this._xOffset = xOffset;
            this._scale = scale;
            this._heightMultiplier = heightMultiplier;
            this._heightOffset = heightOffset;
        }


        public float[,] GenerateNoiseChunk(int xChunk, int yChunk)
        {

            var noiseMap = new float[_vertices,_vertices];
            for (int x = 0; x < _vertices; x++)
            {
                for (int y = 0; y < _vertices; y++)
                {
                    int xPos = (xChunk * _vertices) +  x - xChunk;
                    int yPos = (yChunk * _vertices) +y - yChunk;
                    if (xChunk == 0)
                    {
                        xPos = x;
                    }
                    if (yChunk == 0)
                    {
                        yPos = y;
                    }
                    
                    noiseMap[x, y] = Mathf.PerlinNoise(xPos * _scale + _xOffset, yPos * _scale + _yOffset) * _heightMultiplier - _heightOffset;

                }
            }

            return noiseMap;
        }
    }
}
